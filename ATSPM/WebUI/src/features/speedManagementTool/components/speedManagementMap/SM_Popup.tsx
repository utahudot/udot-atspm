import ChartsContainer from '@/features/speedManagementTool/components/detailsPanel/ChartsContainer'
import SM_Charts from '@/features/speedManagementTool/components/SM_Charts'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import ArrowBackIosNewOutlinedIcon from '@mui/icons-material/ArrowBackIosNewOutlined'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  Dialog,
  DialogContent,
  Divider,
  Tab,
  Typography,
} from '@mui/material'
import dynamic from 'next/dynamic'
import { useState } from 'react'

const StaticMap = dynamic(
  () => import('@/features/speedManagementTool/components/StaticMap'),
  { ssr: false }
)

type SM_PopupProps = {
  route: SpeedManagementRoute
  open: boolean
  onClose: () => void
}

const SM_Popup = ({ route, open, onClose }: SM_PopupProps) => {
  const { name, startdate, enddate } = route.properties

  const [currentTab, setCurrentTab] = useState('1')

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  const formattedDateRange =
    startdate && enddate
      ? `for ${new Date(startdate).toLocaleDateString()} - ${new Date(
          enddate
        ).toLocaleDateString()}`
      : null

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullScreen
      maxWidth="md"
      PaperProps={{
        sx: {
          height: '100%',
          width: '100%',
          margin: 'auto',
          maxWidth: 'lg',
        },
      }}
    >
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'right',
          alignItems: 'center',
          padding: 1,
          borderBottom: '1px solid #ccc',
        }}
      >
        <Box width={'100%'}>
          <Button onClick={onClose} startIcon={<ArrowBackIosNewOutlinedIcon />}>
            Back to Map
          </Button>
        </Box>
      </Box>
      <DialogContent sx={{ px: 0 }}>
        <TabContext value={currentTab}>
          <Box display={'flex'} gap={2} ml={2}>
            <Typography variant="h2">{name}</Typography>
            {formattedDateRange && (
              <Typography variant="subtitle1">{formattedDateRange}</Typography>
            )}
            <TabList
              onChange={handleTabChange}
              aria-label="SM Popup Tabs"
              centered
            >
              <Tab label="Overview" value="1" />
              <Tab label="Charts" value="2" />
            </TabList>
          </Box>
          <Divider />
          <TabPanel value="1" sx={{ p: 0 }}>
            <Box width={'100%'} my={2}>
              <StaticMap routes={[route]} />
              <ChartsContainer selectedRouteId={route.properties.route_id} />
            </Box>
          </TabPanel>
          <TabPanel value="2" sx={{ p: 0 }}>
            <SM_Charts route={route} />
          </TabPanel>
        </TabContext>
      </DialogContent>
    </Dialog>
  )
}

export default SM_Popup
