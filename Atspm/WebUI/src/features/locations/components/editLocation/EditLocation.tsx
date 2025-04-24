import { AddButton } from '@/components/addButton'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  Divider,
  Modal,
  Paper,
  Tab,
  Typography,
} from '@mui/material'
import { useRouter } from 'next/router'
import React, { memo, useCallback, useEffect, useState } from 'react'
import EditLocationHeader from './EditLocationHeader'
import WatchdogEditor from './WatchdogEditor'

function EditLocation() {
  const router = useRouter()
  const location = useLocationStore((state) => state.location)
  const hasUnsavedChanges = useLocationStore((state) => state.hasUnsavedChanges)
  const resetStore = useLocationStore((state) => state.resetStore)
  const updateSavedApproachesFromCurrent = useLocationStore(
    (state) => state.updateSavedApproachesFromCurrent
  )
  const [currentTab, setCurrentTab] = useState('1')
  const [pendingTab, setPendingTab] = useState<string | null>(null)
  const [pendingRoute, setPendingRoute] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const handleTabChange = useCallback(
    (_: React.SyntheticEvent, newTab: string) => {
      if (hasUnsavedChanges() && currentTab !== newTab) {
        setPendingTab(newTab)
        setDialogOpen(true)
      } else {
        setCurrentTab(newTab)
      }
    },
    [currentTab, hasUnsavedChanges]
  )

  const handleDialogClose = (confirm: boolean) => {
    setDialogOpen(false)
    if (confirm) {
      if (pendingTab) {
        updateSavedApproachesFromCurrent()
        setCurrentTab(pendingTab)
        setPendingTab(null)
      } else if (pendingRoute) {
        updateSavedApproachesFromCurrent()
        router.push(pendingRoute)
        setPendingRoute(null)
      }
    } else {
      setPendingTab(null)
      setPendingRoute(null)
    }
  }

  useEffect(() => {
    const handleRouteChangeStart = (url: string) => {
      if (hasUnsavedChanges() && url !== router.asPath) {
        setPendingRoute(url)
        setDialogOpen(true)
        router.events.emit('routeChangeError')
        throw 'Route change aborted due to unsaved changes'
      }
    }

    router.events.on('routeChangeStart', handleRouteChangeStart)
    return () => {
      router.events.off('routeChangeStart', handleRouteChangeStart)
    }
  }, [router, hasUnsavedChanges])

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

      <Modal
        open={dialogOpen}
        onClose={() => handleDialogClose(false)}
        aria-labelledby="leave-confirmation"
        aria-describedby="confirm-leave-location"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 400,
            bgcolor: 'background.paper',
            borderRadius: '10px',
            boxShadow: 24,
            p: 4,
          }}
        >
          <Typography id="leave-confirmation" sx={{ fontWeight: 'bold' }}>
            Unsaved Changes
          </Typography>
          <Typography>
            There are unsaved changes. Are you sure you want to{' '}
            {pendingTab ? 'switch tabs' : 'navigate away'}?
          </Typography>
          <Box
            sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end', gap: 1 }}
          >
            <Button onClick={() => handleDialogClose(false)} color="inherit">
              Cancel
            </Button>
            <Button variant="contained" onClick={() => handleDialogClose(true)}>
              Proceed
            </Button>
          </Box>
        </Box>
      </Modal>
    </>
  )
}

export default memo(EditLocation)

function ApproachesTab() {
  const location = useLocationStore((state) => state.location)
  const approaches = useLocationStore((state) => state.approaches)
  const sortedApproaches = approaches.map((approach) => {
    const sortedDetectors = approach.detectors.sort((a, b) => {
      return a.detectorChannel - b.detectorChannel
    })
    return { ...approach, detectors: sortedDetectors }
  })

  const addApproach = useLocationStore((state) => state.addApproach)

  const [showSummary, setShowSummary] = useState(false)

  const handleAddApproach = useCallback(() => {
    addApproach()
  }, [addApproach])

  const combinedLocation = { ...location, approaches: sortedApproaches }

  return (
    <Box sx={{ minHeight: '400px' }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          alignItems: 'center',
          gap: 2,
          mb: 2,
        }}
      >
        <AddButton label="New Approach" onClick={handleAddApproach} />
        <Button
          variant="outlined"
          onClick={() => setShowSummary((prev) => !prev)}
        >
          {showSummary ? 'Hide Summary' : 'Summary'}
        </Button>
      </Box>

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

      {approaches.length > 0 ? (
        approaches.map((approach) => (
          <EditApproach key={approach.id} approach={approach} />
        ))
      ) : (
        <Box sx={{ p: 2, mt: 2, textAlign: 'center' }}>
          <Typography variant="caption" fontStyle="italic">
            No approaches found
          </Typography>
        </Box>
      )}
    </Box>
  )
}
