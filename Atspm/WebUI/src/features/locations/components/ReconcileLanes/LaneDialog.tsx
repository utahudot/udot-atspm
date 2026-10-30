import { getDeviceActiveDevicesByLocationFromLocationId } from '@/api/config/device/device'
import {
  groupTmcSources,
  normalizeTmcDevices,
} from '@/features/charts/turningMovementCounts/useTmcSources'
import { reportsRequest } from '@/lib/axios'
import {
  Alert,
  Box,
  Button,
  DialogActions,
  DialogContent,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { format, startOfDay, subDays } from 'date-fns'
import { useMemo, useState } from 'react'
import { useQuery } from 'react-query'
import DiscrepancyButton from '../ApproachesReconcilationReport/DiscrepancyButton'
import { useLocationStore } from '../editLocation/locationStore'
import { DetectorReviewEditor } from './DetectorReviewEditor'
import { readDeviceZones } from './laneDecoders'
import {
  applyLaneRecommendations,
  laneChangesConflict,
  LaneReview,
  movementLabels,
  originalLaneChange,
  Recommendation,
  reconcileLanes,
  suggestLaneChannels,
  ZoneEvidence,
  zoneEvidenceLabel,
  ZoneInventory,
} from './reconcileLanes'

export function LaneDialog({
  onClose,
  onBusyChange,
}: {
  onClose: () => void
  onBusyChange?: (busy: boolean) => void
}) {
  const location = useLocationStore((s) => s.location)!
  const approaches = useLocationStore((s) => s.approaches)
  const [selectedDecoder, setDecoder] = useState('')
  const [start, setStart] = useState(
    format(subDays(startOfDay(new Date()), 3), "yyyy-MM-dd'T'HH:mm")
  )
  const [end, setEnd] = useState(
    format(startOfDay(new Date()), "yyyy-MM-dd'T'HH:mm")
  )
  const [minimum, setMinimum] = useState(25)
  const [review, setReview] = useState<LaneReview>()
  const [evidence, setEvidence] = useState<ZoneEvidence[]>([])
  const [snapshot, setSnapshot] = useState('')
  const [selected, setSelected] = useState<string[]>([])
  const [channels, setChannels] = useState<Record<string, string[]>>({})
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [applied, setApplied] = useState(false)
  const devicesQuery = useQuery(['lane-review-devices', location.id], () =>
    getDeviceActiveDevicesByLocationFromLocationId(location.id)
  )
  const devices = useMemo(
    () => normalizeTmcDevices(devicesQuery.data),
    [devicesQuery.data]
  )
  const sources = groupTmcSources(devices).filter(
    (s) => s.decoder
  )
  // Use the same configured decoder groups as TMC. A single supported source needs no choice.
  const decoder =
    sources.length === 1
      ? sources[0].decoder!
      : sources.some((s) => s.decoder === selectedDecoder)
        ? selectedDecoder
        : ''
  const inputs = useMemo(
    () =>
      JSON.stringify({
        location,
        decoder,
        start,
        end,
        minimum,
        approaches,
        devices,
      }),
    [location, decoder, start, end, minimum, approaches, devices]
  )
  const stale = snapshot !== inputs
  async function run() {
    setBusy(true)
    onBusyChange?.(true)
    setError('')
    setReview(undefined)
    setEvidence([])
    setApplied(false)
    setSelected([])
    setChannels({})
    try {
      if (
        !decoder ||
        !start ||
        !end ||
        end <= start ||
        !Number.isInteger(minimum) ||
        minimum < 1
      )
        throw new Error(
          'Choose a decoder, valid date range and positive minimum count.'
        )
      const source = sources.find((s) => s.decoder === decoder)
      if (!source)
        throw new Error('No configured device supports this decoder.')
      const chosen = devices
        .filter((d) => source.deviceIds.includes(d.id!))
        .sort((a, b) => a.id! - b.id!)
      const result = await reportsRequest<{
        zoneEvidence: ZoneEvidence[]
        sourceDeviceIds: number[]
        warnings: string[]
      }>({
        url: '/api/v1/TurningMovementCounts/getReportData',
        method: 'POST',
        data: {
          locationIdentifier: location.locationIdentifier,
          source: 'devices',
          deviceIds: chosen.map((d) => d.id),
          decoder,
          start,
          end,
          binSize: 15,
          reconcileLanes: true,
        },
      })
      if (
        !Array.isArray(result.zoneEvidence) ||
        !Array.isArray(result.sourceDeviceIds) ||
        result.sourceDeviceIds.length === 0 ||
        result.sourceDeviceIds.some((id) => !chosen.some((d) => d.id === id))
      )
        throw new Error(
          'The report API must be updated to support zone evidence.'
        )
      // The decoder resolves index/serial aliases. Use its selection for inventory too,
      // including cameras with no returned bins; evidence alone is not an inventory.
      const inventory: ZoneInventory[] = await Promise.all(
        chosen
          .filter((d) => result.sourceDeviceIds.includes(d.id!))
          .map(async (d) => {
            try {
              return { deviceId: d.id!, names: await readDeviceZones(d.id!) }
            } catch (error) {
              const data = (error as { response?: { data?: string | { error?: { message?: string } } } }).response?.data
              const detail = typeof data === 'string' ? data : data?.error?.message
              return { deviceId: d.id!, names: [], error: detail ? `Get Zones failed: ${detail}` : 'Get Zones failed; verify the device connection and compatible zone provider.' }
            }
          })
      )
      setEvidence(result.zoneEvidence)
      const analysis = reconcileLanes(
        approaches,
        inventory,
        result.zoneEvidence,
        minimum,
        // Detectors have no device owner. An absent zone in this source is not
        // evidence that a detector from another configured source is misassigned.
        // Include devices without TmcDecoder: grouping sources omits them, but
        // they may still own configured zones. Unknown ownership needs review.
        devices.every(
          (device) => device.id != null && source.deviceIds.includes(device.id)
        )
      )
      // Partial responses are useful for diagnosis but cannot safely justify automatic edits.
      if (result.warnings?.length) {
        analysis.warnings.unshift(
          ...result.warnings,
          'Source warnings are present. Resolve them and rerun before applying recommendations.'
        )
        analysis.recommendations = []
      }
      setChannels(suggestLaneChannels(approaches, analysis.recommendations))
      setReview(analysis)
      setSnapshot(inputs)
    } catch (e) {
      const response = (e as { response?: { data?: unknown } }).response?.data
      setError(
        typeof response === 'string'
          ? response
          : e instanceof Error
            ? e.message
            : 'Unable to reconcile lanes.'
      )
    } finally {
      setBusy(false)
      onBusyChange?.(false)
    }
  }
  function apply() {
    try {
      if (stale)
        throw new Error(
          'Configuration or review inputs changed. Run reconciliation again.'
        )
      const next = applyLaneRecommendations(
        approaches,
        review!.recommendations.filter((r) => selected.includes(r.id)),
        Object.fromEntries(
          Object.entries(channels).map(([key, value]) => [
            key,
            value.map(Number),
          ])
        )
      )
      useLocationStore.setState({
        approaches: next,
        channelMap: new Map(
          next.flatMap((a) =>
            a.detectors.map(
              (d) => [d.id, Number(d.detectorChannel)] as [number, number]
            )
          )
        ),
      })
      setApplied(true)
      setSelected([])
      setReview(undefined)
      setError('')
    } catch (e) {
      setError((e as Error).message)
    }
  }
  function renderRecommendation(r: Recommendation) {
    const zone = approaches
      .flatMap((a) => a.detectors)
      .find((d) => d.id === r.detectorId)?.dectectorIdentifier
    const preview = originalLaneChange(approaches, r)
    const enteredChannels = channels[r.id] ?? []
    const label =
      !r.warning && r.movements
        ? r.movements
            .slice(1)
            .map(
              (movement, i) =>
                `Add Channel ${enteredChannels[i] || '[enter channel]'} zone ${zone} ${movementLabels[movement] ?? movement}`
            )
            .join('; ')
        : r.description
    return (
      <Box
        key={r.id}
        sx={{
          display: 'grid',
          gridTemplateColumns:
            r.movements && r.movements.length > 1
              ? 'minmax(0, 1fr) 100px'
              : '1fr',
          alignItems: 'center',
          gap: 1,
        }}
      >
        <DiscrepancyButton
          item={{ id: r.id, label }}
          width="auto"
          selected={selected.includes(r.id)}
          onToggle={() =>
            setSelected(
              selected.includes(r.id)
                ? selected.filter((id) => id !== r.id)
                : [
                    ...selected.filter(
                      (id) =>
                        !review!.recommendations.some(
                          (other) =>
                            other.id === id && laneChangesConflict(other, r)
                        )
                    ),
                    r.id,
                  ]
            )
          }
        />
        <Stack spacing={1}>
          {r.movements?.slice(1).map((movement, i) => (
            <TextField
              key={`${r.id}-${movement}`}
              size="small"
              type="number"
              inputProps={{
                min: 65,
                step: 1,
                'aria-label': `Channel for ${zone} ${movementLabels[movement] ?? movement}`,
              }}
              value={enteredChannels[i] ?? ''}
              onChange={(e) =>
                setChannels((current) => ({
                  ...current,
                  [r.id]: (current[r.id] ?? []).map((value, index) =>
                    index === i ? e.target.value : value
                  ),
                }))
              }
            />
          ))}
        </Stack>
        {preview && (
          <Typography variant="body2" sx={{ gridColumn: '1 / -1' }}>
            {preview}
          </Typography>
        )}
      </Box>
    )
  }
  const content = (
    <>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Alert severity="info">
            Compare the existing location configuration with independent device
            zones. Any template was already applied when the location was
            created. Channels 1–64 are excluded. Counts use the same location
            timezone as TMC reports. A count indicates an observed movement, not
            whether that turn is legally allowed.
          </Alert>
          <TextField
            select
            label="Decoder"
            value={decoder}
            onChange={(e) => setDecoder(e.target.value)}
            disabled={busy || sources.length === 1}
          >
            {sources.map((s) => (
              <MenuItem key={s.key} value={s.decoder}>
                {s.name}
              </MenuItem>
            ))}
          </TextField>
          {!devicesQuery.isLoading && sources.length === 0 && (
            <Alert severity="warning">
              No configured decoder with lane reconciliation support was found.
            </Alert>
          )}
          {devicesQuery.isError && (
            <Alert severity="error">
              Configuration could not be loaded. Close and reopen to retry.
            </Alert>
          )}
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Start (location time)"
              type="datetime-local"
              value={start}
              onChange={(e) => setStart(e.target.value)}
              InputLabelProps={{ shrink: true }}
              disabled={busy}
            />
            <TextField
              label="End (exclusive)"
              type="datetime-local"
              value={end}
              onChange={(e) => setEnd(e.target.value)}
              InputLabelProps={{ shrink: true }}
              disabled={busy}
            />
            <TextField
              label="Minimum observed count"
              type="number"
              value={minimum}
              onChange={(e) => setMinimum(Number(e.target.value))}
              disabled={busy}
              inputProps={{ min: 1 }}
            />
          </Box>
          {error && <Alert severity="error">{error}</Alert>}
          {applied && (
            <Alert severity="success">
              Changes staged in the location editor. Review and save the
              location to persist them.
            </Alert>
          )}
          <Button variant="contained" disabled={busy || !decoder} onClick={run}>
            {busy ? 'Reading zones and statistics…' : 'Compare lanes'}
          </Button>
          {review && (
            <>
              {stale && (
                <Alert severity="warning">
                  Inputs or configuration changed. Compare again before
                  applying.
                </Alert>
              )}
              <Typography variant="h6">
                Recommended lane changes (
                {review.recommendations.filter((r) => !r.warning).length})
              </Typography>
              <Typography>
                Choose additions or removals after verifying lane use. Nothing
                is selected automatically. Removal affects the configured lane
                entry, not the camera zone. Apply stages your choices; save the
                approach to persist them.
              </Typography>
              {review.recommendations.every((r) => r.warning) && (
                <Typography>
                  No lane additions are recommended. Review warnings below for
                  optional removals.
                </Typography>
              )}
              {review.recommendations.some((r) => !r.warning) && (
                <Box>
                  <Typography
                    variant="body2"
                    sx={{ width: 100, ml: 'auto', mb: 0.5 }}
                  >
                    Channel
                  </Typography>
                  <Stack spacing={1}>
                    {review.recommendations
                      .filter((r) => !r.warning)
                      .map(renderRecommendation)}
                  </Stack>
                </Box>
              )}
              <Typography variant="h6">
                Warnings — verify manually ({review.warnings.length})
              </Typography>
              {review.warnings.map((warning, i) => (
                <Alert key={i} severity="warning">
                  {warning}
                  {review.manualActions.filter((action) => action.warning === warning).map((action, index) => (
                    <Box key={index}>
                      {action.detectorIds.map((id) => <DetectorReviewEditor key={id} zone={action.zone} detectorId={id} allowRemove={action.remove} disabled={busy || stale}
                        onAdded={() => { setReview(undefined); setSelected([]); setApplied(true) }} />)}
                      {action.add && <DetectorReviewEditor zone={action.zone} disabled={busy || stale}
                        onAdded={() => { setReview(undefined); setSelected([]); setApplied(true) }} />}
                    </Box>
                  ))}
                  {review.unusedZones.filter((zone) => zone.warning === warning).map((zone) => (
                    <DetectorReviewEditor key={zone.name} zone={zone.name} disabled={busy || stale}
                      onAdded={() => { setReview(undefined); setSelected([]); setApplied(true) }} />
                  ))}
                  {review.recommendations
                    .filter((r) => r.warning === warning)
                    .map(renderRecommendation)}
                </Alert>
              ))}
              <Typography variant="h6">Observed zone counts</Typography>
              <Box
                sx={{
                  overflowX: 'auto',
                  '& th, & td': { p: 1, textAlign: 'left' },
                }}
              >
                <table aria-label="Observed zone counts">
                  <thead>
                    <tr>
                      <th>Device</th>
                      <th>Zone</th>
                      <th>Through</th>
                      <th>Left</th>
                      <th>Right</th>
                      <th>Bins returned</th>
                    </tr>
                  </thead>
                  <tbody>
                    {evidence.map((e) => (
                      <tr key={`${e.deviceId}:${e.zoneId}:${e.zoneName}`}>
                        <td>{e.deviceId}</td>
                        <td>{zoneEvidenceLabel(e)}</td>
                        <td>{e.through}</td>
                        <td>{e.left}</td>
                        <td>{e.right}</td>
                        <td>{e.bins}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </Box>
              <Typography variant="caption">
                Counts cover returned bins only. Missing bins and zero counts do
                not prove that a movement is unavailable. Bin statistics cannot
                determine protected versus permissive operation.
              </Typography>
            </>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={busy}>
          Close
        </Button>
        <Button
          variant="contained"
          onClick={apply}
          disabled={busy || stale || selected.length === 0}
        >
          Apply selected to editor
        </Button>
      </DialogActions>
    </>
  )
  return content
}
