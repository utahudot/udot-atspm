import { useGetLocationSyncLocationFromKey } from '@/api/config/aTSPMConfigurationApi'
import { AddButton } from '@/components/addButton'
import { LocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import { useLocationStore } from '@/features/locations/locationStore'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CloseIcon from '@mui/icons-material/Close'
import DoneIcon from '@mui/icons-material/Done'
import SyncIcon from '@mui/icons-material/Sync'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Chip,
  Collapse,
  Divider,
  Paper,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'

interface ApproachOptionsProps {
  handler: LocationConfigHandler
}

const ApproachOptions = ({ handler }: ApproachOptionsProps) => {
  const setBadApproaches = useLocationStore((state) => state.setBadApproaches)
  const setBadDetectors = useLocationStore((state) => state.setBadDetectors)
  const resetLocationStore = useLocationStore((state) => state.resetStore)
  const { mutateAsync, isLoading } = useGetLocationSyncLocationFromKey()
  const [showSyncContainer, setShowSyncContainer] = useState(false)
  const [categories, setCategories] = useState({
    foundPhases: [],
    foundDetectors: [],
    notFoundApproaches: [],
    notFoundDetectors: [],
  })

  const handleSyncLocation = async () => {
    try {
      const response = await mutateAsync({
        key: parseInt(handler.expandedLocation.id),
      })

      const notFoundApproaches = response.removedApproachIds.map(
        (id: number) => {
          const approach = handler.approaches.find(
            (approach: any) => approach.id === id
          )
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

      setShowSyncContainer(true)
    } catch (error) {
      console.error(error)
    }
  }

  // Memoized arrays of found detector channels and approach phases
  const syncedPhases = useMemo(() => {
    return handler.approaches.flatMap((approach) => [
      approach.protectedPhaseNumber,
      approach.permissivePhaseNumber,
      approach.pedestrianPhaseNumber,
    ])
  }, [handler.approaches])

  const syncedDetectors = useMemo(() => {
    return handler.approaches.flatMap((approach) =>
      approach.detectors.map((det) => det.detectorChannel)
    )
  }, [handler.approaches])

  const isPhaseSynced = (phase: string): boolean => {
    return syncedPhases.includes(phase)
  }

  const isDetectorSynced = (detector: string): boolean => {
    return syncedDetectors.includes(detector)
  }

  // Reset store and local state when location changes
  // useEffect(() => {
  //   resetLocationStore() // Clear badApproaches and badDetectors
  //   setCategories({
  //     foundPhases: [],
  //     foundDetectors: [],
  //     notFoundApproaches: [],
  //     notFoundDetectors: [],
  //   })
  //   setShowSyncContainer(false)
  // }, [handler.expandedLocation.id, resetLocationStore])

  const renderCategory = (
    title: string,
    subtext: string,
    phases: string[],
    detectors: string[]
  ) => (
    <Box>
      <Typography variant="h4" sx={{ mb: 1 }} fontWeight={'bold'}>
        {title}
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
        {subtext}:
      </Typography>

      {/* Phases Section */}
      <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
        Phases/Approaches
      </Typography>
      {phases.length === 0 && (
        <Typography
          variant="body2"
          color="success.main"
          sx={{
            mb: 1,
            display: 'flex',
            alignItems: 'center',
            fontSize: '0.875rem',
          }}
        >
          <CheckCircleIcon sx={{ fontSize: '1rem', mr: 0.5 }} />
          In Sync
        </Typography>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
        {phases.map((phase, index) => (
          <Chip
            key={index}
            label={phase}
            onDelete={() => console.log(`Delete ${phase}`)} // Always define onDelete
            deleteIcon={isPhaseSynced(phase) ? <DoneIcon /> : <CloseIcon />}
            sx={{
              fontSize: '0.875rem',
              bgcolor: isPhaseSynced(phase) ? 'success.light' : undefined,
            }}
          />
        ))}
      </Box>

      {/* Detectors Section */}
      <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
        Detector Channels
      </Typography>
      {detectors.length === 0 && (
        <Typography
          variant="body2"
          color="success.main"
          sx={{
            mb: 1,
            display: 'flex',
            alignItems: 'center',
            fontSize: '0.875rem',
          }}
        >
          <CheckCircleIcon sx={{ fontSize: '1rem', mr: 0.5 }} />
          In Sync
        </Typography>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
        {detectors.map((detector, index) => (
          <Chip
            key={index}
            label={detector}
            onDelete={() => console.log(`Delete ${detector}`)} // Always define onDelete
            deleteIcon={
              isDetectorSynced(detector) ? <DoneIcon /> : <CloseIcon />
            }
            sx={{
              fontSize: '0.875rem',
              bgcolor: isDetectorSynced(detector) ? 'success.light' : undefined,
            }}
          />
        ))}
      </Box>
    </Box>
  )

  return (
    <Box>
      {/* Action Buttons */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          alignItems: 'center',
          mb: 1,
        }}
      >
        <AddButton
          label="New Approach"
          onClick={handler.handleAddNewApproach}
          sx={{ mr: 1 }}
        />
        <LoadingButton
          startIcon={<SyncIcon />}
          loading={isLoading}
          loadingPosition="start"
          variant="contained"
          color="primary"
          onClick={handleSyncLocation}
        >
          Sync
        </LoadingButton>
      </Box>

      {/* Sync Container with Smooth Transition */}
      <Collapse in={showSyncContainer} timeout="auto" unmountOnExit>
        <Paper sx={{ p: 2, position: 'relative' }}>
          {renderCategory(
            'Found',
            'Found data for the following phases/detector channels with no associated approach/detector',
            categories.foundPhases,
            categories.foundDetectors
          )}
          <Divider sx={{ my: 2 }} />
          {renderCategory(
            'Not Found',
            'No data was found for the following approaches/detectors',
            categories.notFoundApproaches,
            categories.notFoundDetectors
          )}

          {/* Finish Button */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'flex-end',
            }}
          >
            <Button
              variant="contained"
              color="primary"
              onClick={() => setShowSyncContainer(false)}
              sx={{ mt: 1 }}
            >
              Finish
            </Button>
          </Box>
        </Paper>
      </Collapse>
    </Box>
  )
}

export default ApproachOptions
