import type { Device } from '@/api/config'
import {
  useGetLocationDevicesFromKey,
  usePatchDeviceFromKey,
} from '@/api/config'
import type { DeviceEventDownload } from '@/api/data'
import { useGetLoggingSyncDeviceEvents } from '@/api/data'
import { useGetDeviceConfigurations } from '@/features/devices/api'
import { useDeleteDevice } from '@/features/devices/api/devices'
import DeviceCard from '@/features/locations/components/editLocation/DeviceCard'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import DeviceModal from '@/features/locations/components/editLocation/NewDeviceModal'
import DevicesWizardModal from '@/features/locations/components/LocationSetupWizard/DevicesWizardPanel'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { useNotificationStore } from '@/stores/notifications'
import AddIcon from '@mui/icons-material/Add'
import LanIcon from '@mui/icons-material/Lan'
import { Avatar, Box, Button, Modal, Typography, useTheme } from '@mui/material'
import { useCallback, useEffect, useMemo, useState } from 'react'

interface CombinedDevice extends Device, Partial<DeviceEventDownload> {}
type DevicesResponse = { value?: Device[] }
type VerificationStage = 'idle' | 'saving' | 'checking'

function getDeviceEventDownloads(result: unknown): DeviceEventDownload[] {
  if (Array.isArray(result)) {
    return result as DeviceEventDownload[]
  }

  if (
    result &&
    typeof result === 'object' &&
    Array.isArray((result as { data?: unknown }).data)
  ) {
    return (result as { data: DeviceEventDownload[] }).data
  }

  return []
}

