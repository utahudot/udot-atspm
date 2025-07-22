import { AddButton } from '@/components/addButton'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import { NavigationProvider } from '@/features/locations/components/Cell/CellNavigation'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useUnsavedGuard } from '@/hooks/useUnsavedGuard'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Button, Divider, Paper, Tab, Typography } from '@mui/material'
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
  const location = useLocationStore((state) => state.location)
  const approaches = useLocationStore((state) => state.approaches)

  const addApproach = useLocationStore((state) => state.addApproach)

  const [showSummary, setShowSummary] = useState(false)

  const handleAddApproach = useCallback(() => {
    addApproach()
  }, [addApproach])

  const combinedLocation = { ...location, approaches }

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
    </Box>
  )
}
