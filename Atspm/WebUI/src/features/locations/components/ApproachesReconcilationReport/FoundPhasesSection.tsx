import DiscrepancySectionCard from '@/features/locations/components/ApproachesReconcilationReport/DiscrepancySectionCard'
import { useNotificationStore } from '@/stores/notifications'
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
  useTheme,
} from '@mui/material'
import React, { useEffect, useMemo, useState } from 'react'
import { NavigationProvider } from '../Cell/CellNavigation'
import { ApproachEditorGrid } from '../editApproach/ApproachEditorGrid'
import type { ConfigApproach } from '../editLocation/locationStore'
import DiscrepancyRow, { DiscrepancyItem } from './DiscrepancyRow'
import type { ItemStatus } from './useDiscrepancyStatuses'

export default function FoundPhasesSection({
  items,
  itemStatuses,
  updateStatus,
  selected,
  setSelected,
  addApproach,
  title = 'Phases',
  isOverlap = false,
  pedsAre1to1 = false,
  approaches = [],
  updateApproach,
}: {
  items: DiscrepancyItem[]
  itemStatuses: Record<string, ItemStatus>
  updateStatus: (id: string, status: ItemStatus) => void
  selected: Record<string, boolean>
  setSelected: React.Dispatch<React.SetStateAction<Record<string, boolean>>>
  addApproach: (phase?: number, properties?: Partial<ConfigApproach>) => void
  isOverlap?: boolean
  pedsAre1to1?: boolean
  approaches?: ConfigApproach[]
  updateApproach?: (approach: ConfigApproach) => void
  title?: string
}) {
  const [drafts, setDrafts] = useState<ConfigApproach[]>([])
  const [targets, setTargets] = useState<Record<number, number>>({})
  const normalizePhase = (value: unknown) =>
    value === '' || value == null ? null : Number(value)
  const validDraft = (draft: ConfigApproach, phase: number) => {
    const protectedPhase = normalizePhase(draft.protectedPhaseNumber)
    const permissivePhase = normalizePhase(draft.permissivePhaseNumber)
    const pedestrianPhase = normalizePhase(draft.pedestrianPhaseNumber)
    return (
      protectedPhase !== null &&
      [protectedPhase, permissivePhase, pedestrianPhase].every(
        (v) => v === null || (Number.isInteger(v) && v >= 0)
      ) &&
      ((protectedPhase === phase &&
        Boolean(draft.isProtectedPhaseOverlap) === isOverlap) ||
        (permissivePhase === phase &&
          Boolean(draft.isPermissivePhaseOverlap) === isOverlap))
    )
  }
  const theme = useTheme()
  const { addNotification } = useNotificationStore()

  const [availableItems, setAvailableItems] = useState<DiscrepancyItem[]>(items)

  useEffect(() => {
    setAvailableItems(items)
  }, [items])

  const toggle = (key: string) => setSelected((p) => ({ ...p, [key]: !p[key] }))
  const clear = () => setSelected({})

  const selectedItems = useMemo(
    () => availableItems.filter((it) => !!selected[it.id.toString()]),
    [availableItems, selected]
  )

  const addSelected = () => {
    if (!selectedItems.length) return

    setTargets({})
    setDrafts(
      selectedItems.map(
        (item, index) =>
          ({
            id: index + 1,
            description: 'New Approach',
            isNew: true,
            detectors: [],
            protectedPhaseNumber: Number(item.label),
            isProtectedPhaseOverlap: isOverlap,
            permissivePhaseNumber: null,
            pedestrianPhaseNumber: null,
            pedestrianDetectors: '',
            isPermissivePhaseOverlap: false,
            isPedestrianPhaseOverlap: false,
            directionTypeId: 'NA',
          }) as unknown as ConfigApproach
      )
    )
  }

  const applyDrafts = () => {
    if (
      !drafts.length ||
      drafts.some(
        (draft, index) => !validDraft(draft, Number(selectedItems[index].label))
      )
    )
      return
    try {
      const phasesAdded: number[] = []

      for (const [index, it] of selectedItems.entries()) {
        const phase = Number(it.label)
        if (!Number.isFinite(phase)) continue

        const draft = drafts[index]
        const properties = {
          ...draft,
          protectedPhaseNumber: normalizePhase(draft.protectedPhaseNumber),
          permissivePhaseNumber: normalizePhase(draft.permissivePhaseNumber),
          pedestrianPhaseNumber:
            pedsAre1to1 &&
            !draft.isProtectedPhaseOverlap &&
            Number(draft.protectedPhaseNumber) > 0
              ? normalizePhase(draft.protectedPhaseNumber)
              : normalizePhase(draft.pedestrianPhaseNumber),
          pedestrianDetectors:
            pedsAre1to1 &&
            !draft.isProtectedPhaseOverlap &&
            Number(draft.protectedPhaseNumber) > 0
              ? String(draft.protectedPhaseNumber)
              : draft.pedestrianDetectors,
        }
        if (targets[index] && updateApproach) {
          const current = approaches.find((a) => a.id === targets[index])
          if (!current) return
          updateApproach({
            ...properties,
            id: current.id,
            detectors: current.detectors,
          })
        } else addApproach(phase, properties)
        phasesAdded.push(phase)
        updateStatus(it.id.toString(), 'added')
      }

      if (phasesAdded.length) {
        setAvailableItems((prev) =>
          prev.filter((it) => !phasesAdded.includes(Number(it.label)))
        )
      }

      addNotification({
        title: `Staged ${selectedItems.length} approach change(s)`,
        type: 'success',
      })
      clear()
      setDrafts([])
    } catch (err) {
      console.error(err)
      addNotification({ title: 'Error adding approaches', type: 'error' })
    }
  }

  return (
    <>
      <Paper
        variant="outlined"
        sx={{ p: 2, mb: 2, backgroundColor: theme.palette.grey[50] }}
      >
        <DiscrepancySectionCard
          title={title}
          actions={
            <>
              <Button
                size="small"
                variant="outlined"
                disabled={selectedItems.length === 0}
                onClick={addSelected}
                sx={{ height: 28 }}
              >
                Add Selected ({selectedItems.length})
              </Button>
              <Button
                size="small"
                variant="text"
                disabled={selectedItems.length === 0}
                onClick={clear}
                sx={{ height: 28 }}
              >
                Clear
              </Button>
            </>
          }
        >
          <DiscrepancyRow
            items={availableItems}
            itemStatuses={itemStatuses}
            isSelected={(it) => !!selected[it.id.toString()]}
            onToggleSelected={(it) => toggle(it.id.toString())}
          />
        </DiscrepancySectionCard>
      </Paper>
      <Dialog
        open={drafts.length > 0}
        onClose={() => setDrafts([])}
        maxWidth="xl"
        fullWidth
      >
        <DialogTitle>Configure approaches before adding</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>
            Review each approach before adding it to the configuration.
          </Typography>
          <NavigationProvider>
            <Stack spacing={2}>
              {drafts.map((draft, index) => (
                <Box key={index}>
                  <Typography variant="subtitle1">
                    {isOverlap ? 'Overlap' : 'Phase'}{' '}
                    {selectedItems[index]?.label}
                  </Typography>
                  {isOverlap && updateApproach && (
                    <TextField
                      select
                      label="Add overlap to"
                      fullWidth
                      sx={{ my: 1 }}
                      value={targets[index] ?? 0}
                      onChange={(event) => {
                        const id = Number(event.target.value)
                        if (!id) {
                          setTargets((current) => ({ ...current, [index]: 0 }))
                          setDrafts((current) =>
                            current.map((a, i) =>
                              i === index
                                ? ({
                                    id: index + 1,
                                    description: 'New Approach',
                                    isNew: true,
                                    detectors: [],
                                    protectedPhaseNumber: Number(
                                      selectedItems[index].label
                                    ),
                                    isProtectedPhaseOverlap: true,
                                    permissivePhaseNumber: null,
                                    pedestrianPhaseNumber: null,
                                    directionTypeId: 'NA',
                                  } as unknown as ConfigApproach)
                                : a
                            )
                          )
                          return
                        }
                        const existing = approaches.find((a) => a.id === id)
                        if (!existing) return
                        const next = { ...existing }
                        const overlap = Number(selectedItems[index].label)
                        if (!next.permissivePhaseNumber) {
                          next.permissivePhaseNumber = overlap
                          next.isPermissivePhaseOverlap = true
                        } else if (!next.protectedPhaseNumber) {
                          next.protectedPhaseNumber = overlap
                          next.isProtectedPhaseOverlap = true
                        }
                        setTargets((current) => ({ ...current, [index]: id }))
                        setDrafts((current) =>
                          current.map((a, i) => (i === index ? next : a))
                        )
                      }}
                    >
                      <MenuItem value={0}>New approach</MenuItem>
                      {approaches.map((a) => (
                        <MenuItem
                          key={a.id}
                          value={a.id}
                          disabled={Object.entries(targets).some(
                            ([i, id]) => Number(i) !== index && id === a.id
                          )}
                        >
                          {a.description || `Approach ${a.id}`}
                        </MenuItem>
                      ))}
                    </TextField>
                  )}
                  {!validDraft(draft, Number(selectedItems[index]?.label)) && (
                    <Alert severity="error">
                      Enter a protected phase number (zero is allowed) and
                      retain the observed {isOverlap ? 'overlap' : 'phase'} as
                      protected or permissive. Phase numbers must be nonnegative
                      whole numbers.
                    </Alert>
                  )}
                  <ApproachEditorGrid
                    approach={draft}
                    pedsAre1to1={
                      pedsAre1to1 &&
                      !draft.isProtectedPhaseOverlap &&
                      Number(draft.protectedPhaseNumber) > 0
                    }
                    updateApproach={(updated) =>
                      setDrafts((current) =>
                        current.map((a, i) => (i === index ? updated : a))
                      )
                    }
                  />
                </Box>
              ))}
            </Stack>
          </NavigationProvider>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDrafts([])}>Cancel</Button>
          <Button
            variant="contained"
            disabled={drafts.some(
              (draft, index) =>
                !validDraft(draft, Number(selectedItems[index]?.label))
            )}
            onClick={applyDrafts}
          >
            {Object.values(targets).some(Boolean)
              ? 'Apply approach changes'
              : 'Add approaches'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
