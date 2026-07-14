import { usePatchApproachFromKey, usePatchLocationFromKey } from '@/api/config'
import { AddButton } from '@/components/addButton'
import { useGetDeviceConfigurations } from '@/features/devices/api'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import ApproachesReconcilationReport from '@/features/locations/components/ApproachesReconcilationReport/ApproachesReconcilationReport'
import { LocationDiscrepancyReport } from '@/features/locations/components/ApproachesReconcilationReport/useDiscrepancyStatuses'
import { NavigationProvider } from '@/features/locations/components/Cell/CellNavigation'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import type { LocationExpanded } from '@/features/locations/types'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { useNotificationStore } from '@/stores/notifications'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import SyncIcon from '@mui/icons-material/Sync'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Checkbox,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  Paper,
  Typography,
} from '@mui/material'
import type { AxiosResponse } from 'axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useMutation } from 'react-query'

type SyncLocationResponse = {
  loggedButUnusedProtectedOrPermissivePhases?: number[] | null
  loggedButUnusedOverlapPhases?: number[] | null
  loggedButUnusedDetectorChannels?: number[] | null
  removedApproachIds?: number[] | null
  removedDetectors?: Array<string | number> | null
}

type ZoneRequest = {
  IpAddress?: string | null
  port?: string
  detectionType?: string
  deviceId?: string
}

type ZoneResult = {
  zones: string[]
  error?: string
}

type ZoneDevice = {
  id?: number | null
  deviceIdentifier?: string | number | null
  deviceType?: string | number | null
  ipaddress?: string | null
  deviceConfigurationId?: number | null
}

