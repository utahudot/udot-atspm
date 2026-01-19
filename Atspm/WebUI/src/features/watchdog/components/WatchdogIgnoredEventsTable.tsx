import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import WatchDogDatePopup from '@/features/locations/components/editLocation/WatchDogDatePopup'
import {
  useDeleteWatchdogIgnoreEvents,
  useEditWatchdogIgnoreEvents,
  useGetWatchdogIgnoreEvents,
} from '@/features/watchdog/api/watchdogIgnoreEvents'
import { useNotificationStore } from '@/stores/notifications'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { addDays, format } from 'date-fns'
import { useEffect, useMemo, useRef, useState } from 'react'

type ComponentType = 'Location' | 'Approach' | 'Detector' | null

type WatchdogIgnoreEvent = {
  id: number
  locationId: number
  locationIdentifier: string
  start: string
  end: string
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
  if (!e.componentType) return 'Global'
  if (e.componentType === 'Location')
    return `Location (${e.componentId ?? '—'})`
  if (e.componentType === 'Approach')
    return `Approach (${e.componentId ?? '—'})`
  return `Detector (${e.componentId ?? '—'})`
}

function formatMetaLine(row: WatchdogIgnoreEvent) {
  const created = row.created
    ? format(new Date(row.created), 'MM/dd/yyyy')
    : null
  const modified = row.modified
    ? format(new Date(row.modified), 'MM/dd/yyyy')
    : null

  const createdBy = row.createdBy ?? '—'
  const modifiedBy = row.modifiedBy ?? '—'

  const createdPart = created
    ? `Created ${created} by ${createdBy}`
    : `Created by ${createdBy}`
  const modifiedPart = modified
    ? `Modified ${modified} by ${modifiedBy}`
    : `Modified by ${modifiedBy}`

  return `${createdPart} / ${modifiedPart}`
}

type IgnoreEventEditorModalProps = {
  isOpen: boolean
  onSave: (row: WatchdogIgnoreEvent) => void | Promise<void>
  onClose: () => void
  data?: WatchdogIgnoreEvent | null
}

function IgnoreEventEditorModal({
  isOpen,
  onSave,
  onClose,
  data,
}: IgnoreEventEditorModalProps) {
  const anchorRef = useRef<HTMLDivElement | null>(null)
  const [startDate, setStartDate] = useState<Date | null>(new Date())
  const [endDate, setEndDate] = useState<Date | null>(addDays(new Date(), 1))

  useEffect(() => {
    if (!isOpen) return
    if (!data) return
    setStartDate(new Date(data.start))
    setEndDate(new Date(data.end))
  }, [isOpen, data])

  const handleIgnoreEvent = async () => {
    if (!data || !startDate || !endDate) return
    await onSave({
      ...data,
      start: toUTCDateStamp(startDate),
      end: toUTCDateStamp(endDate),
    })
  }

  return (
    <Box
      // gives WatchDogDatePopup a stable anchor element
      ref={anchorRef}
      sx={{
        position: 'fixed',
        top: 0,
        left: 0,
        width: 1,
        height: 1,
        pointerEvents: 'none',
      }}
    >
      <WatchDogDatePopup
        open={isOpen}
        anchorEl={anchorRef.current}
        handlePopoverClose={onClose}
        startDate={startDate}
        setStartDate={setStartDate}
        endDate={endDate}
        setEndDate={setEndDate}
        handleIgnoreEvent={handleIgnoreEvent}
        handleRemoveIgnore={() => {}}
      />
    </Box>
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
    id: r.id,
    locationIdentifier: r.locationIdentifier,
    component: componentLabel(r),
    issueType: r.issueType,
    phase: r.phase ?? '—',
    start: r.start,
    end: r.end,
    meta: formatMetaLine(r),
    // keep the full row available for edit/delete modals
    _raw: r,
  }))

  const headers = [
    'Location',
    'Component',
    'Issue',
    'Phase',
    'Start',
    'End',
    'Meta',
  ]
  const headerKeys = [
    'locationIdentifier',
    'component',
    'issueType',
    'phase',
    'start',
    'end',
    'meta',
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

  const handleDeleteById = async (id: number) => {
    try {
      await deleteIgnore(id)
      addNotification({ title: 'Ignore Event Removed', type: 'success' })
      await refetch()
    } catch {
      addNotification({ title: 'Error Removing Ignore Event', type: 'error' })
    }
  }

  const filterAssociatedObjects = () => []

  return (
    <AdminTable
      pageName="Ignored Event"
      headers={headers}
      headerKeys={headerKeys}
      data={tableData}
      // swap these to your real claims if you want to gate buttons
      hasEditPrivileges={true}
      hasDeletePrivileges={true}
      editModal={
        <IgnoreEventEditorModal
          isOpen={true}
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
