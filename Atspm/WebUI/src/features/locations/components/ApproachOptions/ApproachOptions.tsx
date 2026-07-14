import { usePatchApproachFromKey, usePatchLocationFromKey } from '@/api/config'
import { AddButton } from '@/components/addButton'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import ApproachesReconcilationReport from '@/features/locations/components/ApproachesReconcilationReport/ApproachesReconcilationReport'
import { LocationDiscrepancyReport } from '@/features/locations/components/ApproachesReconcilationReport/useDiscrepancyStatuses'
import { NavigationProvider } from '@/features/locations/components/Cell/CellNavigation'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import type { LocationExpanded } from '@/features/locations/types'
import { configAxios } from '@/lib/axios'
import { useNotificationStore } from '@/stores/notifications'
import SyncIcon from '@mui/icons-material/Sync'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  Paper,
  Typography,
} from '@mui/material'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useMutation } from 'react-query'

type SyncLocationResponse = {
  loggedButUnusedProtectedOrPermissivePhases?: number[] | null
  loggedButUnusedOverlapPhases?: number[] | null
  loggedButUnusedDetectorChannels?: number[] | null
  removedApproachIds?: number[] | null
  removedDetectors?: Array<string | number> | null
}

const emptyCategories: LocationDiscrepancyReport = {
  foundPhaseNumbers: [],
  notFoundApproaches: [],
  foundDetectorChannels: [],
  notFoundDetectorChannels: [],
}

function useSyncLocation() {
  return useMutation((locationId: number) =>
    configAxios.post<SyncLocationResponse, SyncLocationResponse>(
      `/Location/${locationId}/SyncLocation`
    )
  )
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

  const [showSummary, setShowSummary] = useState(false)
  const [showPedsAre1To1Dialog, setShowPedsAre1To1Dialog] = useState(false)
  const [categories, setCategories] =
    useState<LocationDiscrepancyReport>(emptyCategories)

  const combinedLocation = useMemo(
    () => (location ? { ...location, approaches } : undefined),
    [location, approaches]
  )

  const summaryLocation = combinedLocation as unknown as
    | LocationExpanded
    | undefined

  const detectorCount = useMemo(
    () =>
      approaches.reduce(
        (acc, approach) => acc + (approach.detectors?.length || 0),
        0
      ),
    [approaches]
  )

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
        </Box>
      </Paper>

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
