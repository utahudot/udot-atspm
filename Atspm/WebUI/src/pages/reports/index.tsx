import { ResponsivePageLayout } from '@/components/ResponsivePage'
import TspReport from '@/features/tspReport/components/TspReport'
import LeftTurnGapReport from '@/pages/left-turn-gap-report'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab } from '@mui/material'
import { useState } from 'react'
function Reports() {
  const [currentTab, setCurrentTab] = useState('1')
  return (
    <ResponsivePageLayout title="Reports" noBottomMargin>
      <TabContext value={currentTab}>
        <TabList onChange={(e, value) => setCurrentTab(value)}>
          <Tab label="Left Turn Gap Report" value={'1'} />
          <Tab label="TSP Report" value={'2'} />
        </TabList>
        <Box sx={{ my: 3 }}>
          <TabPanel value="1" sx={{ padding: 0 }}>
            <LeftTurnGapReport />
          </TabPanel>
          <TabPanel value="2" sx={{ padding: 0 }}>
            <TspReport />
          </TabPanel>
        </Box>
      </TabContext>
    </ResponsivePageLayout>
  )
}
export default Reports
