import DeleteModal from '@/components/AdminTable/DeleteModal'
import { parseApproachIdFromKey } from '@/features/locations/components/ApproachesReconcilationReport/discrepancyKeys'
import DiscrepancySectionCard from '@/features/locations/components/ApproachesReconcilationReport/DiscrepancySectionCard'
import type { ConfigApproach } from '@/features/locations/components/editLocation/locationStore'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { useNotificationStore } from '@/stores/notifications'
import DeleteIcon from '@mui/icons-material/Delete'
import NotificationsPausedIcon from '@mui/icons-material/NotificationsPaused'
import { Box, Button, Paper, Typography, useTheme } from '@mui/material'
import React, { useMemo, useState } from 'react'
import DiscrepancyRow, { DiscrepancyItem } from './DiscrepancyRow'
import type { ItemStatus } from './useDiscrepancyStatuses'

export default function NotFoundSection({
  approaches,
  notFoundApproaches,
  notFoundDetectors,
  itemStatuses,
  updateStatus,
  selected,
  setSelected,
  deleteApproach,
  deleteDetector,
}: {
  approaches: ConfigApproach[]
  notFoundApproaches: DiscrepancyItem[]
  notFoundDetectors: DiscrepancyItem[]
  itemStatuses: Record<string, ItemStatus>
  updateStatus: (id: string, status: ItemStatus) => void
  selected: Record<string, boolean>
  setSelected: React.Dispatch<React.SetStateAction<Record<string, boolean>>>
  deleteApproach: (approach: ConfigApproach) => void
  deleteDetector: (id: number) => void
}) {
  const theme = useTheme()
  const { addNotification } = useNotificationStore()
  const { badApproaches, badDetectors, setBadApproaches, setBadDetectors } =
    useLocationWizardStore()

  const [deleteModalOpen, setDeleteModalOpen] = useState(false)

  const clearSelection = () => setSelected({})

  const toggle = (key: string) => setSelected((p) => ({ ...p, [key]: !p[key] }))

  const allItems = useMemo(
    () => [...notFoundApproaches, ...notFoundDetectors],
    [notFoundApproaches, notFoundDetectors]
  )

  const selectedItems = useMemo(
    () => allItems.filter((it) => !!selected[it.id.toString()]),
    [allItems, selected]
  )

  const selectableItems = useMemo(() => {
    return allItems.filter((it) => {
      const st = itemStatuses[it.id.toString()] ?? 'pending'
      return st !== 'deleted'
    })
  }, [allItems, itemStatuses])

  const deletableSelected = useMemo(() => {
    return selectedItems.filter((it) => {
      const st = itemStatuses[it.id.toString()] ?? 'pending'
      return st !== 'ignored' && st !== 'deleted'
    })
  }, [selectedItems, itemStatuses])

  const deleteItems = (items: DiscrepancyItem[]) => {
    for (const item of items) {
      const key = item.id.toString()
      const st = itemStatuses[key] ?? 'pending'
      if (st === 'ignored' || st === 'deleted') continue

      if (item.kind === 'NOT_FOUND_APP') {
        const approachId = parseApproachIdFromKey(key)
        if (approachId == null) continue
        const target = approaches.find((a) => a.id === approachId)
        if (target) {
          deleteApproach(target)
          updateStatus(key, 'deleted')
        }
      } else if (item.kind === 'NOT_FOUND_DET') {
        const channel = item.label.toString()
        const storeDetectors = approaches.flatMap((a) => a.detectors)
        const target = storeDetectors.find(
          (d) => d.detectorChannel?.toString() === channel
        )
        if (target) {
          deleteDetector(target.id)
          updateStatus(key, 'deleted')
        }
      }
    }
  }

  const confirmDelete = () => {
    try {
      deleteItems(deletableSelected)
      addNotification({
        title: 'Deleted selected discrepancy items',
        type: 'success',
      })
      clearSelection()
    } catch (err) {
      console.error(err)
      addNotification({
        title: 'Error deleting discrepancy items',
        type: 'error',
      })
    } finally {
      setDeleteModalOpen(false)
    }
  }

  const ignoreSelected = () => {
    if (!selectedItems.length) return

    const ignoreApproachIds = new Set<number>()
    const ignoreDetChannels = new Set<string>()

    for (const item of selectedItems) {
      const key = item.id.toString()
      const st = itemStatuses[key] ?? 'pending'
      if (st === 'deleted') continue

      updateStatus(key, 'ignored')

      if (item.kind === 'NOT_FOUND_APP') {
        const id = parseApproachIdFromKey(key)
        if (id != null) ignoreApproachIds.add(id)
      } else if (item.kind === 'NOT_FOUND_DET') {
        ignoreDetChannels.add(item.label.toString())
      }
    }

    if (ignoreApproachIds.size) {
      setBadApproaches(
        badApproaches.filter((approachId) => !ignoreApproachIds.has(approachId))
      )
    }

    if (ignoreDetChannels.size) {
      setBadDetectors(
        badDetectors.filter(
          (detectorChannel) => !ignoreDetChannels.has(detectorChannel)
        )
      )
    }

    clearSelection()
    addNotification({
      title: `Ignored ${selectedItems.length} item(s)`,
      type: 'success',
    })
  }

  const modalName = 'The Selected Discrepancy Items'
  const modalObjectType = 'Items'

  const modalId = 0

  const selectedCount = selectedItems.length

  const selectOrClearAll = () => {
    if (!selectableItems.length) return

    if (selectedCount > 0) {
      clearSelection()
      return
    }

    const next: Record<string, boolean> = {}
    for (const it of selectableItems) next[it.id.toString()] = true
    setSelected(next)
  }

  const actions = (
    <>
      <Button
        variant="outlined"
        size="small"
        onClick={selectOrClearAll}
        disabled={selectableItems.length === 0}
        sx={{ height: 28 }}
      >
        {selectedCount > 0 ? 'Clear All' : 'Select All'}
      </Button>

      <Button
        variant="outlined"
        color="error"
        size="small"
        onClick={() => setDeleteModalOpen(true)}
        disabled={deletableSelected.length === 0}
        startIcon={<DeleteIcon />}
        sx={{ height: 28 }}
      >
        Delete
      </Button>

      <Button
        variant="outlined"
        size="small"
        onClick={ignoreSelected}
        disabled={selectedItems.length === 0}
        startIcon={<NotificationsPausedIcon />}
        sx={{ height: 28 }}
      >
        Ignore Selected
      </Button>
    </>
  )

  return (
    <>
      <Box sx={{ mb: 2 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 1 }}>
          Not Found
        </Typography>

        <Typography variant="body2" color="textSecondary">
          No data was found for the following configured items
        </Typography>
      </Box>

      <Paper sx={{ p: 2, backgroundColor: theme.palette.grey[50] }}>
        <Box
          display="flex"
          justifyContent="flex-end"
          alignItems="center"
          gap={1}
          flexWrap="wrap"
          sx={{ mb: 2 }}
        >
          {selectedItems.length > 0 ? (
            <Typography
              variant="body2"
              sx={{
                mr: 1,
                color: theme.palette.text.secondary,
                whiteSpace: 'nowrap',
              }}
            >
              {selectedItems.length} selected
            </Typography>
          ) : null}

          {actions}
        </Box>

        <DiscrepancySectionCard title="Approaches/Phases">
          <DiscrepancyRow
            items={notFoundApproaches}
            itemStatuses={itemStatuses}
            isSelected={(it) => selected[it.id.toString()]}
            onToggleSelected={(it) => toggle(it.id.toString())}
            buttonWidth={110}
            getSortKey={(it) => getPhaseNumberFromLabel(it.label)}
          />
        </DiscrepancySectionCard>

        <Box sx={{ mt: 2 }}>
          <DiscrepancySectionCard title="Detectors">
            <DiscrepancyRow
              items={notFoundDetectors}
              itemStatuses={itemStatuses}
              isSelected={(it) => selected[it.id.toString()]}
              onToggleSelected={(it) => toggle(it.id.toString())}
              buttonWidth={65}
            />
          </DiscrepancySectionCard>
        </Box>
      </Paper>

      <DeleteModal
        id={modalId}
        name={modalName}
        objectType={modalObjectType}
        open={deleteModalOpen}
        onClose={() => setDeleteModalOpen(false)}
        onConfirm={confirmDelete}
        selectedRow={{ id: 0, name: 'the selected items' }}
        associatedObjects={[]}
        associatedObjectsLabel=""
      />
    </>
  )
}

export const getPhaseNumberFromLabel = (label: string | number) => {
  const s = label.toString()
  const m = s.match(/ph\s*(\d+)/i)
  const n = m ? Number(m[1]) : Number(s)
  return Number.isFinite(n) ? n : Number.POSITIVE_INFINITY
}
