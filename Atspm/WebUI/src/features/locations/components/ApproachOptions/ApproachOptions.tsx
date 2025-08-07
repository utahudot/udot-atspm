import { useGetLocationSyncLocationFromKey } from '@/api/config'
import { AddButton } from '@/components/addButton'
import ApproachesReconcilationReport from '@/features/locations/components/ApproachesReconcilationReport'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import SyncIcon from '@mui/icons-material/Sync'
import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import { useCallback, useEffect, useMemo, useState } from 'react'

const ApproachOptions = () => {
  const {
    shouldSyncApproaches,
    approachesSynced,
    setShouldSyncApproaches,
    setApproachesSynced,
    setBadApproaches,
    setBadDetectors,
  } = useLocationWizardStore()
  const { approaches, location, addApproach } = useLocationStore()

  const { mutateAsync, isLoading } = useGetLocationSyncLocationFromKey()

  const [categories, setCategories] = useState({
    foundPhases: [] as string[],
    foundDetectors: [] as string[],
    notFoundApproaches: [] as string[],
    notFoundDetectors: [] as string[],
  })

  const syncedPhases = useMemo(
    () =>
      approaches.flatMap((approach) => [
        approach.protectedPhaseNumber,
        approach.permissivePhaseNumber,
        approach.pedestrianPhaseNumber,
      ]),
    [approaches]
  )

  const syncedDetectors = useMemo(
    () =>
      approaches.flatMap((approach) =>
        approach.detectors.map((det) => det.detectorChannel)
      ),
    [approaches]
  )

  const handleSyncLocation = useCallback(async () => {
    try {
      const response = await mutateAsync({
        key: parseInt(location.id, 10),
      })

      const notFoundApproaches = response.removedApproachIds.map(
        (id: number) => {
          const approach = approaches.find((a: any) => a.id === id)
          return approach
            ? approach.description
            : `Unknown Approach (ID: ${id})`
        }
      )

      setBadApproaches(response.removedApproachIds)
      setBadDetectors(response.removedDetectors)

      setCategories({
        foundPhases: [
          ...response.loggedButUnusedProtectedOrPermissivePhases,
          ...response.loggedButUnusedOverlapPhases,
        ],
        foundDetectors: response.loggedButUnusedDetectorChannels,
        notFoundApproaches,
        notFoundDetectors: response.removedDetectors,
      })
    } catch (error) {
      console.error(error)
    }
  }, [
    mutateAsync,
    location.id,
    approaches,
    setBadApproaches,
    setBadDetectors,
    setCategories,
  ])

  // If the store says we should sync, and we haven't yet, do it automatically
  useEffect(() => {
    if (shouldSyncApproaches && !approachesSynced) {
      ;(async () => {
        await handleSyncLocation()
        setApproachesSynced(true)
        setShouldSyncApproaches(false)
      })()
    }
  }, [
    shouldSyncApproaches,
    approachesSynced,
    handleSyncLocation,
    setApproachesSynced,
    setShouldSyncApproaches,
  ])

  return (
    <Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          alignItems: 'center',
          mb: 1,
        }}
      >
        <AddButton label="New Approach" onClick={addApproach} sx={{ mr: 1 }} />
        <LoadingButton
          startIcon={<SyncIcon />}
          loading={isLoading}
          loadingPosition="start"
          variant="contained"
          color="primary"
          onClick={async () => {
            await handleSyncLocation()
            setApproachesSynced(true)
            setShouldSyncApproaches(false)
          }}
        >
          Sync
        </LoadingButton>
      </Box>

      <ApproachesReconcilationReport
        synced={approachesSynced}
        categories={categories}
        syncedPhases={syncedPhases}
        syncedDetectors={syncedDetectors}
      />
    </Box>
  )
}

export default ApproachOptions
