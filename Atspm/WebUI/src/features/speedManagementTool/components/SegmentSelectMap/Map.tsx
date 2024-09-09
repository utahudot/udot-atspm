// import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import { memo, useEffect, useState } from 'react'
import { MapContainer, TileLayer } from 'react-leaflet'
import { getEnv } from '@/lib/getEnv'

// type SpeedMapProps = {
//   fullScreenRef?: React.RefObject<HTMLDivElement> | null // Nullable fullScreenRef
//   routes: SpeedManagementRoute[] // Array of routes
//   setSelectedRouteId?: ((routeId: number) => void) | null // Nullable setSelectedRouteId
// }

const Map = () => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  //   const getColor = (route: SpeedManagementRoute) => {
  //     let field
  //     switch (routeRenderOption) {
  //       case RouteRenderOption.Violations:
  //         field = 'estimatedViolations'
  //         break
  //       case RouteRenderOption.Posted_Speed:
  //         field = 'Speed_Limit'
  //         break
  //       case RouteRenderOption.Average_Speed:
  //         field = 'avg'
  //         break
  //       case RouteRenderOption.Percentile_85th:
  //         field = 'percentilespd_85'
  //         break
  //       case RouteRenderOption.Percentile_95th:
  //         field = 'percentilespd_95'
  //         break
  //       case RouteRenderOption.Percentile_99th:
  //         field = 'percentilespd_99'
  //         break
  //       default:
  //         field = 'avg'
  //         break
  //     }

  //     const val = route.properties[
  //       field as keyof SpeedManagementRoute['properties']
  //     ] as number

  //     if (routeRenderOption === RouteRenderOption.Violations) {
  //       if (val <= mediumMin) return ViolationColors.Low
  //       if (val < mediumMax) return ViolationColors.Medium
  //       return ViolationColors.High
  //     }

  //     if (val === null) return '#000'

  //     if (val < 20) return 'rgba(0, 115, 255, 1)'
  //     if (val < 30) return 'rgba(0, 255, 170, 1)'
  //     if (val < 35) return 'rgba(55, 255, 0, 1)'
  //     if (val < 40) return 'rgba(175, 250, 0, 1)'
  //     if (val < 45) return 'rgba(247, 214, 0, 1)'
  //     if (val < 55) return 'rgba(245, 114, 0, 1)'
  //     if (val < 65) return 'rgba(245, 57, 0, 1)'
  //     if (val < 75) return 'rgba(245, 0, 0, 1)'
  //     return '#000'
  //   }

  const renderer = L.canvas({ tolerance: 5 }) // Increase clickability of polylines

  //   const handleDisplaySettingsClick = (
  //     event: React.MouseEvent<HTMLButtonElement>
  //   ) => {
  //     setAnchorEl(anchorEl ? null : event.currentTarget)
  //   }

  //   const open = Boolean(anchorEl)
  //   const id = open ? 'settings-popover' : undefined

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      setInitialLatLong([
        parseFloat(env.MAP_DEFAULT_LATITUDE),
        parseFloat(env.MAP_DEFAULT_LONGITUDE),
      ])
    }
    fetchEnv()
  }, [])

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      {initialLatLong && <MapContainer
        center={initialLatLong}
        zoom={10}
        scrollWheelZoom={true}
        style={{
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        renderer={renderer}
        ref={setMapRef}
        doubleClickZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_bright/{z}/{x}/{y}{r}.png"
        />

        {/* 
        {routes.map((route, index) => (
          <Polyline
            key={index}
            pathOptions={{ color: getColor(route) }}
            positions={route.geometry.coordinates}
            weight={2.5}
            eventHandlers={{
              click: () =>
                setSelectedRouteId &&
                setSelectedRouteId(route.properties.route_id),
              mouseover: (e) => {
                e.target.setStyle({ weight: 4, color: 'blue' })
              },
              mouseout: (e) => {
                e.target.setStyle({
                  weight: 2.5,
                  color: getColor(route),
                })
              },
            }}
          />
        ))} */}
      </MapContainer>}
    </Box>
  )
}

export default memo(Map)
