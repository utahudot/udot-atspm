import ApproachOptions from '@/features/locations/components/ApproachOptions/ApproachOptions'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Button, Modal, Tab, Typography } from '@mui/material'
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

  useEffect(() => {
    if (activeStep === 0) {
      setCurrentTab('1')
    } else if (activeStep === 1) {
      setCurrentTab('2')
    } else if (activeStep === 2) {
      setCurrentTab('3')
    }
  }, [activeStep])

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
        updateSavedApproachesFromCurrent() // Sync saved state before switching
        setCurrentTab(pendingTab)
        setPendingTab(null)
      } else if (pendingRoute) {
        updateSavedApproachesFromCurrent() // Sync saved state before leaving
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

  useEffect(() => {
    return () => {
      resetStore() // Reset store when component unmounts
    }
  }, [resetStore])

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
            border: 'none',
            borderRadius: '10px',
            boxShadow: 24,
            p: 4,
          }}
        >
          <Typography id="leave-confirmation" sx={{ fontWeight: 'bold' }}>
            {' '}
            Unsaved Changes
          </Typography>
          <Typography>
            There are unsaved changes. Are you sure you want to{' '}
            {pendingTab ? 'switch tabs' : 'navigate away'}?
          </Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => handleDialogClose(false)} color="inherit">
              Cancel
            </Button>
            <Button
              variant="contained"
              onClick={() => handleDialogClose(true)}
              color="primary"
            >
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
  const [approachIds, addApproach] = useLocationStore((state) => [
    state.approaches.map((a) => a.id),
    state.addApproach,
  ])

  const handleAddApproach = useCallback(() => {
    addApproach()
  }, [addApproach])

  return (
    <Box sx={{ minHeight: '400px' }}>
      <ApproachOptions />
      {approachIds.map((id) => (
        <ApproachWrapper key={id} approachId={id} />
      ))}

      {approachIds.length === 0 && (
        <Box sx={{ p: 2, mt: 2, textAlign: 'center' }}>
          <Typography variant="caption" fontStyle="italic">
            No approaches found
          </Typography>
        </Box>
      )}
    </Box>
  )
}

function ApproachWrapper({ approachId }: { approachId: number }) {
  const approach = useLocationStore((state) =>
    state.approaches.find((a) => a.id === approachId)
  )

  if (!approach) return null

  return <EditApproach approach={approach} />
}
