import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import SM_TopBar from '@/features/speedManagementTool/components/SM_Topbar'
import SM_Popup from '@/features/speedManagementTool/components/speedManagementMap/Popup'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useEffect, useRef, useState } from 'react'
import ReactDOM from 'react-dom'

const SpeedManagementMap = dynamic(() => import('./Map'), { ssr: false })

const Map = () => {
  const [selectedRouteId, setSelectedRouteId] = useState<number | undefined>()
  const [isSeparateScreen, setIsSeparateScreen] = useState(false)
  const [popupWindow, setPopupWindow] = useState<Window | null>(null)
  const [isClient, setIsClient] = useState(false)

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

  useEffect(() => {
    // Indicate that the component has mounted on the client side
    setIsClient(true)
  }, [])

  const handleOpenSeparateScreen = () => {
    if (typeof window !== 'undefined') {
      const newWindow = window.open(
        '',
        '_blank',
        'width=800,height=600,scrollbars=yes,resizable=yes'
      )
      if (newWindow) {
        newWindow.document.body.innerHTML = '' // Clear the new window content
        setPopupWindow(newWindow)
        setIsSeparateScreen(true)

        // Render the popup content into the new window
        ReactDOM.render(
          <SM_Popup
            route={
              routes.find(
                (route) => route.properties.route_id === selectedRouteId
              ) || { properties: {} }
            }
            onClose={() => {
              newWindow.close()
              setPopupWindow(null)
              setIsSeparateScreen(false)
              setSelectedRouteId(undefined)
            }}
            open={true}
            isSeparateScreen={true}
            onPopBack={() => {
              newWindow.close()
              setPopupWindow(null)
              setIsSeparateScreen(false)
            }}
          />,
          newWindow.document.body
        )
      }
    }
  }

  const handlePopBack = () => {
    if (popupWindow) {
      popupWindow.close()
      setPopupWindow(null)
    }
    setIsSeparateScreen(false)
  }

  useEffect(() => {
    if (popupWindow) {
      const handleUnload = () => {
        setPopupWindow(null)
        setIsSeparateScreen(false)
      }

      popupWindow.addEventListener('beforeunload', handleUnload)
      return () => {
        popupWindow.removeEventListener('beforeunload', handleUnload)
      }
    }
  }, [popupWindow])

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
      {isClient && !isSeparateScreen && (
        <SM_Popup
          route={
            routes.find(
              (route) => route.properties.route_id === selectedRouteId
            ) || { properties: {} }
          }
          onClose={() => setSelectedRouteId(undefined)}
          open={!!selectedRouteId}
          isSeparateScreen={isSeparateScreen}
          onPopBack={handlePopBack}
          onOpenSeparateScreen={handleOpenSeparateScreen}
        />
      )}
    </Box>
  )
}

export default memo(Map)
