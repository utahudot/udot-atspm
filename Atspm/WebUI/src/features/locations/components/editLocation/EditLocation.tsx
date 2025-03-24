import ApproachOptions from '@/features/locations/components/ApproachOptions/ApproachOptions'
import EditApproach from '@/features/locations/components/editApproach/EditApproach'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import EditGeneralLocation from '@/features/locations/components/editLocation/editGeneralLocation'
import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab, Typography } from '@mui/material'
import React, { memo, useCallback, useState } from 'react'
import EditLocationHeader from './EditLocationHeader'
import WatchdogEditor from './WatchdogEditor'

function EditLocation() {
  const location = useLocationStore((state) => state.location)
interface EditLocationProps {
  handler: LocationConfigHandler
  updateLocationVersion: (location: Location | null) => void
}

const EditLocation = ({
  handler,
  updateLocationVersion,
}: EditLocationProps) => {
  const { activeStep } = useLocationWizardStore()
  const [currentTab, setCurrentTab] = useState('1')

  const handleTabChange = useCallback(
    (_: React.SyntheticEvent, newTab: string) => {
      setCurrentTab(newTab)
    },
    []
  )

  if (!location) return null
  useEffect(() => {
    if (activeStep === 0) {
      setCurrentTab('1')
    } else if (activeStep === 1) {
      setCurrentTab('2')
    } else if (activeStep === 2) {
      setCurrentTab('3')
    }
  }, [activeStep])

  useEffect(() => {
    setSortedApproaches(handler.approaches)
  }, [handler.approaches])

  return (
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
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <AddButton
          label="New Approach"
          onClick={handleAddApproach}
          sx={{ mb: 0.2 }}
        />
      </Box>

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
