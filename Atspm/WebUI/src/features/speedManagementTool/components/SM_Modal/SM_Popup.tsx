import SM_Charts from '@/features/speedManagementTool/components/SM_Charts'
import ChartsContainer from '@/features/speedManagementTool/components/SM_Modal/ChartsContainer'
import { getDataSourceName } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import ArrowBackIosNewOutlinedIcon from '@mui/icons-material/ArrowBackIosNewOutlined'
import EastOutlinedIcon from '@mui/icons-material/EastOutlined'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogContent,
  Divider,
  Tab,
  Typography,
} from '@mui/material'
import dynamic from 'next/dynamic'
import { useEffect, useState } from 'react'

const StaticMap = dynamic(() => import('./StaticMap'), { ssr: false })

type SM_PopupProps = {
  routes: SpeedManagementRoute[]
  open: boolean
  onClose: () => void
}

const SM_Popup = ({ routes, open, onClose }: SM_PopupProps) => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()
  const [currentTab, setCurrentTab] = useState('1')

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  useEffect(() => {
    if (routes.length > 1) {
      // Automatically switch to "Charts" tab if multiple routes
      setCurrentTab('2')
    }
  }, [routes])

  const formattedDateRange =
    routes[0]?.properties.startdate && routes[0]?.properties.enddate
      ? `for ${new Date(
          routes[0].properties.startdate
        ).toLocaleDateString()} - ${new Date(
          routes[0].properties.enddate
        ).toLocaleDateString()}`
      : null

  const getDataSourceColor = (sourceId: number) => {
    switch (sourceId) {
      case 1:
        return '#2196f3'
      case 2:
        return '#f44336'
      case 3:
        return '#4caf50'
      default:
        return '#000'
    }
  }

  const InfoBox = ({ label, value }: { label: string; value: string }) => (
    <Box
      display="flex"
      flexDirection="column"
      justifyContent="center"
      alignItems="center"
      bgcolor="rgba(33, 150, 243, 0.2)"
      borderRadius={1}
      padding="10px"
      marginRight="10px"
      minWidth={200}
    >
      <Typography variant="subtitle2">{label}</Typography>
      <Typography variant="body1" sx={{ fontWeight: 'bold' }}>
        {value}
      </Typography>
    </Box>
  )

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
        <Chip
          label={getDataSourceName(submittedRouteSpeedRequest.sourceId)}
          sx={{
            backgroundColor: getDataSourceColor(
              submittedRouteSpeedRequest.sourceId
            ),
            color: 'white',
          }}
        />
      </Box>
      <DialogContent sx={{ px: 0 }}>
        <TabContext value={currentTab}>
          <Box display={'flex'} gap={2} ml={2}>
            <Typography variant="h2" display="flex" alignItems="center">
              {routes.length > 1 ? (
                <>
                  {routes[0]?.properties.name}
                  <EastOutlinedIcon sx={{ mx: 3 }} />
                  {routes[routes.length - 1]?.properties.name}
                </>
              ) : (
                routes[0]?.properties.name
              )}
            </Typography>
            {formattedDateRange && (
              <Typography variant="subtitle1">{formattedDateRange}</Typography>
            )}
            <TabList
              onChange={handleTabChange}
              aria-label="SM Popup Tabs"
              centered
              sx={{ visibility: routes.length > 1 ? 'hidden' : 'visible' }}
            >
              <Tab label="Overview" value="1" />
              <Tab label="Charts" value="2" />
            </TabList>
          </Box>
          <Divider />
          {routes.length === 1 && (
            <TabPanel value="1" sx={{ p: 0 }}>
              <Box width={'100%'} my={2}>
                <StaticMap routes={routes} />
                <Box display="flex" padding="10px">
                  <InfoBox
                    label="Speed Limit"
                    value={
                      routes[0]?.properties.speedLimit
                        ? `${routes[0]?.properties.speedLimit} mph`
                        : 'No Data'
                    }
                  />
                  <InfoBox
                    label="Average Speed"
                    value={
                      routes[0]?.properties.avg
                        ? `${routes[0]?.properties.avg} mph`
                        : 'No Data'
                    }
                  />
                </Box>
                <Divider />
                <ChartsContainer
                  selectedRouteId={routes[0]?.properties.route_id}
                />
              </Box>
            </TabPanel>
          )}
          <TabPanel value="2" sx={{ p: 0 }}>
            <SM_Charts routes={routes} />
          </TabPanel>
        </TabContext>
      </DialogContent>
    </Dialog>
  )
}

export default SM_Popup
