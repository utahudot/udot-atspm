import FullScreenToggleButton from '@/components/fullScreenLayoutButton'
import { MAP_DEFAULT_LATITUDE, MAP_DEFAULT_LONGITUDE } from '@/config'
import Popup from '@/features/speedManagementTool/components/speedManagementMap/Popup'
import useSpeedManagementStore, {
  RouteRenderOption,
} from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
import SpeedLegend from './Legend'

type SpeedMapProps = {
  fullScreenRef: React.RefObject<HTMLDivElement>
  routes: SpeedManagementRoute[]
  setSelectedRouteId: (routeId: number) => void
}

const SpeedMap = ({
  fullScreenRef,
  routes,
  setSelectedRouteId,
}: SpeedMapProps) => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()

  useEffect(() => {
    if (!mapRef) return

    const mapContainer = mapRef.getContainer()
    const handleResize = () => {
      mapRef.invalidateSize()
    }

    const resizeObserver = new ResizeObserver(handleResize)
    resizeObserver.observe(mapContainer)

    return () => {
      resizeObserver.disconnect()
    }
  }, [mapRef])

  const getColor = (route: SpeedManagementRoute) => {
    let field
    switch (routeRenderOption) {
      case RouteRenderOption.Violations:
        field = 'estimatedViolations'
        break
      case RouteRenderOption.Posted_Speed:
        field = 'Speed_Limit'
        break
      case RouteRenderOption.Average_Speed:
        field = 'avg'
        break
      case RouteRenderOption.Percentile_85th:
        field = 'percentilespd_85'
        break
      case RouteRenderOption.Percentile_95th:
        field = 'percentilespd_95'
        break
      case RouteRenderOption.Percentile_99th:
        field = 'percentilespd_99'
        break
      default:
        field = 'avg'
        break
    }

    const val = route.properties[
      field as keyof SpeedManagementRoute['properties']
    ] as number

    if (routeRenderOption === RouteRenderOption.Violations) {
      if (val <= mediumMin) return ViolationColors.Low
      if (val < mediumMax) return ViolationColors.Medium
      return ViolationColors.High
    }

    if (val === null) return '#c1c9cc'

    if (val < 20) return 'rgba(0, 115, 255, 1)'
    if (val < 30) return 'rgba(0, 255, 170, 1)'
    if (val < 35) return 'rgba(55, 255, 0, 1)'
    if (val < 40) return 'rgba(175, 250, 0, 1)'
    if (val < 45) return 'rgba(247, 214, 0, 1)'
    if (val < 55) return 'rgba(245, 114, 0, 1)'
    if (val < 65) return 'rgba(245, 57, 0, 1)'
    if (val < 75) return 'rgba(245, 0, 0, 1)'
    return '#c1c9cc'
  }

  const renderer = L.canvas({ tolerance: 5 }) // Increase clickability of polylines

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <MapContainer
        center={[MAP_DEFAULT_LATITUDE, MAP_DEFAULT_LONGITUDE]}
        zoom={6}
        scrollWheelZoom={true}
        style={{
          minHeight: '700px',
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        renderer={renderer}
        ref={setMapRef}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png"
        />
        <Box
          sx={{
            position: 'absolute',
            right: '10px',
            top: '10px',
            zIndex: 1000,
          }}
        >
          <FullScreenToggleButton targetRef={fullScreenRef} />
        </Box>
        {routes.map((route, index) => {
          return (
            <div key={index}>
              <Polyline
                pathOptions={{ color: getColor(route) }}
                key={index}
                positions={route.geometry.coordinates}
                weight={2.5}
                eventHandlers={{
                  click: () => setSelectedRouteId(route.properties.route_id),
                }}
              >
                <Popup route={route} />
              </Polyline>
            </div>
          )
        })}
        <SpeedLegend />
      </MapContainer>
    </Box>
  )
}

export default memo(SpeedMap)
