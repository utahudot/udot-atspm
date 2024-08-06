import ApacheEChart from '@/features/charts/components/apacheEChart'
import { useCongestionTrackingChart } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import DetailsPanel from '@/features/speedManagementTool/components/detailsPanel'
import OptionsPanel from '@/features/speedManagementTool/components/optionsPanel'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import { Box, IconButton, Paper } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useRef, useState } from 'react'

const SpeedManagementMap = dynamic(() => import('./Map'), { ssr: false })

const Map = () => {
  const [leftSidebarOpen, setLeftSidebarOpen] = useState(true)
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

  const toggleSidebar = () => setLeftSidebarOpen(!leftSidebarOpen)

  return (
    <>
      <Box
        ref={fullScreenRef}
        sx={{ display: 'flex', flex: 1, overflow: 'hidden' }}
      >
        <Box
          sx={{
            position: 'relative',
            backgroundColor: '#fff',
            width: leftSidebarOpen ? '400px' : '0px',
            transition: 'width 0.3s',
          }}
        >
          <Box
            sx={{
              width: '400px',
              overflow: 'hidden',
            }}
          >
            <OptionsPanel />
          </Box>
          <Box
            sx={{
              borderRadius: '3px',
              backgroundColor: 'primary.main',
              position: 'absolute',
              top: '50%',
              left: leftSidebarOpen ? '420px' : '50px',
              transform: 'translate(-100%, -50%)',
              zIndex: 100,
              transition: 'left 0.3s',
            }}
          >
            <IconButton
              type="button"
              onClick={toggleSidebar}
              sx={{
                color: '#fff',
                p: 0.7,
                transform: leftSidebarOpen
                  ? 'rotateY(0deg)'
                  : 'rotateY(180deg)',
                transition: 'transform 0.3s',
              }}
            >
              <ChevronLeftIcon />
            </IconButton>
          </Box>
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
        <Box sx={{ width: '370px', backgroundColor: '#fff', overflow: 'auto' }}>
          <DetailsPanel selectedRouteId={selectedRouteId} />
        </Box>
      </Box>
      <Box>
        {selectedRouteId ? (
          <Box
            sx={{
              height: '500px',
              overflowY: 'auto',
              zIndex: 1000000000000000000000,
            }}
          >
            <CongestionTrackingChartsContainer
              selectedRouteId={selectedRouteId}
            />
          </Box>
        ) : null}
      </Box>
    </>
  )
}

export default memo(Map)

const CongestionTrackingChartsContainer = ({
  selectedRouteId,
}: {
  selectedRouteId: number
}) => {
  const { data: congestionTrackingData } = useCongestionTrackingChart({
    options: {
      segmentId: selectedRouteId.toString(),
      startDate: '2022-01-01T00:00:00',
      endDate: '2022-01-01T23:59:59',
    },
  })
  if (!congestionTrackingData) return null

  return (
    // align in center
    <Box sx={{ display: 'flex', justifyContent: 'center' }}>
      <Paper sx={{ p: 4, my: 3, width: '1000px', marginLeft: '2px' }}>
        <ApacheEChart
          id="congestion-chart"
          option={congestionTrackingData}
          style={{ width: '900px', height: '600px' }}
        />
      </Paper>
    </Box>
  )
}
