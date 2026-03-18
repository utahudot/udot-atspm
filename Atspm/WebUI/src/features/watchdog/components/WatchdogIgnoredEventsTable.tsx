import ATSPMDialog from '@/components/ATSPMDialog'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import {
  useDeleteWatchdogIgnoreEvents,
  useEditWatchdogIgnoreEvents,
  useGetWatchdogIgnoreEvents,
} from '@/features/watchdog/api/watchdogIgnoreEvents'
import { useNotificationStore } from '@/stores/notifications'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box, Button, TextField } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { type FormEvent, useEffect, useMemo, useState } from 'react'

type ComponentType = 'Location' | 'Approach' | 'Detector' | null

type WatchdogIgnoreEvent = {
  id: number
  locationId: number
  locationIdentifier: string
  start: string
  end: string | null
  issueType: string
  componentType: ComponentType
  componentId: number | null
  phase: number | null
  created?: string
  modified?: string
  createdBy?: string
  modifiedBy?: string
}

function componentLabel(e: WatchdogIgnoreEvent) {
  if (!e.componentType) return 'Location'
  if (e.componentType === 'Location')
    return `Location (${e.componentId ?? '—'})`
  if (e.componentType === 'Approach')
    return `Approach (${e.componentId ?? '—'})`
  return `Detector (${e.componentId ?? '—'})`
}

type IgnoreEventEditorModalProps = {
  open: boolean
  onSave: (row: WatchdogIgnoreEvent) => void | Promise<void>
  onClose: () => void
  data?: WatchdogIgnoreEvent | null
}

function IgnoreEventEditorModal({
  open,
  onSave,
  onClose,
  data,
}: IgnoreEventEditorModalProps) {
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)

  useEffect(() => {
    if (!open) return

    setStartDate(data?.start ? new Date(data.start) : null)
    setEndDate(data?.end ? new Date(data.end) : null)
  }, [open, data])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!data || !startDate) return

    await onSave({
      ...data,
      start: toUTCDateStamp(startDate),
      end: endDate ? toUTCDateStamp(endDate) : null,
    })
    onClose()
  }

  return (
    <ATSPMDialog
      isOpen={open}
      onClose={onClose}
      onSubmit={handleSubmit}
      title="Edit Ignored Event"
      auditInfo={data ?? undefined}
      dialogProps={{ sx: { minWidth: 520, pt: 0 } }}
    >
      <TextField
        autoFocus
        margin="dense"
        fullWidth
        label="Location"
        value={data?.locationIdentifier ?? ''}
        disabled
      />
      <TextField
        margin="dense"
        fullWidth
        label="Component"
        value={data ? componentLabel(data) : ''}
        disabled
      />
      <TextField
        margin="dense"
        fullWidth
        label="Issue"
        value={data?.issueType ?? ''}
        disabled
      />
      <TextField
        margin="dense"
        fullWidth
        label="Phase"
        value={data?.phase ?? '—'}
        disabled
      />
      <DatePicker
        label="Start Date"
        value={startDate}
        onChange={(newValue) => setStartDate(newValue)}
        slotProps={{
          textField: {
            autoFocus: true,
            margin: 'dense',
            fullWidth: true,
            required: true,
          },
        }}
      />
      <DatePicker
        label="End Date"
        value={endDate}
        onChange={(newValue) => setEndDate(newValue)}
        slotProps={{
          textField: {
            margin: 'dense',
            fullWidth: true,
            helperText: 'Optional',
          },
        }}
      />
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1 }}>
        <Button type="button" onClick={() => setEndDate(null)}>
          Clear End Date
        </Button>
      </Box>
    </ATSPMDialog>
  )
}

export default function WatchdogIgnoredEvents() {
  const { addNotification } = useNotificationStore()

  const { data, isLoading, refetch } = useGetWatchdogIgnoreEvents()
  const { mutateAsync: editIgnore } = useEditWatchdogIgnoreEvents()
  const { mutateAsync: deleteIgnore } = useDeleteWatchdogIgnoreEvents()

  const rows = useMemo(() => {
    const value = (data?.value ?? []) as WatchdogIgnoreEvent[]
    return value.slice().sort((a, b) => {
      const ak = `${a.locationIdentifier}-${a.componentType ?? 'Global'}-${
        a.componentId ?? -1
      }-${a.issueType}-${a.phase ?? -1}`
      const bk = `${b.locationIdentifier}-${b.componentType ?? 'Global'}-${
        b.componentId ?? -1
      }-${b.issueType}-${b.phase ?? -1}`
      return ak.localeCompare(bk)
    })
  }, [data?.value])

  if (isLoading) return <Box height="500px">Loading…</Box>

  const tableData = rows.map((r) => ({
    ...r,
    component: componentLabel(r),
    phaseDisplay: r.phase ?? '—',
    endDisplay: r.end ?? '—',
  }))

  const cells = [
    { key: 'locationIdentifier', label: 'Location' },
    { key: 'component', label: 'Component' },
    { key: 'issueType', label: 'Issue' },
    { key: 'phaseDisplay', label: 'Phase' },
    { key: 'start', label: 'Start' },
    { key: 'endDisplay', label: 'End' },
  ]

  const handleEditRow = async (updated: WatchdogIgnoreEvent) => {
    try {
      await editIgnore({
        id: updated.id,
        data: {
          locationId: updated.locationId,
          locationIdentifier: updated.locationIdentifier,
          issueType: updated.issueType,
          start: updated.start,
          end: updated.end,
          componentType: updated.componentType,
          componentId: updated.componentId,
          phase: updated.phase,
        },
      })

      addNotification({ title: 'Ignore Event Updated', type: 'success' })
      await refetch()
    } catch {
      addNotification({ title: 'Error Updating Ignore Event', type: 'error' })
    }
  }

  const handleDeleteById = async (id: string | number) => {
    try {
      await deleteIgnore(Number(id))
      addNotification({ title: 'Ignore Event Removed', type: 'success' })
      await refetch()
    } catch {
      addNotification({ title: 'Error Removing Ignore Event', type: 'error' })
    }
  }

  const filterAssociatedObjects = () => []

  return (
    <AdminTable
      cells={cells}
      pageName="Ignored Event"
      data={tableData}
      marginTop={0}
      hasEditPrivileges={true}
      hasDeletePrivileges={true}
      editModal={
        <IgnoreEventEditorModal
          open={true}
          onSave={handleEditRow}
          onClose={() => {}}
        />
      }
      deleteModal={
        <DeleteModal
          id={0}
          name={''}
          objectType="Ignored Event"
          open={false}
          onClose={() => {}}
          onConfirm={handleDeleteById}
          associatedObjects={[]}
          associatedObjectsLabel=""
          filterFunction={filterAssociatedObjects}
        />
      }
    />
  )
}
