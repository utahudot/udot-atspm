import { AddButton } from '@/components/addButton'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab, Typography } from '@mui/material'
import { useState } from 'react'
import EditLocationHeader from './EditLocationHeader'
import WatchdogEditor from './WatchdogEditor'

const EditLocation = () => {
  const { location, addApproach } = useLocationStore()
  const [currentTab, setCurrentTab] = useState('1')

  if (!location) return null

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  return (
    <TabContext value={currentTab}>
      <EditLocationHeader />
      <TabList onChange={handleTabChange} aria-label="Location Tabs">
        <Tab label="General" value="1" />
        <Tab label="Devices" value="2" />
        <Tab label="Approaches" value="3" />
        <Tab label="Watchdog" value="4" />
      </TabList>
      <TabPanel value="1" sx={{ padding: '0px' }}>
        <LocationGeneralOptionsEditor />
      </TabPanel>
      <TabPanel value="2" sx={{ padding: '0px', marginBottom: '100px' }}>
        <EditDevices />
      </TabPanel>
      <TabPanel value="3" sx={{ padding: 0, minHeight: '400px' }}>
        <Box sx={{ minHeight: '400px' }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'flex-end',
            }}
          >
            <AddButton
              label="New Approach"
              onClick={addApproach}
              sx={{ mb: 0.2 }}
            />
          </Box>
          {location.approaches?.map((approach) => (
            <EditApproach key={approach.id} approach={approach} />
          ))}
          {location?.approaches?.length === 0 && (
            <Box sx={{ p: 2, mt: 2, textAlign: 'center' }}>
              <Typography variant="caption" fontStyle={'italic'}>
                No approaches found
              </Typography>
            </Box>
          )}
        </Box>
      </TabPanel>
      <TabPanel value="4" sx={{ padding: 0 }}>
        <WatchdogEditor />
      </TabPanel>
    </TabContext>
  )
}

export default EditLocation
