import DiscrepancySectionCard from '@/features/locations/components/ApproachesReconcilationReport/DiscrepancySectionCard'
import { useNotificationStore } from '@/stores/notifications'
import { Box, Button, Paper, Typography, useTheme } from '@mui/material'
import React, { useEffect, useMemo, useState } from 'react'
import DiscrepancyRow, { DiscrepancyItem } from './DiscrepancyRow'
import type { ItemStatus } from './useDiscrepancyStatuses'

export default function FoundPhasesSection({
  items,
  itemStatuses,
  updateStatus,
  selected,
  setSelected,
  addApproach,
}: {
  items: DiscrepancyItem[]
  itemStatuses: Record<string, ItemStatus>
  updateStatus: (id: string, status: ItemStatus) => void
  selected: Record<string, boolean>
  setSelected: React.Dispatch<React.SetStateAction<Record<string, boolean>>>
  addApproach: (phase?: number) => void
}) {
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

    try {
      const phasesAdded: number[] = []

      for (const it of selectedItems) {
        const phase = Number(it.label)
        if (!Number.isFinite(phase)) continue

        addApproach(phase)
        phasesAdded.push(phase)
        updateStatus(it.id.toString(), 'added')
      }

      if (phasesAdded.length) {
        setAvailableItems((prev) =>
          prev.filter((it) => !phasesAdded.includes(Number(it.label)))
        )
      }

      addNotification({
        title: `Added ${selectedItems.length} approach(es)`,
        type: 'success',
      })
      clear()
    } catch (err) {
      console.error(err)
      addNotification({ title: 'Error adding approaches', type: 'error' })
    }
  }

  return (
    <Paper
      variant="outlined"
      sx={{ p: 2, mb: 2, backgroundColor: theme.palette.grey[50] }}
    >
      <Box sx={{ mb: 2 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 1 }}>
          Found
        </Typography>
        <Typography variant="body2" color="textSecondary">
          Data was found for the following unconfigured items
        </Typography>
      </Box>

      <DiscrepancySectionCard
        title="Phases"
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
  )
}
