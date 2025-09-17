import { usePatchApproachFromKey, usePatchLocationFromKey } from '@/api/config'
import { AddButton } from '@/components/addButton'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import { NavigationProvider } from '@/features/locations/components/Cell/CellNavigation'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useUnsavedGuard } from '@/hooks/useUnsavedGuard'
import { useNotificationStore } from '@/stores/notifications'
import { TabContext, TabList, TabPanel } from '@mui/lab'
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
  Tab,
  Typography,
} from '@mui/material'
import React, { memo, useCallback, useEffect, useState } from 'react'
import EditLocationHeader from './EditLocationHeader'
import WatchdogEditor from './WatchdogEditor'

function EditLocation() {
  const location = useLocationStore((state) => state.location)
  const hasUnsavedChanges = useLocationStore((state) => state.hasUnsavedChanges)
  const resetStore = useLocationStore((state) => state.resetStore)
  const resetApproaches = useLocationStore((state) => state.resetApproaches)
  const [currentTab, setCurrentTab] = useState('1')

  const { allowNavigate, Prompt } = useUnsavedGuard({
    isDirty: hasUnsavedChanges,
    commit: resetApproaches,
  })

  const handleTabChange = async (_: React.SyntheticEvent, newTab: string) => {
    if (await allowNavigate(`tab:${newTab}`)) {
      setCurrentTab(newTab)
    }
  }

  useEffect(() => () => resetStore(), [resetStore])

  if (!location) return null

  return (
    <>
      <TabContext value={currentTab}>
        <EditLocationHeader />
        <TabList onChange={handleTabChange} aria-label="Location Tabs">
          <Tab label="General" value="1" />
          <Tab label="Devices" value="2" />
          <Tab label="Approaches" value="3" />
          <Tab label="Watchdog" value="4" />
        </TabList>

        <TabPanel value="1" sx={{ padding: 0 }}>
          <LocationGeneralOptionsEditor />
        </TabPanel>
        <TabPanel value="2" sx={{ padding: 0, marginBottom: '100px' }}>
          <EditDevices />
        </TabPanel>
        <TabPanel value="3" sx={{ padding: 0, minHeight: '400px' }}>
          <ApproachesTab />
        </TabPanel>
        <TabPanel value="4" sx={{ padding: 0 }}>
          <WatchdogEditor />
        </TabPanel>
      </TabContext>

      <Prompt />
    </>
  )
}

export default memo(EditLocation)

function ApproachesTab() {
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

  const { mutateAsync: updateLocation } = usePatchLocationFromKey()
  const { mutateAsync: updateApproach } = usePatchApproachFromKey()

  const [showSummary, setShowSummary] = useState(false)
  const [showPedsAre1To1Dialog, setShowPedsAre1To1Dialog] = useState(false)

  const handleAddApproach = useCallback(() => {
    addApproach()
  }, [addApproach])

  const combinedLocation = { ...location, approaches }

  const handleToggle = () => setShowPedsAre1To1Dialog(true)
  const applyChange = async () => {
    const { pedsAre1to1 } = combinedLocation
    setLocation({
      ...combinedLocation,
      pedsAre1to1: !pedsAre1to1,
    })

    const updatedApproaches = combinedLocation.approaches.map((approach) => ({
      ...approach,
      pedestrianDetectors: !pedsAre1to1
        ? approach.protectedPhaseNumber?.toString()
        : null,
      pedestrianPhaseNumber: !pedsAre1to1
        ? approach.protectedPhaseNumber
        : null,
    }))

    updateApproachesInStore(updatedApproaches)
    updateSavedApproaches(updatedApproaches)

    setShowPedsAre1To1Dialog(false)

    try {
      await updateLocation({
        key: combinedLocation.id,
        data: { pedsAre1to1: !pedsAre1to1 },
      })

      await Promise.all(
        updatedApproaches.map((app) =>
          updateApproach({
            key: app.id,
            data: {
              pedestrianDetectors: app.pedestrianDetectors,
              pedestrianPhaseNumber: app.pedestrianPhaseNumber,
            },
          })
        )
      )

      addNotification({
        title: `Location updated`,
        type: 'success',
      })
    } catch (error) {
      addNotification({
        title: `Error updating location`,
        type: 'error',
      })
    }
  }

  return (
    <Box sx={{ minHeight: '400px', mt: 2 }}>
      <Paper
        variant="outlined"
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 1,
          px: 2,
          py: 1,
        }}
      >
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <FormControlLabel
            control={<Checkbox onChange={handleToggle} />}
            name="pedsAre1to1"
            label="Pedestrian Phases 1:1"
            checked={location?.pedsAre1to1}
            sx={{ height: '30px' }}
          />
          <Typography variant="caption" color="textSecondary">
            {location?.pedsAre1to1
              ? 'Pedestrian phases are locked to their protected phases.'
              : 'Pedestrian phases can be edited individually.'}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <AddButton label="New Approach" onClick={handleAddApproach} />
          <Button
            variant="outlined"
            onClick={() => setShowSummary((prev) => !prev)}
          >
            {showSummary ? 'Hide Summary' : 'Summary'}
          </Button>
        </Box>
      </Paper>
      {showSummary && (
        <Paper sx={{ mb: 2 }}>
          <Typography variant="h6" sx={{ p: 2, fontWeight: 'bold' }}>
            Approaches
          </Typography>
          <Divider sx={{ m: 1 }} />
          <ApproachesInfo location={combinedLocation} />
          <Typography variant="h6" sx={{ p: 2, fontWeight: 'bold' }}>
            Detectors
          </Typography>
          <Divider sx={{ m: 1 }} />
          <DetectorsInfo location={combinedLocation} />
        </Paper>
      )}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 1 }}>
        <Typography variant="h6">Approaches</Typography>
        <Typography variant="caption" color="textSecondary">
          {approaches.length}{' '}
          {approaches.length === 1 ? 'Approach' : 'Approaches'}
          {'  •  '}
          {approaches.reduce(
            (acc, approach) => acc + (approach.detectors?.length || 0),
            0
          )}{' '}
          Detectors
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
          {location?.pedsAre1to1
            ? 'Unlock individual pedestrian phase control?'
            : 'Lock all pedestrian phases to protected phases?'}
        </DialogTitle>

        <DialogContent sx={{ fontSize: '0.9rem', color: 'text.secondary' }}>
          {location?.pedsAre1to1
            ? 'This will allow you to edit each pedestrian phase individually for every approach.'
            : 'This will force every pedestrian phase to mirror its protected phase and disable individual edits.'}
        </DialogContent>

        <DialogActions>
          <Button onClick={() => setShowPedsAre1To1Dialog(false)}>
            Cancel
          </Button>
          <Button onClick={applyChange} color="primary" variant="contained">
            {location?.pedsAre1to1 ? 'Unlock' : 'Lock'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
