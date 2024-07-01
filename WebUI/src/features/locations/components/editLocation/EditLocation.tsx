import { AddButton } from '@/components/addButton'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import { sortApproachesByPhaseNumber } from '@/features/locations/components/editApproach/utils/sortApproaches'
import EditGeneralLocation from '@/features/locations/components/editLocation/editGeneralLocation'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { Location } from '../../types'
import EditDevices from './EditDevices'
import EditLocationHeader from './EditLocationHeader'
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
    setSortedApproaches(sortApproachesByPhaseNumber(handler.approaches))
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
      </TabList>
      <TabPanel value="1" sx={{ padding: '0px' }}>
        <EditGeneralLocation
          handleLocationEdit={handler.handleLocationEdit}
          updateLocation={handler.updateExpandedLocation}
          location={handler.expandedLocation}
          refetchLocation={handler.refetchLocation}
        />
      </TabPanel>
      <TabPanel value="2" sx={{ padding: '0px', marginBottom: '100px' }}>
        <EditDevices locationId={handler.expandedLocation.id} />
      </TabPanel>
      <TabPanel value="3" sx={{ padding: 0 }}>
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
            key={approach.id ?? approach.index}
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
      </TabPanel>
    </TabContext>
  )
}

export default EditLocation
