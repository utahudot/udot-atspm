import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import SM_TopBar from '@/features/speedManagementTool/components/SM_Topbar'
import SM_Popup from '@/features/speedManagementTool/components/speedManagementMap/SM_Popup'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useRef, useState } from 'react'

const SpeedManagementMap = dynamic(() => import('./SM_Map'), { ssr: false })

const Map = () => {
  const [selectedRouteId, setSelectedRouteId] = useState<number | undefined>()
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
        height: `calc(100vh - 110px)`,
        flexDirection: 'column',
      }}
    >
      <SM_TopBar routes={routes} />
      <Box display={'flex'}>
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
      {selectedRouteId && (
        <SM_Popup
          route={
            routes.find(
              (route) => route.properties.route_id === selectedRouteId
            ) || { properties: {} }
          }
          onClose={() => setSelectedRouteId(undefined)}
          open={!!selectedRouteId}
        />
      )}
    </Box>
  )
}

export default memo(Map)
