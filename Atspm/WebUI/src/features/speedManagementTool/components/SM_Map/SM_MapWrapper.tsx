import { useGetRouteSpeeds } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import SM_Popup from '@/features/speedManagementTool/components/SM_Modal/SM_Popup'
import SM_TopBar from '@/features/speedManagementTool/components/SM_Topbar/SM_Topbar'
import {
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import CloseIcon from '@mui/icons-material/Close'
import {
  Box,
  Button,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  Typography,
} from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useEffect, useRef, useState } from 'react'

export const SM_Height = 'calc(100vh - 250px)'

const SpeedManagementMap = dynamic(() => import('./SM_Map'), {
  ssr: false,
})

const SM_MapWrapper = () => {
  const [selectedRouteIds, setSelectedRouteIds] = useState<string[]>([])
  const [showPopup, setShowPopup] = useState<boolean>(false)
  const [routeData, setRouteData] = useState<RoutesResponse>()
  const fullScreenRef = useRef<HTMLDivElement>(null)

  const {
    submittedRouteSpeedRequest,
    routeSpeedRequest,
    routeRenderOption,
    multiselect,
    setSubmittedRouteSpeedRequest,
    setMediumMin,
    setMediumMax,
    setSliderMax,
  } = useSpeedManagementStore()

  const [isRequestChanged, setIsRequestChanged] = useState(false)

  useEffect(() => {
    const change =
      JSON.stringify(routeSpeedRequest) !==
      JSON.stringify(submittedRouteSpeedRequest)

    setIsRequestChanged(change)
  }, [routeSpeedRequest, submittedRouteSpeedRequest])

  const { mutateAsync: fetchRoutes, isLoading } = useGetRouteSpeeds()

  useEffect(() => {
    if (routeData) {
      return
    }

    fetchRoutes({ data: routeSpeedRequest }).then((response) => {
      setRouteData(response as unknown as RoutesResponse)
    })
  }, [])

  const handleOptionClick = () => {
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    if (routeSpeedRequest.sourceId === DataSource.ATSPM) {
      setMediumMin(80)
      setMediumMax(300)
      setSliderMax(500)
    } else if (routeSpeedRequest.sourceId === DataSource.PeMS) {
      setMediumMin(150)
      setMediumMax(400)
      setSliderMax(600)
    }
    fetchRoutes({ data: routeSpeedRequest }).then((response) => {
      setRouteData(response as unknown as RoutesResponse)
    })
  }

  useEffect(() => {
    if (!multiselect) {
      setSelectedRouteIds([])
    }
  }, [multiselect])

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

  const handleRouteSelection = (routeId: string) => {
    if (multiselect) {
      setSelectedRouteIds((prevSelectedRouteIds) =>
        prevSelectedRouteIds.includes(routeId)
          ? prevSelectedRouteIds.filter((id) => id !== routeId)
          : [...prevSelectedRouteIds, routeId]
      )
    } else {
      setSelectedRouteIds([routeId])
      setShowPopup(true) // Show the popup for a single route
    }
  }

  const handleViewCharts = () => {
    setShowPopup(true)
  }

  const handleClosePopup = () => {
    setShowPopup(false)
  }

  return (
    <Box
      ref={fullScreenRef}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      <SM_TopBar
        handleOptionClick={handleOptionClick}
        isLoading={isLoading}
        isRequestChanged={isRequestChanged}
      />
      <Box sx={{ display: 'flex', flex: 1 }}>
        <Box
          sx={{
            width: multiselect ? 300 : 0,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden',
            bgcolor: 'background.paper',
            borderTop: 'none',
            borderLeft: '1px solid',
            borderBottom: '1px solid',
            borderColor: 'divider',
            maxHeight: SM_Height,
            overflowY: 'auto',
          }}
        >
          <Box
            sx={{
              p: 2,
              borderBottom: '1px solid',
              borderColor: 'divider',
              backgroundColor: 'background.default',
            }}
          >
            <Typography variant="subtitle2">Selected Routes</Typography>
          </Box>

          <List sx={{ flex: 1, overflowY: 'auto', minWidth: '300px' }}>
            {selectedRouteIds.map((routeId) => {
              const route = routes.find(
                (route) => route.properties.route_id === routeId
              )
              return (
                <ListItem key={routeId}>
                  <ListItemText
                    primary={route?.properties.name || 'Unknown Route'}
                  />
                  <ListItemSecondaryAction>
                    <IconButton
                      edge="end"
                      aria-label="delete"
                      onClick={() =>
                        setSelectedRouteIds((prevSelectedRouteIds) =>
                          prevSelectedRouteIds.filter((id) => id !== routeId)
                        )
                      }
                    >
                      <CloseIcon />
                    </IconButton>
                  </ListItemSecondaryAction>
                </ListItem>
              )
            })}
          </List>
          <Box sx={{ p: 2, minWidth: '300px' }}>
            <Button
              variant="contained"
              color="primary"
              fullWidth
              onClick={handleViewCharts}
              disabled={selectedRouteIds.length === 0}
            >
              View Charts
            </Button>
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
            selectedRouteIds={selectedRouteIds}
            setSelectedRouteId={handleRouteSelection}
          />
        </Box>
      </Box>

      {showPopup && (
        <SM_Popup
          routes={routes.filter((route) =>
            selectedRouteIds.includes(route.properties.route_id)
          )}
          onClose={handleClosePopup}
          open={showPopup}
        />
      )}
    </Box>
  )
}

export default memo(SM_MapWrapper)
