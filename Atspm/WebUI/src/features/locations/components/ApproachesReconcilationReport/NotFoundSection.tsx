import DeleteModal from '@/components/AdminTable/DeleteModal'
import { useNotificationStore } from '@/stores/notifications'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import React, { useMemo, useState } from 'react'
import { NavigationProvider } from '../Cell/CellNavigation'
import { ApproachEditorGrid } from '../editApproach/ApproachEditorGrid'
import type { ConfigApproach } from '../editLocation/locationStore'
import { useLocationWizardStore } from '../LocationSetupWizard/locationSetupWizardStore'
import { parseApproachIdFromKey } from './discrepancyKeys'
import DiscrepancyRow, { type DiscrepancyItem } from './DiscrepancyRow'
import DiscrepancySectionCard from './DiscrepancySectionCard'
import {
  assignmentFields,
  assignmentKey,
  type ItemStatus,
  type MissingPhaseAssignment,
} from './useDiscrepancyStatuses'

export default function NotFoundSection({
  approaches,
  notFoundApproaches,
  notFoundDetectors,
  missingAssignments,
  itemStatuses,
  updateStatus,
  selected,
  setSelected,
  updateApproach,
  deleteApproach,
  deleteDetector,
  onIgnore,
  pedsAre1to1 = false,
  errors,
}: {
  approaches: ConfigApproach[]
  notFoundApproaches: DiscrepancyItem[]
  notFoundDetectors: DiscrepancyItem[]
  missingAssignments: MissingPhaseAssignment[]
  itemStatuses: Record<string, ItemStatus>
  updateStatus: (id: string, status: ItemStatus) => void
  selected: Record<string, boolean>
  setSelected: React.Dispatch<React.SetStateAction<Record<string, boolean>>>
  updateApproach: (approach: ConfigApproach) => void
  deleteApproach: (approach: ConfigApproach) => void
  deleteDetector: (id: number) => void
  errors?: import('../editLocation/locationStore').LocationStore['errors']
  pedsAre1to1?: boolean
  onIgnore?: () => void
}) {
  const { addNotification } = useNotificationStore()
  const { badApproaches, badDetectors, setBadApproaches, setBadDetectors } =
    useLocationWizardStore()
  const [confirmRemoval, setConfirmRemoval] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<ConfigApproach | null>(null)
  const assignments = useMemo(
    () =>
      missingAssignments
        .filter((a) => assignmentFields[a.role])
        .map((a) => ({
          id: assignmentKey(a),
          kind: 'NOT_FOUND_ASSIGNMENT',
          label: `${approaches.find((p) => p.id === a.approachId)?.description || `Approach ${a.approachId}`} - ${a.role} ${a.isOverlap ? 'overlap' : 'phase'} ${a.phaseNumber}`,
          assignment: a,
        })),
    [missingAssignments, approaches]
  )
  // A retained approach needs a vehicle phase or overlap. Pedestrian-only
  // assignments do not replace its protected/permissive vehicle configuration.
  const retainsVehicleAssignment = (
    approach: ConfigApproach,
    removedRoles: string[]
  ) =>
    (['protected', 'permissive'] as const).some(
      (role) =>
        !removedRoles.includes(role) &&
        Number(approach[assignmentFields[role][0]]) > 0
    )
  const canRemoveAssignment = (item: (typeof assignments)[number]) => {
    const approach = approaches.find((a) => a.id === item.assignment.approachId)
    return (
      !!approach && retainsVehicleAssignment(approach, [item.assignment.role])
    )
  }
  const allItems = [
    ...assignments.filter(canRemoveAssignment),
    ...notFoundDetectors,
  ]
  const pending = (item: DiscrepancyItem) =>
    ['pending', 'unsaved'].includes(
      itemStatuses[item.id.toString()] ?? 'pending'
    )
  const selectable = allItems.filter(pending)
  const selectedItems = selectable.filter((i) => selected[i.id.toString()])
  const validSelection = approaches.every((approach) => {
    const roles = assignments
      .filter((item) => selected[item.id] && pending(item))
      .filter((item) => item.assignment.approachId === approach.id)
      .map((item) => item.assignment.role)
    return !roles.length || retainsVehicleAssignment(approach, roles)
  })
  const clear = () => setSelected({})
  const toggle = (id: string) =>
    setSelected((previous) => ({ ...previous, [id]: !previous[id] }))
  const candidates = approaches.filter(
    (a) =>
      missingAssignments.some((m) => m.approachId === a.id) ||
      notFoundApproaches.some(
        (i) => parseApproachIdFromKey(i.id.toString()) === a.id
      )
  )

  const removeSelected = () => {
    if (!validSelection) return
    // Build one update per approach so selecting multiple missing fields cannot
    // overwrite an earlier removal. Verify the evidence still matches the draft.
    const updates = new Map<number, ConfigApproach>()
    for (const item of assignments.filter(
      (i) => selected[i.id] && pending(i)
    )) {
      const a = item.assignment
      const target =
        updates.get(a.approachId) ??
        approaches.find((p) => p.id === a.approachId)
      const [numberField, overlapField] = assignmentFields[a.role]
      if (
        !target ||
        Number(target[numberField]) !== a.phaseNumber ||
        Boolean(target[overlapField]) !== a.isOverlap
      )
        continue
      updates.set(target.id, {
        ...target,
        // Protected phase is required by the existing editor/API; zero means unassigned.
        [numberField]: a.role === 'protected' ? 0 : null,
        [overlapField]: false,
      })
      updateStatus(item.id, 'deleted')
    }
    for (const updated of updates.values()) updateApproach(updated)
    for (const item of selectedItems.filter(
      (i) => i.kind === 'NOT_FOUND_DET'
    )) {
      const detector = approaches
        .flatMap((a) => a.detectors)
        .find((d) => String(d.detectorChannel) === String(item.label))
      if (detector) {
        deleteDetector(detector.id)
        updateStatus(String(item.id), 'deleted')
      }
    }
    resolveSelectedWarnings()
    clear()
    setConfirmRemoval(false)
    addNotification({
      title: selectedItems.some((i) => i.kind === 'NOT_FOUND_DET')
        ? 'Detector deletions requested. Save any phase or overlap changes separately.'
        : 'Removed selected assignments from the draft. Save to persist changes.',
      type: 'success',
    })
  }

  const resolveSelectedWarnings = () => {
    if (onIgnore) onIgnore()
    else {
      const resolved = new Set(
        assignments
          .filter((i) => selected[i.id] || !pending(i))
          .map((i) => i.id)
      )
      setBadApproaches(
        badApproaches.filter(
          (id) =>
            !missingAssignments.some((a) => a.approachId === id) ||
            assignments.some(
              (i) => i.assignment.approachId === id && !resolved.has(i.id)
            )
        )
      )
      const ignoredChannels = new Set(
        selectedItems
          .filter((i) => i.kind === 'NOT_FOUND_DET')
          .map((i) => String(i.label))
      )
      setBadDetectors(
        badDetectors.filter((c) => !ignoredChannels.has(String(c)))
      )
    }
  }
  const ignoreSelected = () => {
    for (const item of selectedItems) updateStatus(String(item.id), 'ignored')
    resolveSelectedWarnings()
    clear()
  }
  const renderGroup = (
    title: string,
    items: DiscrepancyItem[],
    label: string
  ) => (
    <Box component="section" aria-label={label}>
      <DiscrepancySectionCard title={title}>
        <DiscrepancyRow
          items={items}
          itemStatuses={itemStatuses}
          isSelected={(i) => !!selected[String(i.id)]}
          onToggleSelected={(i) => toggle(String(i.id))}
          buttonWidth={260}
        />
      </DiscrepancySectionCard>
    </Box>
  )
  return (
    <>
      <Box sx={{ mb: 2 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 1 }}>
          Not Found
        </Typography>
        <Typography variant="body2">
          No data was found for the following configured assignments. Verify the
          observation period before removing them.
        </Typography>
      </Box>
      <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
        <Stack spacing={2}>
          <Stack direction="row" spacing={1} justifyContent="flex-end">
            <Button
              size="small"
              variant="outlined"
              disabled={!selectable.length}
              onClick={() =>
                selectedItems.length
                  ? clear()
                  : setSelected(
                      Object.fromEntries(
                        selectable.map((i) => [String(i.id), true])
                      )
                    )
              }
            >
              {selectedItems.length ? 'Clear All' : 'Select All'}
            </Button>
            <Button
              size="small"
              variant="outlined"
              color="error"
              disabled={!selectedItems.length || !validSelection}
              onClick={() => setConfirmRemoval(true)}
            >
              Remove selected
            </Button>
            <Button
              size="small"
              variant="outlined"
              disabled={!selectedItems.length}
              onClick={ignoreSelected}
            >
              Ignore Selected
            </Button>
          </Stack>
          {!validSelection && (
            <Alert severity="warning">
              This selection would leave an approach without a vehicle phase or
              overlap. Keep an assignment or use Delete entire approach.
            </Alert>
          )}
          {candidates.map((approach) => (
            <Paper
              key={approach.id}
              component="section"
              aria-label={`Current configuration: ${approach.description || approach.id}`}
              variant="outlined"
              sx={{ p: 2 }}
            >
              <Typography variant="h6">
                {approach.description || `Approach ${approach.id}`}
              </Typography>
              <NavigationProvider>
                <ApproachEditorGrid
                  errors={errors}
                  approach={approach}
                  pedsAre1to1={pedsAre1to1}
                  updateApproach={updateApproach}
                />
              </NavigationProvider>
              <Stack spacing={1} sx={{ mt: 2 }}>
                {assignments
                  .filter((item) => {
                    const [numberField, overlapField] =
                      assignmentFields[item.assignment.role]
                    return (
                      item.assignment.approachId === approach.id &&
                      pending(item) &&
                      Number(approach[numberField]) ===
                        item.assignment.phaseNumber &&
                      Boolean(approach[overlapField]) ===
                        item.assignment.isOverlap
                    )
                  })
                  .map((missing) => {
                    const { role, isOverlap, phaseNumber } = missing.assignment
                    const label = `${role} ${isOverlap ? 'overlap' : 'phase'} ${phaseNumber}`
                    return (
                      <Stack
                        key={missing.id}
                        direction="row"
                        spacing={1}
                        alignItems="center"
                        flexWrap="wrap"
                      >
                        {canRemoveAssignment(missing) && (
                          <Checkbox
                            size="small"
                            checked={!!selected[missing.id]}
                            onChange={() => toggle(missing.id)}
                            inputProps={{
                              'aria-label': `Select ${approach.description || approach.id} - ${label}`,
                            }}
                          />
                        )}
                        <Typography variant="body2">
                          Not observed: {label}.
                        </Typography>
                        {canRemoveAssignment(missing) ? (
                          <Button
                            size="small"
                            color="error"
                            variant="outlined"
                            onClick={() => {
                              setSelected({ [missing.id]: true })
                              setConfirmRemoval(true)
                            }}
                          >
                            Remove {label}
                          </Button>
                        ) : (
                          <Typography variant="body2">
                            Required vehicle assignment; delete the entire
                            approach to remove it.
                          </Typography>
                        )}
                      </Stack>
                    )
                  })}
              </Stack>
              <Typography variant="subtitle2" sx={{ mt: 2 }}>
                Configured detectors
              </Typography>
              {approach.detectors.length ? (
                <Table size="small" aria-label="Current detector configuration">
                  <TableHead>
                    <TableRow>
                      <TableCell>Channel</TableCell>
                      <TableCell>Zone</TableCell>
                      <TableCell>Lane</TableCell>
                      <TableCell>Movement</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {approach.detectors.map((detector) => (
                      <TableRow key={detector.id}>
                        <TableCell>{detector.detectorChannel}</TableCell>
                        <TableCell>
                          {detector.dectectorIdentifier || 'Not configured'}
                        </TableCell>
                        <TableCell>
                          {detector.laneNumber ?? 'Not configured'}
                        </TableCell>
                        <TableCell>
                          {detector.movementType || 'Not configured'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <Typography variant="body2">
                  No detectors configured.
                </Typography>
              )}
              <Stack spacing={1} alignItems="flex-start" sx={{ mt: 2 }}>
                <Typography variant="body2">
                  Removing a phase or overlap keeps this approach and its
                  detectors. Deleting the entire approach also deletes all
                  detectors shown above.
                </Typography>
                <Button
                  variant="outlined"
                  color="error"
                  onClick={() => setDeleteTarget(approach)}
                >
                  Delete entire approach: {approach.description || approach.id}
                </Button>
              </Stack>
            </Paper>
          ))}
          {renderGroup(
            'Detector Channels',
            notFoundDetectors,
            'Not Found detector channels'
          )}
        </Stack>
      </Paper>
      <Dialog open={confirmRemoval} onClose={() => setConfirmRemoval(false)}>
        <DialogTitle>Remove selected assignments and detectors?</DialogTitle>
        <DialogContent>
          <Alert severity="info">
            Phase and overlap removal clears only the selected assignment.
            Approaches and their detectors are retained only when a vehicle
            phase or overlap remains. Existing detector deletions are saved
            immediately when explicitly selected. Phase and overlap edits remain
            in the draft until saved.
          </Alert>
          <ul>
            {selectedItems.map((i) => (
              <li key={i.id}>{i.label}</li>
            ))}
          </ul>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmRemoval(false)}>Cancel</Button>
          <Button
            color="error"
            variant="contained"
            disabled={!validSelection}
            onClick={removeSelected}
          >
            Confirm removal
          </Button>
        </DialogActions>
      </Dialog>
      <DeleteModal
        id={deleteTarget?.id ?? 0}
        name={`${deleteTarget?.description || 'this approach'} and all of its detectors`}
        objectType="entire approach"
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => {
          const current = approaches.find((a) => a.id === deleteTarget?.id)
          if (current) deleteApproach(current)
          clear()
        }}
      />
    </>
  )
}

export const getPhaseNumberFromLabel = (label: string | number) => {
  const match = String(label).match(/ph\s*(\d+)/i)
  const value = match ? Number(match[1]) : Number(label)
  return Number.isFinite(value) ? value : Number.POSITIVE_INFINITY
}
