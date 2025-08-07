import { useGetDetectionType, useGetLocationType } from '@/api/config'
import CalendarCell from '@/features/locations/components/Cell/CalendarCell'
import CommentCell from '@/features/locations/components/Cell/CommentCell'
import { MultiSelectCell } from '@/features/locations/components/Cell/MultiSelectCell'
import SelectCell from '@/features/locations/components/Cell/SelectCell'
import { TextCell } from '@/features/locations/components/Cell/TextCell'
import { hasUniqueDetectorChannels } from '@/features/locations/components/editApproach/utils/checkDetectors'
import {
  hardwareTypeOptions,
  laneTypeOptions,
  movementTypeOptions,
} from '@/features/locations/components/editDetector/selectOptions'
import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
import { DetectionType } from '@/features/locations/types'
import {
  alpha,
  Avatar,
  AvatarGroup,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
  useTheme,
} from '@mui/material'
import { useEffect, useMemo, useState } from 'react'

interface EditDetectorsProps {
  approach: ConfigApproach
  deleteMode: boolean
  onSelectionChange?: (ids: number[]) => void
}

const EditDetectors = ({
  approach,
  deleteMode,
  onSelectionChange,
}: EditDetectorsProps) => {
  const theme = useTheme()
  const errorColor = theme.palette.error.main
  const successColor = theme.palette.success.main
  const innerErrorBg = alpha(errorColor, 0.2)
  const innerSuccessBg = alpha(successColor, 0.2)

  const [selectedIds, setSelectedIds] = useState<number[]>([])
  useEffect(() => {
    if (!deleteMode) setSelectedIds([])
  }, [deleteMode])

  const handleRowClick = (id: number) => {
    if (!deleteMode) return
    setSelectedIds((prev) => {
      const next = prev.includes(id)
        ? prev.filter((x) => x !== id)
        : [...prev, id]
      onSelectionChange?.(next)
      return next
    })
  }

  const { updateDetector, errors, setErrors, warnings, location, channelMap } =
    useLocationStore((s) => ({
      updateDetector: s.updateDetector,
      errors: s.errors,
      setErrors: s.setErrors,
      warnings: s.warnings,
      location: s.location,
      channelMap: s.channelMap,
    }))

  const { data: dtData } = useGetDetectionType()
  const { data: ltData } = useGetLocationType()
  const detectionTypes = useMemo(
    () => (dtData?.value ?? []) as DetectionType[],
    [dtData]
  )
  const locationType = ltData?.value?.find(
    (t) => t.id === location?.locationTypeId
  )
  const detectionOptions = useMemo(
    () =>
      detectionTypes
        .filter((d) => {
          if (locationType?.name === 'Intersection')
            return ['AC', 'AS', 'LLC', 'LLS', 'SBP', 'AP'].includes(
              d.abbreviation
            )
          if (locationType?.name === 'Ramp')
            return ['P', 'D', 'IQ', 'EQ'].includes(d.abbreviation)
          return true
        })
        .map((d) => ({ value: d.abbreviation, label: d.description })),
    [detectionTypes, locationType]
  )

  const detectors = approach.detectors
  const rowCount = detectors.length + 1
  const colCount = 13

  return (
    <TableContainer component={Paper}>
      <Table stickyHeader aria-label="detector table">
        <TableHead>
          <TableRow>
            <TableCell colSpan={9} sx={{ py: 1 }} />
            <TableCell colSpan={2} align="center" sx={{ py: 1 }}>
              <Typography variant="caption" fontStyle="italic">
                Advanced Count Only
              </Typography>
            </TableCell>
            <TableCell colSpan={2} align="center" sx={{ py: 1 }}>
              <Typography variant="caption" fontStyle="italic">
                Advanced Speed Only
              </Typography>
            </TableCell>
          </TableRow>
          <TableRow>
            {[
              'Channel',
              'Detection Types',
              'Hardware',
              'Latency Correction',
              'Lane Number',
              'Movement Type',
              'Lane Type',
              'Date Added',
              'Comments',
              'Distance to Stop Bar',
              'Decision Point',
              'Minimum Speed Filter',
              'Movement Delay',
            ].map((h, i) => (
              <TableCell
                key={i}
                sx={{ py: 1, fontSize: '12px', pl: i === 0 ? 1 : 0 }}
              >
                {h}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>

        <TableBody>
          {detectors.length === 0 ? (
            <TableRow>
              <TableCell colSpan={colCount} sx={{ textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  No detectors found
                </Typography>
              </TableCell>
            </TableRow>
          ) : (
            detectors.map((det, i) => {
              const rowIdx = i + 1
              const isSelected = selectedIds.includes(det.id)
              const outline = isSelected
                ? `2px solid ${errorColor}`
                : det.isNew
                  ? `2px solid ${successColor}`
                  : undefined

              return (
                <TableRow
                  key={det.id}
                  sx={{
                    position: 'relative',
                    cursor: deleteMode ? 'pointer' : 'default',
                    caretColor: 'transparent',
                    outline,
                    outlineOffset: outline ? '-2px' : undefined,
                    borderRadius: outline ? 1 : undefined,
                    bgcolor: isSelected
                      ? innerErrorBg
                      : det.isNew
                        ? innerSuccessBg
                        : undefined,
                  }}
                >
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={0}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.detectorChannel}
                    onUpdate={(v) => {
                      updateDetector(det.id, 'detectorChannel', v)
                      // recompute channel errors for all detectors
                      const { isValid, errors: channelErrors } =
                        hasUniqueDetectorChannels(channelMap)
                      // preserve non-channel errors
                      const prev = errors || {}
                      const nonChannel = Object.fromEntries(
                        Object.entries(prev).filter(
                          ([key]) => !channelMap.has(Number(key))
                        )
                      )
                      if (!isValid) {
                        setErrors({ ...nonChannel, ...channelErrors })
                      } else {
                        setErrors(
                          Object.keys(nonChannel).length ? nonChannel : null
                        )
                      }
                    }}
                    error={errors && errors[det.id]}
                    warning={warnings && warnings[det.id]}
                  />
                  <MultiSelectCell<string>
                    approachId={approach.id}
                    row={rowIdx}
                    col={1}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.detectionTypes.map((dt) => dt.abbreviation)}
                    options={detectionOptions}
                    onUpdate={(abbrs) => {
                      const types = detectionTypes.filter((d) =>
                        abbrs.includes(d.abbreviation)
                      )
                      updateDetector(det.id, 'detectionTypes', types)
                    }}
                    renderValue={(selected) => (
                      <AvatarGroup max={12}>
                        {detectionOptions.map((d) =>
                          selected.includes(d.value) ? (
                            <Tooltip key={d.value} title={d.label}>
                              <Avatar
                                sx={{
                                  bgcolor: theme.palette.primary.main,
                                  width: 26,
                                  height: 26,
                                  fontSize: '11px',
                                }}
                              >
                                {d.value}
                              </Avatar>
                            </Tooltip>
                          ) : (
                            <Avatar
                              key={d.value}
                              sx={{
                                bgcolor: theme.palette.grey[400],
                                width: 26,
                                height: 26,
                                fontSize: '11px',
                              }}
                            >
                              {d.value}
                            </Avatar>
                          )
                        )}
                      </AvatarGroup>
                    )}
                  />
                  <SelectCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={2}
                    rowCount={rowCount}
                    colCount={colCount}
                    options={hardwareTypeOptions}
                    value={det.detectionHardware}
                    onUpdate={(v) =>
                      updateDetector(det.id, 'detectionHardware', v)
                    }
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={3}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.latencyCorrection}
                    onUpdate={(v) =>
                      updateDetector(det.id, 'latencyCorrection', v)
                    }
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={4}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.laneNumber}
                    onUpdate={(v) => updateDetector(det.id, 'laneNumber', v)}
                  />
                  <SelectCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={5}
                    rowCount={rowCount}
                    colCount={colCount}
                    options={movementTypeOptions}
                    value={det.movementType}
                    onUpdate={(v) => updateDetector(det.id, 'movementType', v)}
                  />
                  <SelectCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={6}
                    rowCount={rowCount}
                    colCount={colCount}
                    options={laneTypeOptions}
                    value={det.laneType}
                    onUpdate={(v) => updateDetector(det.id, 'laneType', v)}
                  />
                  <CalendarCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={7}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.dateAdded}
                    onUpdate={(v) => updateDetector(det.id, 'dateAdded', v)}
                  />
                  <CommentCell
                    approachId={approach.id}
                    detector={det}
                    row={rowIdx}
                    col={8}
                    rowCount={rowCount}
                    colCount={colCount}
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={9}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.distanceFromStopBar}
                    onUpdate={(v) =>
                      updateDetector(det.id, 'distanceFromStopBar', v)
                    }
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={10}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.decisionPoint}
                    onUpdate={(v) => updateDetector(det.id, 'decisionPoint', v)}
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={11}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.minSpeedFilter}
                    onUpdate={(v) =>
                      updateDetector(det.id, 'minSpeedFilter', v)
                    }
                  />
                  <TextCell
                    approachId={approach.id}
                    row={rowIdx}
                    col={12}
                    rowCount={rowCount}
                    colCount={colCount}
                    value={det.movementDelay}
                    onUpdate={(v) => updateDetector(det.id, 'movementDelay', v)}
                  />
                  {deleteMode && (
                    <TableCell
                      colSpan={colCount}
                      onClick={() => handleRowClick(det.id)}
                      sx={{
                        p: 0,
                        position: 'absolute',
                        inset: 0,
                        zIndex: 1,
                        bgcolor: 'transparent',
                        cursor: 'pointer',
                      }}
                    />
                  )}
                </TableRow>
              )
            })
          )}
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default EditDetectors
