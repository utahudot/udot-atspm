import ApproachOptions from '@/features/locations/components/ApproachOptions/ApproachOptions'
import EditDevices from '@/features/locations/components/editLocation/EditDevices'
import LocationGeneralOptionsEditor from '@/features/locations/components/editLocation/LocationGeneralOptionsEditor'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useUnsavedGuard } from '@/hooks/useUnsavedGuard'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Tab } from '@mui/material'
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

  const handleTabChange = useCallback(
    async (_: React.SyntheticEvent, newTab: string) => {
      if (await allowNavigate(`tab:${newTab}`)) {
        setCurrentTab(newTab)
      }
    },
    [allowNavigate]
  )

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

        <TabPanel value="1" sx={{ padding: 0, minHeight: '400px' }}>
          <LocationGeneralOptionsEditor />
        </TabPanel>
        <TabPanel
          value="2"
          sx={{ padding: 0, marginBottom: '100px', minHeight: '400px' }}
        >
          <EditDevices />
        </TabPanel>
        <TabPanel value="3" sx={{ padding: 0, minHeight: '400px' }}>
          <ApproachOptions />
        </TabPanel>
        <TabPanel value="4" sx={{ padding: 0, minHeight: '400px' }}>
          <WatchdogEditor />
        </TabPanel>
      </TabContext>

      <Prompt />
    </>
  )
}

export default memo(EditLocation)
