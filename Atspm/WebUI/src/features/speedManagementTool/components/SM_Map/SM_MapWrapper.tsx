import { useGetRouteSpeeds } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useUdotSpeedLimitRoutes } from '@/features/speedManagementTool/api/getUdotSpeedLimitRoutes'
import HotspotSidebar from '@/features/speedManagementTool/components/SM_Map/HotspotSidebar'
import SM_Popup from '@/features/speedManagementTool/components/SM_Modal/SM_Popup'
import SM_TopBar from '@/features/speedManagementTool/components/SM_Topbar/SM_Topbar'
import {
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import CloseIcon from '@mui/icons-material/Close'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  Tab,
  Typography,
} from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo, useEffect, useMemo, useRef, useState } from 'react'
import { ExportableReports } from '../ExportableReports/ExportableReports'

export const SM_Height = 'calc(100vh - 250px)'

const SpeedManagementMap = dynamic(() => import('./SM_Map'), {
  ssr: false,
})

type Coords = [number, number] | [number, number][]

function isLonLatPair(c: Coords): c is [number, number] {
  return (
    Array.isArray(c) &&
    c.length === 2 &&
    typeof c[0] === 'number' &&
    typeof c[1] === 'number'
  )
}

function swapCoordinates(coords: Coords): Coords {
  if (isLonLatPair(coords)) {
    const [x, y] = coords
    return [y, x]
  }
  return coords.map(swapCoordinates) as [number, number][]
}

const SM_MapWrapper = () => {
  const [currentTab, setCurrentTab] = useState('1')
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

  const handleTabChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  const [isRequestChanged, setIsRequestChanged] = useState(false)

  useEffect(() => {
    const change =
      JSON.stringify(routeSpeedRequest) !==
      JSON.stringify(submittedRouteSpeedRequest)

    setIsRequestChanged(change)
  }, [routeSpeedRequest, submittedRouteSpeedRequest])

  const { mutateAsync: fetchRoutes, isLoading } = useGetRouteSpeeds()

  useEffect(() => {
    const fetchData = async () => {
      const response = await fetchRoutes({ data: submittedRouteSpeedRequest })
      setRouteData(response as unknown as RoutesResponse)
    }
    fetchData()
  }, [submittedRouteSpeedRequest, fetchRoutes])

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
  }

  useEffect(() => {
    if (!multiselect) {
      setSelectedRouteIds([])
    }
  }, [multiselect])

  const { data: speedLimitData } = useUdotSpeedLimitRoutes()

  const speedLimitRoutes = useMemo(
    () =>
      speedLimitData?.features?.map((feature) => {
        return {
          ...feature,
          geometry: {
            ...feature.geometry,
            coordinates: swapCoordinates(feature.geometry.coordinates),
          },
          properties: feature.properties,
        }
      }) || [],
    [speedLimitData]
  )

  const filteredRoutes = useMemo(
    () => routeData?.features.filter((route) => route?.geometry?.coordinates),
    [routeData]
  )

  const routes = useMemo(
    () =>
      filteredRoutes?.map((feature) => ({
        ...feature,
        geometry: {
          ...feature.geometry,
          coordinates: swapCoordinates(feature.geometry.coordinates),
        },
        properties: feature.properties,
      })) || [],
    [filteredRoutes]
  )

  const handleRouteSelection = (routeId: string) => {
    if (multiselect) {
      setSelectedRouteIds((prevSelectedRouteIds) =>
        prevSelectedRouteIds.includes(routeId)
          ? prevSelectedRouteIds.filter((id) => id !== routeId)
          : [...prevSelectedRouteIds, routeId]
      )
    } else {
      setSelectedRouteIds([routeId])
      setShowPopup(true)
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
      <TabContext value={currentTab}>
        <TabList
          onChange={handleTabChange}
          aria-label="Speed Management options"
          textColor="primary"
          indicatorColor="primary"
        >
          <Tab label="Map" value="1" />
          <Tab label="Reports" value="2" />
        </TabList>
        <TabPanel value="1" sx={{ padding: '0px' }}>
          <SM_TopBar
            handleOptionClick={handleOptionClick}
            isLoading={isLoading}
            isRequestChanged={isRequestChanged}
            routes={routes}
          />
          <Box sx={{ display: 'flex', flex: 1 }}>
            <Box
              sx={{
                width: multiselect ? 220 : 0,
                display: 'flex',
                flexDirection: 'column',
                overflow: 'hidden',
                bgcolor: 'background.paper',
                borderTop: 'none',
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
                <Typography variant="subtitle2">Selected Segments</Typography>
              </Box>

              <List sx={{ flex: 1, overflowY: 'auto' }}>
                {selectedRouteIds.map((routeId) => {
                  const route = routes.find(
                    (route) => route.properties.route_id === routeId
                  )
                  return (
                    <ListItem key={routeId}>
                      <ListItemText
                        primary={route?.properties.name || 'Unknown Segment'}
                        sx={{ overflow: 'hidden', textOverflow: 'ellipsis' }}
                      />
                      <ListItemSecondaryAction>
                        <IconButton
                          edge="end"
                          aria-label="delete"
                          onClick={() =>
                            setSelectedRouteIds((prevSelectedRouteIds) =>
                              prevSelectedRouteIds.filter(
                                (id) => id !== routeId
                              )
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
              <Box sx={{ p: 2 }}>
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
            <Box sx={{ flex: 1, height: '100%', position: 'relative' }}>
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

              {/* Loading Overlay */}
              {isLoading && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    width: '100%',
                    height: '100%',
                    backgroundColor: 'rgba(0, 0, 0, 0.3)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    zIndex: 10,
                  }}
                >
                  <CircularProgress />
                </Box>
              )}
            </Box>
            <HotspotSidebar handleRouteSelection={handleRouteSelection} />
          </Box>

          {showPopup && (
            <SM_Popup
              key={selectedRouteIds.join()}
              routes={routes.filter((route) =>
                selectedRouteIds.includes(route.properties.route_id)
              )}
              onClose={handleClosePopup}
              open={showPopup}
            />
          )}
        </TabPanel>
        <TabPanel value="2" sx={{ padding: '0px' }}>
          <ExportableReports />
        </TabPanel>
      </TabContext>
    </Box>
  )
}

export default memo(SM_MapWrapper)
