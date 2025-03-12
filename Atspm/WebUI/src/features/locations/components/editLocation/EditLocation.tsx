import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import { AddButton } from '@/components/addButton'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditGeneralLocation from '@/features/locations/components/editLocation/editGeneralLocation'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import EditDevices from './EditDevices'
import EditLocationHeader from './EditLocationHeader'
import WatchdogEditor from './WatchdogEditor'
import {
  ApproachForConfig,
  LocationConfigHandler,
} from './editLocationConfigHandler'

interface EditLocationProps {
  handler: LocationConfigHandler
  updateLocationVersion: (location: Location | null) => void
}

const EditLocation = ({
  handler,
  updateLocationVersion,
}: EditLocationProps) => {
  const [currentTab, setCurrentTab] = useState('1')
  const [sortedApproaches, setSortedApproaches] = useState<ApproachForConfig[]>(
    []
  )

  useEffect(() => {
    setSortedApproaches(handler.approaches)
  }, [handler.approaches])

  if (!handler.expandedLocation) return null

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  return (
    <TabContext value={currentTab}>
      <EditLocationHeader
        location={handler.expandedLocation}
        updateLocationVersion={updateLocationVersion}
        refetchLocation={handler.refetchLocation}
      />
      <TabList onChange={handleTabChange} aria-label="Location Tabs">
        <Tab label="General" value="1" />
        <Tab label="Devices" value="2" />
        <Tab label="Approaches" value="3" />
        <Tab label="Watchdog" value="4" />
      </TabList>
      <TabPanel value="1" sx={{ padding: '0px' }}>
        <EditGeneralLocation
          handleLocationEdit={handler.handleLocationEdit}
          updateLocation={handler.updateExpandedLocation}
          location={handler.expandedLocation}
        />
      </TabPanel>
      <TabPanel value="2" sx={{ padding: '0px', marginBottom: '100px' }}>
        <EditDevices locationId={handler.expandedLocation.id} />
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
              onClick={handler.handleAddNewApproach}
              sx={{ mb: 0.2 }}
            />
          </Box>
          {sortedApproaches?.map((approach) => (
            <EditApproach
              key={approach.id}
              approach={approach}
              handler={handler}
            />
          ))}
          {sortedApproaches.length === 0 && (
            <Box sx={{ p: 2, mt: 2, textAlign: 'center' }}>
              <Typography variant="caption" fontStyle={'italic'}>
                No approaches found
              </Typography>
            </Box>
          )}
        </Box>
      </TabPanel>
      <TabPanel value="4" sx={{ padding: 0 }}>
        <WatchdogEditor handler={handler} />
      </TabPanel>
    </TabContext>
  )
}

export default EditLocation
