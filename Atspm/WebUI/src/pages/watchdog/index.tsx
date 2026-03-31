import { ResponsivePageLayout } from '@/components/ResponsivePage'
import WatchdogIgnoredEventsTable from '@/features/watchdog/components/WatchdogIgnoredEventsTable'
import WatchdogLogs from '@/features/watchdog/components/WatchdogLogs'
import WatchdogSummaryReport from '@/features/watchdog/components/WatchDogSummaryReport'

import Authorization from '@/lib/Authorization'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab } from '@mui/material'
import { useState } from 'react'

const WatchDog = () => {
  const requiredClaim = 'Watchdog:View'
  const [currentTab, setCurrentTab] = useState('1')

  const handleTabChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title={'Watchdog'} noBottomMargin>
        <TabContext value={currentTab}>
          <TabList
            onChange={handleTabChange}
            aria-label="watchDog options"
            textColor="primary"
            indicatorColor="primary"
          >
            <Tab label="Logs" value="1" />
            <Tab label="Summary Report" value="2" />
            <Tab label="Ignored Events" value="3" />
          </TabList>

          <TabPanel value="1" sx={{ padding: '0px' }}>
            <Box sx={{ paddingTop: '0px' }}>
              <WatchdogLogs />
            </Box>
          </TabPanel>

          {/* ConfigurationComponent */}
          <TabPanel value="2" sx={{ padding: '0px' }}>
            <Box sx={{ paddingTop: '0px' }}>
              <WatchdogSummaryReport />
            </Box>
          </TabPanel>

          <TabPanel value="3" sx={{ padding: '0px' }}>
            <Box sx={{ paddingTop: '0px' }}>
              <WatchdogIgnoredEventsTable />
            </Box>
          </TabPanel>
        </TabContext>
      </ResponsivePageLayout>
    </Authorization>
  )
}

export default WatchDog
