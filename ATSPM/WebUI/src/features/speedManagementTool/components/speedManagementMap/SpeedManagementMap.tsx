import CongestionTrackingChartsContainer from '@/features/charts/congestionTracker/components/CongestionTrackerChartsContainer'
import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import ChartOptions from '@/features/speedManagementTool/components/ChartOptions/ChartOptions'
import OptionsPanel from '@/features/speedManagementTool/components/optionsPanel'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useRef, useState } from 'react'

const SpeedManagementMap = dynamic(() => import('./Map'), { ssr: false })

const Map = () => {
  const [selectedRouteId, setSelectedRouteId] = useState<number>()
  const fullScreenRef = useRef<HTMLDivElement>(null)

  const { submittedRouteSpeedRequest, routeRenderOption } =
    useSpeedManagementStore()

  const { data: routeData } = useRoutes({
    options: submittedRouteSpeedRequest,
  })

  const { data: speedLimitData } = useUdotSpeedLimitRoutes()

  const speedLimitRoutes =
    speedLimitData?.features.map((feature) => {
      return {
        ...feature,
        geometry: {
          ...feature.geometry,
          coordinates: feature.geometry.coordinates.map((coord) => [
            coord[1],
            coord[0],
          ]),
        },
        properties: feature.properties,
      }
    }) || []

  const filteredRoutes = routeData?.features.filter(
    (route) => route?.geometry?.coordinates
  )

  const routes =
    filteredRoutes?.map((feature) => ({
      ...feature,
      geometry: {
        ...feature.geometry,
        coordinates: feature.geometry.coordinates.map((coord) => [
          coord[1],
          coord[0],
        ]),
      },
      properties: feature.properties,
    })) || []

  return (
    <Box
      ref={fullScreenRef}
      sx={{
        display: 'flex',
        flex: 1,
        overflow: 'hidden',
        height: `calc(100vh - 180px)`,
      }}
    >
      <Box
        sx={{
          overflowY: 'auto',
          overflowX: 'hidden',
          backgroundColor: 'background.default',
        }}
      >
        <Sidebar selectedRouteId={selectedRouteId} />
      </Box>
      <Box sx={{ flex: 1, height: '100%' }}>
        <SpeedManagementMap
          fullScreenRef={fullScreenRef}
          routes={
            routeRenderOption === RouteRenderOption.Posted_Speed
              ? speedLimitRoutes
              : routes
          }
          setSelectedRouteId={setSelectedRouteId}
        />
      </Box>
    </Box>
  )
}

export default memo(Map)

const Sidebar = ({ selectedRouteId }) => {
  const [currentTab, setCurrentTab] = useState('1')
  const [chartType, setChartType] = useState('CongestionTracker')

  const handleChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  return (
    <TabContext value={currentTab}>
      <Box
        sx={{
          borderLeft: 'thin solid rgba(0, 0, 0, 0.12)',
          display: 'flex',
          flexDirection: 'column',
          backgroundColor: 'background.paper',
        }}
      >
        <Box
          sx={{
            position: 'sticky',
            top: 0,
            backgroundColor: 'background.paper',

            zIndex: 10,
            borderTop: 'thin solid rgba(0, 0, 0, 0.12)',
            borderBottom: 'thin solid rgba(0, 0, 0, 0.12)',
          }}
        >
          <TabList
            onChange={handleChange}
            aria-label="Tab options"
            textColor="primary"
            indicatorColor="primary"
          >
            <Tab label="Routes" value="1" />
            <Tab label="Charts" value="2" />
          </TabList>
        </Box>
        <TabPanel value="1" sx={{ width: '450px' }}>
          <OptionsPanel />
        </TabPanel>
        <TabPanel value="2" sx={{ width: '900px', p: 0 }}>
          <Box sx={{ overflowY: 'auto', overflowX: 'hidden' }}>
            <Box mb={2} p={2} width="300px">
              <ChartOptions />
            </Box>
            <Box minHeight={'550px'}>
              {selectedRouteId && (
                <CongestionTrackingChartsContainer
                  selectedRouteId={selectedRouteId}
                />
              )}
            </Box>
          </Box>
        </TabPanel>
      </Box>
    </TabContext>
  )
}