const EditDevices = () => {
  const theme = useTheme()
  const { location } = useLocationStore()
  const { addNotification } = useNotificationStore()

  const { deviceVerificationStatus, setDeviceVerificationStatus } =
    useLocationWizardStore()

  const [isModalOpen, setModalOpen] = useState(false)
  const [currentDevice, setCurrentDevice] = useState<Device | null>(null)
  const [openDeleteModal, setOpenDeleteModal] = useState(false)
  const [deleteDeviceId, setDeleteDeviceId] = useState<number | null>(null)

  const [showSyncModal, setShowSyncModal] = useState(false)

  const [ipChanges, setIpChanges] = useState<Record<number, string>>({})

  const [isFetchingEvents, setIsFetchingEvents] = useState(false)
  const [verificationStage, setVerificationStage] =
    useState<VerificationStage>('idle')

  const {
    data: devicesData,
    refetch: refetchDevices,
    isFetching: isRefetchingDevices,
  } = useGetLocationDevicesFromKey(location?.id as number, {
    expand: 'DeviceConfiguration',
  })

  const devices = useMemo(
    () => (devicesData as DevicesResponse | undefined)?.value || [],
    [devicesData]
  )
  const hasLoadedDevices = Boolean(
    (devicesData as DevicesResponse | undefined)?.value
  )
  const hasDevices = devices.length > 0

  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()
  const { mutate: deleteDevice } = useDeleteDevice()
  const { mutateAsync: updateDevice } = usePatchDeviceFromKey()

  const deviceIds = useMemo(
    () =>
      devices
        .map((device) => Number(device.id))
        .filter((deviceId) => Number.isInteger(deviceId) && deviceId > 0),
    [devices]
  )
  const {
    data: deviceEventResults,
    mutateAsync: syncDeviceEvents,
    isLoading: isEventDataLoading,
  } = useGetLoggingSyncDeviceEvents()

  const persistPendingIpChanges = useCallback(
    async (notifyOnSuccess = false) => {
      const invalidChanges = Object.entries(ipChanges).filter(
        ([deviceId, newIp]) =>
          devices.some((device) => device.id === Number(deviceId)) &&
          newIp.trim().length === 0
      )

      if (invalidChanges.length > 0) {
        addNotification({
          title:
            invalidChanges.length === 1
              ? 'IP address is required before continuing'
              : 'IP addresses are required before continuing',
          type: 'error',
        })

        return { hasErrors: true }
      }

      const pendingChanges = Object.entries(ipChanges)
        .map(([deviceId, newIp]) => {
          const matchingDevice = devices.find(
            (device) => device.id === Number(deviceId)
          )
          const trimmedIp = newIp.trim()

          if (
            !matchingDevice ||
            !trimmedIp ||
            matchingDevice.ipaddress === trimmedIp
          ) {
            return null
          }

          return {
            deviceId: matchingDevice.id,
            ipaddress: trimmedIp,
          }
        })
        .filter(
          (
            change
          ): change is {
            deviceId: number
            ipaddress: string
          } => change !== null
        )

      if (!pendingChanges.length) {
        return { hasErrors: false }
      }

      const results = await Promise.allSettled(
        pendingChanges.map(({ deviceId, ipaddress }) =>
          updateDevice({
            key: deviceId,
            data: { ipaddress },
          })
        )
      )

      const savedIds: number[] = []
      const failedIds: number[] = []

      results.forEach((result, index) => {
        const deviceId = pendingChanges[index].deviceId

        if (result.status === 'fulfilled') {
          savedIds.push(deviceId)
          return
        }

        failedIds.push(deviceId)
        console.error(
          `Failed to update device IP for device ${deviceId}:`,
          result.reason
        )
      })

      if (savedIds.length > 0) {
        await refetchDevices()
        setIpChanges((prev) => {
          const nextChanges = { ...prev }

          savedIds.forEach((deviceId) => {
            delete nextChanges[deviceId]
          })

          return nextChanges
        })
      }

      if (notifyOnSuccess && savedIds.length > 0) {
        addNotification({
          title:
            savedIds.length === 1
              ? 'Device IP address saved'
              : `${savedIds.length} device IP addresses saved`,
          type: 'success',
        })
      }

      if (failedIds.length > 0) {
        addNotification({
          title:
            failedIds.length === 1
              ? 'Failed to save 1 device IP address'
              : `Failed to save ${failedIds.length} device IP addresses`,
          type: 'error',
        })
      }

      return { hasErrors: failedIds.length > 0 }
    },
    [addNotification, devices, ipChanges, refetchDevices, updateDevice]
  )

  const handleResync = useCallback(async () => {
    try {
      setIsFetchingEvents(true)
      setVerificationStage('saving')

      const { hasErrors } = await persistPendingIpChanges()
      if (hasErrors) return

      if (deviceIds.length === 0) return

      setVerificationStage('checking')
      await syncDeviceEvents({ data: { deviceIds } })
    } catch (err) {
      console.error('Failed to fetch device event data: ', err)
    } finally {
      setVerificationStage('idle')
      setIsFetchingEvents(false)
    }
  }, [deviceIds, persistPendingIpChanges, syncDeviceEvents])

  // ------------------------------------------------
  // 1) If the wizard says "READY_TO_RUN", open modal & run check
  // ------------------------------------------------
  useEffect(() => {
    if (deviceVerificationStatus !== 'READY_TO_RUN') return
    if (deviceIds.length === 0) return
    setShowSyncModal(true)
    handleResync()
    setDeviceVerificationStatus('DONE')
  }, [
    deviceVerificationStatus,
    setDeviceVerificationStatus,
    handleResync,
    deviceIds,
  ])

  const combinedDevices: CombinedDevice[] = useMemo(() => {
    if (!devices.length) return []
    const finalEventData = getDeviceEventDownloads(deviceEventResults)
    const allConfigs = deviceConfigurationsData?.value || []

    return devices.map((dev) => {
      const matchedEvents = finalEventData.find((e) => e.deviceId === dev.id)
      const matchedConfig = allConfigs.find(
        (cfg) => cfg.id === dev.deviceConfigurationId
      )
      return {
        ...dev,
        ...matchedEvents,
        deviceConfiguration: matchedConfig ?? dev.deviceConfiguration,
      } as CombinedDevice
    })
  }, [devices, deviceEventResults, deviceConfigurationsData])

  const handleSaveAndClose = async () => {
    try {
      setIsFetchingEvents(true)
      setVerificationStage('saving')

      const { hasErrors } = await persistPendingIpChanges(true)
      if (hasErrors) return

      setIpChanges({})
      setShowSyncModal(false)
      setDeviceVerificationStatus('DONE')
    } finally {
      setVerificationStage('idle')
      setIsFetchingEvents(false)
    }
  }

  const handleModalClose = () => {
    setShowSyncModal(false)
  }

  if (!deviceConfigurationsData?.value || !hasLoadedDevices) {
    return <Typography variant="h6">Loading...</Typography>
  }

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
        <Button
          startIcon={<LanIcon />}
          variant="contained"
          color="primary"
          disabled={!hasDevices}
          onClick={() => {
            setShowSyncModal(true)
            handleResync()
          }}
        >
          Verify IP Addresses
        </Button>
      </Box>

      <DevicesWizardModal
        open={showSyncModal}
        onClose={handleModalClose}
        onSaveAndClose={handleSaveAndClose}
        devices={combinedDevices}
        onResync={handleResync}
        isResyncing={
          isEventDataLoading || isRefetchingDevices || isFetchingEvents
        }
        verificationStage={verificationStage}
        ipChanges={ipChanges}
        setIpChanges={setIpChanges}
      />

      <Box
        sx={{
          display: 'flex',
          overflowX: 'auto',
          flexWrap: 'nowrap',
          gap: '30px',
          mt: '10px',
        }}
      >
        {devices.map((device) => (
          <DeviceCard
            key={device.id}
            device={device}
            onEdit={(dev) => {
              setCurrentDevice(dev)
              setModalOpen(true)
            }}
            onDelete={() => {
              setDeleteDeviceId(device.id ?? null)
              setOpenDeleteModal(true)
            }}
          />
        ))}

        {/* Add Device card */}
        <Button
          onClick={() => {
            setCurrentDevice(null)
            setModalOpen(true)
          }}
          sx={{
            padding: 2,
            mb: 1.95,
            minWidth: '400px',
            minHeight: '400px',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            alignItems: 'center',
            border: `4px dashed ${theme.palette.primary.main}`,
          }}
        >
          <Avatar sx={{ bgcolor: theme.palette.primary.main, mb: 1 }}>
            <AddIcon />
          </Avatar>
          <Typography variant="h6" sx={{ mt: 2 }}>
            Add New Device
          </Typography>
        </Button>
      </Box>

      {/* Add/Edit Device Modal */}
      {isModalOpen && (
        <DeviceModal
          onClose={() => setModalOpen(false)}
          device={currentDevice}
          locationId={String(location?.id ?? '')}
          refetchDevices={refetchDevices}
        />
      )}

      {/* Delete Confirmation Modal */}
      <Modal
        open={openDeleteModal}
        onClose={() => setOpenDeleteModal(false)}
        aria-labelledby="delete-confirmation"
        aria-describedby="confirm-delete-approach"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 400,
            bgcolor: 'background.paper',
            boxShadow: 24,
            p: 4,
          }}
        >
          <Typography id="delete-confirmation" sx={{ fontWeight: 'bold' }}>
            Confirm Delete
          </Typography>
          <Typography id="confirm-delete-approach" sx={{ mt: 2 }}>
            Are you sure you want to delete this device?
          </Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenDeleteModal(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={() => {
                if (deleteDeviceId) {
                  deleteDevice(deleteDeviceId, {
                    onSuccess: () => {
                      void refetchDevices()
                    },
                  })
                }
                setOpenDeleteModal(false)
              }}
              color="error"
              variant="contained"
            >
              Delete Device
            </Button>
          </Box>
        </Box>
      </Modal>
    </>
  )
}

export default EditDevices
