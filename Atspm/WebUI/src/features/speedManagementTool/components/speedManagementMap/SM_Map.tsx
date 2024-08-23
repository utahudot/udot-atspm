import FullScreenToggleButton from '@/components/fullScreenLayoutButton'
import RoutesToggle from '@/features/speedManagementTool/components/detailsPanel/RoutesToggle'
import ViolationRangeSlider from '@/features/speedManagementTool/components/detailsPanel/ViolationRangeSlider'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import { getEnv } from '@/lib/getEnv'
import DisplaySettingsOutlinedIcon from '@mui/icons-material/DisplaySettingsOutlined'
import { Box, Button, Paper, Popper } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import React, { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
import SpeedLegend from './Legend'

type SpeedMapProps = {
  fullScreenRef?: React.RefObject<HTMLDivElement> | null
  routes: SpeedManagementRoute[]
  setSelectedRouteId?: ((routeId: number) => void) | null
}

const SpeedMap = ({
  fullScreenRef = null,
  routes,
  setSelectedRouteId = null,
}: SpeedMapProps) => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )

  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()

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

    if (val === null) return '#000'

    if (val < 20) return 'rgba(0, 115, 255, 1)'
    if (val < 30) return 'rgba(0, 255, 170, 1)'
    if (val < 35) return 'rgba(55, 255, 0, 1)'
    if (val < 40) return 'rgba(175, 250, 0, 1)'
    if (val < 45) return 'rgba(247, 214, 0, 1)'
    if (val < 55) return 'rgba(245, 114, 0, 1)'
    if (val < 65) return 'rgba(245, 57, 0, 1)'
    if (val < 75) return 'rgba(245, 0, 0, 1)'
    return '#000'
  }

  const renderer = L.canvas({ tolerance: 5 }) // Increase clickability of polylines

  const handleDisplaySettingsClick = (
    event: React.MouseEvent<HTMLButtonElement>
  ) => {
    setAnchorEl(anchorEl ? null : event.currentTarget)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'settings-popover' : undefined

  if (!initialLatLong) {
    return <div>Loading...</div>
  }

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <MapContainer
        center={initialLatLong}
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
        doubleClickZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_bright/{z}/{x}/{y}{r}.png"
        />
        {fullScreenRef && (
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
        )}
        <Box
          sx={{
            position: 'absolute',
            right: '10px',
            top: fullScreenRef ? '50px' : '10px',
            zIndex: 1000,
          }}
        >
          <Button
            sx={{
              px: 1,
              minWidth: 0,
            }}
            variant="contained"
            onClick={handleDisplaySettingsClick}
          >
            <DisplaySettingsOutlinedIcon fontSize="small" />
          </Button>
          <Popper
            id={id}
            open={open}
            anchorEl={anchorEl}
            placement="bottom-start"
            disablePortal
          >
            <Paper sx={{ width: '300px' }}>
              <RoutesToggle />
              <ViolationRangeSlider />
            </Paper>
          </Popper>
        </Box>
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
        ))}
        <SpeedLegend />
      </MapContainer>
    </Box>
  )
}

export default memo(SpeedMap)