const emptyCategories: LocationDiscrepancyReport = {
  foundPhaseNumbers: [],
  notFoundApproaches: [],
  foundDetectorChannels: [],
  notFoundDetectorChannels: [],
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

const deviceTypeNames: Record<number, string> = {
  0: 'Unknown',
  1: 'SignalController',
  2: 'RampController',
  3: 'AICamera',
  4: 'FIRCamera',
  5: 'LidarSensor',
  6: 'WavetronixSpeed',
  7: 'SpeedSensor',
}

function useGetZones() {
  return usePostRequest<string[], ZoneRequest>({
    url: '/Detector/retrieveDetectionData',
    axiosInstance: configAxios,
    headers,
    notify: false,
  })
}

function useSyncLocation() {
  return useMutation((locationId: number) =>
    configAxios.post<SyncLocationResponse, SyncLocationResponse>(
      `/Location/${locationId}/SyncLocation`
    )
  )
}

const getDeviceTypeName = (deviceType: string | number | null | undefined) => {
  if (typeof deviceType === 'number') {
    return deviceTypeNames[deviceType] ?? `${deviceType}`
  }

  return deviceType ?? ''
}

const isFirCameraDevice = (device: ZoneDevice) =>
  getDeviceTypeName(device.deviceType) === 'FIRCamera'

const getZonesFromResponse = (response: unknown) => {
  if (Array.isArray(response)) return response as string[]

  const data = (response as AxiosResponse<string[]> | undefined)?.data
  return Array.isArray(data) ? data : []
}

const getExternalServiceErrorMessage = (error: unknown) => {
  if (typeof error === 'object' && error !== null) {
    if ('response' in error) {
      const response = error.response

      if (
        typeof response === 'object' &&
        response !== null &&
        'data' in response
      ) {
        const data = response.data

        if (typeof data === 'string' && data.length > 0) {
          return data
        }
      }
    }

    if ('message' in error && typeof error.message === 'string') {
      return error.message
    }
  }

  return 'Failed to retrieve data from the external service.'
}

const ApproachOptions = () => {
  const {
    approachVerificationStatus,
    setApproachVerificationStatus,
    setBadApproaches,
    setBadDetectors,
  } = useLocationWizardStore()
  const addNotification = useNotificationStore((state) => state.addNotification)

  const location = useLocationStore((state) => state.location)
  const setLocation = useLocationStore((state) => state.setLocation)
  const approaches = useLocationStore((state) => state.approaches)
  const addApproach = useLocationStore((state) => state.addApproach)
  const updateSavedApproaches = useLocationStore(
    (state) => state.updateSavedApproaches
  )
  const updateApproachesInStore = useLocationStore(
    (state) => state.updateApproaches
  )

  const { mutateAsync: syncLocation, isLoading: isSyncingApproaches } =
    useSyncLocation()
  const { mutateAsync: updateLocation } = usePatchLocationFromKey()
  const { mutateAsync: updateApproach } = usePatchApproachFromKey()
  const { mutateAsync: getZones } = useGetZones()
  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()

  const [showSummary, setShowSummary] = useState(false)
  const [showZones, setShowZones] = useState(false)
  const [showPedsAre1To1Dialog, setShowPedsAre1To1Dialog] = useState(false)
  const [zones, setZones] = useState<Record<string, ZoneResult>>({})
  const [categories, setCategories] =
    useState<LocationDiscrepancyReport>(emptyCategories)

  const combinedLocation = useMemo(
    () => (location ? { ...location, approaches } : undefined),
    [location, approaches]
  )

  const summaryLocation = combinedLocation as unknown as
    | LocationExpanded
    | undefined

  const locationDevices = useMemo(
    () =>
      Array.isArray(combinedLocation?.devices)
        ? (combinedLocation.devices as ZoneDevice[])
        : [],
    [combinedLocation?.devices]
  )

  const firCameraDevices = useMemo(
    () => locationDevices.filter(isFirCameraDevice),
    [locationDevices]
  )

  const hasFirCameraDevice = firCameraDevices.length > 0

  const detectorCount = useMemo(
    () =>
      approaches.reduce(
        (acc, approach) => acc + (approach.detectors?.length || 0),
        0
      ),
    [approaches]
  )

  const handleGetZones = useCallback(async () => {
    if (firCameraDevices.length === 0) {
      addNotification({ title: 'No FIRCamera devices found', type: 'error' })
      return
    }

    const grouped: Record<string, ZoneResult> = {}

    for (const device of firCameraDevices) {
      const deviceConfig = deviceConfigurationsData?.value?.find(
        (config) => config.id === device.deviceConfigurationId
      )
      const deviceType = getDeviceTypeName(device.deviceType)
      const label = `${deviceType} - ${
        device.deviceIdentifier || `Device ${device.id}`
      }`

      try {
        const response = await getZones({
          IpAddress: device.ipaddress,
          port: deviceConfig?.port?.toString(),
          detectionType: deviceType,
          deviceId: device.deviceIdentifier?.toString(),
        })
        grouped[label] = { zones: getZonesFromResponse(response) }
      } catch (error: unknown) {
        grouped[label] = {
          zones: [],
          error: getExternalServiceErrorMessage(error),
        }
      }
    }

    setZones(grouped)
    setShowZones(true)
  }, [
    addNotification,
    deviceConfigurationsData?.value,
    firCameraDevices,
    getZones,
  ])

  const handleSyncLocation = useCallback(async () => {
    if (!location?.id) return

    try {
      const response = await syncLocation(location.id)
      const removedApproachIds = response.removedApproachIds ?? []
      const removedDetectors = (response.removedDetectors ?? []).map(String)

      setBadApproaches(removedApproachIds)
      setBadDetectors(removedDetectors)

      const notFoundApproaches = removedApproachIds.map((id) => {
        const approach = approaches.find((a) => a.id === id)
        return {
          id,
          description: approach?.description ?? `Unknown Approach (ID: ${id})`,
        }
      })

      const foundPhaseNumbers = Array.from(
        new Set<number>([
          ...(response.loggedButUnusedProtectedOrPermissivePhases ?? []),
          ...(response.loggedButUnusedOverlapPhases ?? []),
        ])
      )

      setCategories({
        foundPhaseNumbers,
        notFoundApproaches,
        foundDetectorChannels: response.loggedButUnusedDetectorChannels ?? [],
        notFoundDetectorChannels: removedDetectors,
      })
    } catch (error) {
      console.error(error)
      addNotification({
        title: 'Error reconciling approaches',
        type: 'error',
      })
    } finally {
      setApproachVerificationStatus('DONE')
    }
  }, [
    addNotification,
    approaches,
    location?.id,
    setApproachVerificationStatus,
    setBadApproaches,
    setBadDetectors,
    syncLocation,
  ])

  useEffect(() => {
    if (approachVerificationStatus === 'READY_TO_RUN') {
      void handleSyncLocation()
    }
  }, [approachVerificationStatus, handleSyncLocation])

  const applyPedestrianPhaseModeChange = useCallback(async () => {
    if (!combinedLocation) return

    const nextPedsAre1to1 = !combinedLocation.pedsAre1to1

    setLocation({
      ...combinedLocation,
      pedsAre1to1: nextPedsAre1to1,
    })

    const updatedApproaches = combinedLocation.approaches.map((approach) => ({
      ...approach,
      pedestrianDetectors: nextPedsAre1to1
        ? approach.protectedPhaseNumber?.toString()
        : null,
      pedestrianPhaseNumber: nextPedsAre1to1
        ? approach.protectedPhaseNumber
        : null,
    }))

    updateApproachesInStore(updatedApproaches)
    updateSavedApproaches(updatedApproaches)
    setShowPedsAre1To1Dialog(false)

    try {
      await updateLocation({
        key: combinedLocation.id,
        data: { pedsAre1to1: nextPedsAre1to1 },
      })

      await Promise.all(
        updatedApproaches.map((approach) =>
          updateApproach({
            key: approach.id,
            data: {
              pedestrianDetectors: approach.pedestrianDetectors,
              pedestrianPhaseNumber: approach.pedestrianPhaseNumber,
            },
          })
        )
      )

      addNotification({
        title: 'Location updated',
        type: 'success',
      })
    } catch (error) {
      addNotification({
        title: 'Error updating location',
        type: 'error',
      })
    }
  }, [
    addNotification,
    combinedLocation,
    setLocation,
    updateApproach,
    updateApproachesInStore,
    updateLocation,
    updateSavedApproaches,
  ])

  if (!combinedLocation) return null

  return (
    <Box sx={{ minHeight: '400px', mt: 2 }}>
      <Paper
        variant="outlined"
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: 2,
          mb: 1,
          px: 2,
          py: 1,
        }}
      >
        <Box sx={{ display: 'flex', flexDirection: 'column' }}>
          <FormControlLabel
            control={
              <Checkbox onChange={() => setShowPedsAre1To1Dialog(true)} />
            }
            name="pedsAre1to1"
            label="Pedestrian Phases 1:1"
            checked={combinedLocation.pedsAre1to1}
            sx={{ height: '30px' }}
          />
          <Typography variant="caption" color="text.secondary">
            {combinedLocation.pedsAre1to1
              ? 'Pedestrian phases are locked to their protected phases.'
              : 'Pedestrian phases can be edited individually.'}
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          <AddButton label="New Approach" onClick={() => addApproach()} />

          <Button
            variant="outlined"
            onClick={() => setShowSummary((prev) => !prev)}
          >
            {showSummary ? 'Hide Summary' : 'Summary'}
          </Button>

          <LoadingButton
            startIcon={<SyncIcon />}
            loading={isSyncingApproaches}
            loadingPosition="start"
            variant="outlined"
            onClick={handleSyncLocation}
          >
            Reconcile Approaches
          </LoadingButton>

          {hasFirCameraDevice && (
            <Button variant="outlined" onClick={handleGetZones}>
              Get Zones
            </Button>
          )}
        </Box>
      </Paper>

      {showZones && (
        <Paper sx={{ mb: 2 }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              p: 2,
            }}
          >
            <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
              Found Zones
            </Typography>
            <IconButton
              size="small"
              onClick={() => setShowZones((prev) => !prev)}
            >
              {showZones ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            </IconButton>
          </Box>
          <Collapse in={showZones}>
            <Divider sx={{ mx: 1 }} />
            <Box sx={{ p: 2 }}>
              {Object.keys(zones).length > 0 ? (
                <Grid container spacing={2}>
                  {Object.entries(zones).map(([label, result]) => (
                    <Grid item xs={12} sm={6} md={4} key={label}>
                      <Paper variant="outlined" sx={{ p: 2, height: '100%' }}>
                        <Typography
                          variant="subtitle1"
                          sx={{ fontWeight: 'bold', mb: 1 }}
                        >
                          {label}
                        </Typography>
                        <Divider sx={{ mb: 1 }} />

                        {result.zones.length > 0 && (
                          <Box
                            sx={{
                              columnCount: 1,
                              '& p': { breakInside: 'avoid' },
                            }}
                          >
                            {result.zones.map((zone, index) => (
                              <Typography key={index} variant="body2">
                                {zone}
                              </Typography>
                            ))}
                          </Box>
                        )}

                        {result.zones.length === 0 && !result.error && (
                          <Typography variant="body2" color="text.secondary">
                            No zones
                          </Typography>
                        )}

                        {result.error && (
                          <Typography variant="body2" color="error">
                            {result.error}
                          </Typography>
                        )}
                      </Paper>
                    </Grid>
                  ))}
                </Grid>
              ) : (
                <Typography variant="body1" color="text.secondary">
                  No zones found
                </Typography>
              )}
            </Box>
          </Collapse>
        </Paper>
      )}

      {approachVerificationStatus === 'DONE' && (
        <ApproachesReconcilationReport categories={categories} />
      )}

      {showSummary && (
        <Paper sx={{ mb: 2 }}>
          <Typography variant="h6" sx={{ p: 2, fontWeight: 'bold' }}>
            Approaches
          </Typography>
          <Divider sx={{ m: 1 }} />
          <ApproachesInfo location={summaryLocation} />
          <Typography variant="h6" sx={{ p: 2, fontWeight: 'bold' }}>
            Detectors
          </Typography>
          <Divider sx={{ m: 1 }} />
          <DetectorsInfo location={summaryLocation} />
        </Paper>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 1 }}>
        <Typography variant="h6">Approaches</Typography>
        <Typography variant="caption" color="text.secondary">
          {approaches.length}{' '}
          {approaches.length === 1 ? 'Approach' : 'Approaches'}
          {' | '}
          {detectorCount} {detectorCount === 1 ? 'Detector' : 'Detectors'}
        </Typography>
      </Box>

      {approaches.length > 0 ? (
        <NavigationProvider>
          {approaches.map((approach) => (
            <EditApproach key={approach.id} approach={approach} />
          ))}
        </NavigationProvider>
      ) : (
        <Box sx={{ p: 2, mt: 2, textAlign: 'center' }}>
          <Typography variant="caption" fontStyle="italic">
            No approaches found
          </Typography>
        </Box>
      )}

      <Dialog
        open={showPedsAre1To1Dialog}
        onClose={() => setShowPedsAre1To1Dialog(false)}
        sx={{ '& .MuiDialog-paper': { width: '400px' } }}
      >
        <DialogTitle variant="h4">
          {combinedLocation.pedsAre1to1
            ? 'Unlock individual pedestrian phase control?'
            : 'Lock all pedestrian phases to protected phases?'}
        </DialogTitle>

        <DialogContent sx={{ fontSize: '0.9rem', color: 'text.secondary' }}>
          {combinedLocation.pedsAre1to1
            ? 'This will allow you to edit each pedestrian phase individually for every approach.'
            : 'This will force every pedestrian phase to mirror its protected phase and disable individual edits.'}
        </DialogContent>

        <DialogActions>
          <Button onClick={() => setShowPedsAre1To1Dialog(false)}>
            Cancel
          </Button>
          <Button
            onClick={applyPedestrianPhaseModeChange}
            color="primary"
            variant="contained"
          >
            {combinedLocation.pedsAre1to1 ? 'Unlock' : 'Lock'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default ApproachOptions
