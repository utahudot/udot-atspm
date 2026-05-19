import { Color } from '@/features/charts/utils'
import SM_Charts from '@/features/speedManagementTool/components/SM_Charts'
import SM_GeneralCharts from '@/features/speedManagementTool/components/SM_Modal/SM_GeneralCharts'
import { getDataSourceName } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import CloseIcon from '@mui/icons-material/Close'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Menu,
  Paper,
  Skeleton,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  Typography,
  tableCellClasses,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import dynamic from 'next/dynamic'
import React, { useEffect, useMemo, useState } from 'react'

const StaticMap = dynamic(() => import('./StaticMap'), { ssr: false })

export const getDataSourceColor = (sourceId: number | undefined) => {
  switch (sourceId) {
    case 1:
      return '#0e70c6'
    case 2:
      return Color.BrightRed
    case 3:
      return '#2d7f2f'
    default:
      return '#000'
  }
}

type SM_PopupProps = {
  routes: SpeedManagementRoute[]
  open: boolean
  onClose: () => void
}

const SM_Popup = ({ routes, open, onClose }: SM_PopupProps) => {
  const theme = useTheme()
  const {
    multiselect,
    submittedRouteSpeedRequest: { startDate, endDate },
  } = useSpeedManagementStore()

  const isMediumOrSmaller = useMediaQuery(theme.breakpoints.down('md'))

  const distinctRoutes = useMemo(() => {
    const map = new Map<number, SpeedManagementRoute>()
    for (const r of routes) {
      if (!map.has(r.properties.route_id)) {
        map.set(r.properties.route_id, r)
      }
    }
    return Array.from(map.values())
  }, [routes])

  const routeNames = distinctRoutes.map((r) => r.properties.name)
  let mainLabel: string | null = null
  let allRoutesFormatted: string[] = routeNames

  if (routeNames.length > 1) {
    const firstName = routeNames[0]
    const lastName = routeNames[routeNames.length - 1]
    mainLabel = `${firstName} ➝ ${lastName}`
  } else if (routeNames.length === 1) {
    mainLabel = routeNames[0]
  }

  const [currentTab, setCurrentTab] = useState('1')
  useEffect(() => {
    if (multiselect) {
      setCurrentTab('2')
    }
  }, [multiselect])

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const handleOpenMenu = (e: React.MouseEvent<HTMLButtonElement>) =>
    setAnchorEl(e.currentTarget)
  const handleCloseMenu = () => setAnchorEl(null)

  if (!routes[0]?.properties) {
    return <div>No property data available.</div>
  }

  const { properties } = routes[0]

  const formatNumber = (num: number | null, decimals = 1) =>
    num ? `${num.toFixed(decimals)} mph` : 'N/A'
  const formatPercent = (num: number | null) =>
    !num || num === 0 ? 'N/A' : `${num.toFixed(1)}%`
  const formatViolations = (count: number | null, percent: number | null) => {
    if (!count || count === 0) return 'N/A'
    return `${count.toLocaleString()} (${formatPercent(percent)} of total)`
  }
  const formatPercentDifference = (
    val: number | null,
    limit: number | null
  ) => {
    if (!val || !limit) return ''
    const diff = ((val - limit) / limit) * 100
    return `(${diff.toFixed(1)}%)`
  }

  const propertyData = [
    {
      label: 'Speed Limit',
      value: formatNumber(properties.speedLimit, 0),
    },
    {
      label: 'Violations',
      value: formatViolations(
        properties.violations,
        properties.percentViolations
      ),
    },
    {
      label: 'Max Speed',
      value: formatNumber(properties.maxSpeed),
    },
    {
      label: 'Average Speed',
      value: properties.averageSpeed
        ? `${formatNumber(properties.averageSpeed)} ${formatPercentDifference(
            properties.averageSpeed,
            properties.speedLimit
          )}`
        : 'N/A',
    },
    {
      label: 'Extreme Violations',
      value: formatViolations(
        properties.extremeViolations,
        properties.percentExtremeViolations
      ),
    },
    {
      label: 'Min Speed',
      value: formatNumber(properties.minSpeed),
    },
    {
      label: '85th Percentile Speed',
      value: properties.averageEightyFifthSpeed
        ? `${formatNumber(
            properties.averageEightyFifthSpeed
          )} ${formatPercentDifference(
            properties.averageEightyFifthSpeed,
            properties.speedLimit
          )}`
        : 'N/A',
    },
    {
      label: 'Flow',
      value: properties.flow?.toLocaleString() || 'N/A',
    },
    {
      label: 'Variability',
      value: formatNumber(properties.variability),
    },
  ]

  const columnCount = isMediumOrSmaller ? 2 : 3
  const rows = Math.ceil(propertyData.length / columnCount)
  const organizedRows = Array.from({ length: rows }, (_, rowIndex) =>
    propertyData.slice(
      rowIndex * columnCount,
      rowIndex * columnCount + columnCount
    )
  )

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullScreen
      maxWidth="md"
      aria-labelledby="route-popup-title"
      PaperProps={{
        sx: {
          height: '100%',
          width: '100%',
          margin: 'auto',
          maxWidth: 'lg',
        },
      }}
    >
      <DialogTitle sx={{ display: 'none' }} id="route-popup-title">
        Route Popup
      </DialogTitle>

      <DialogContent sx={{ p: 0 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', ml: 2 }}>
          <Chip
            label={getDataSourceName(properties.sourceId)}
            sx={{
              backgroundColor: getDataSourceColor(properties.sourceId),
              color: 'white',
            }}
            size="small"
          />
          <IconButton onClick={onClose} sx={{ marginLeft: 'auto' }}>
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>

        <TabContext value={currentTab}>
          <Box display="flex" gap={2} alignItems="center" ml={2}>
            {distinctRoutes.length > 1 ? (
              <Button
                variant="text"
                onClick={handleOpenMenu}
                sx={{ textAlign: 'left', p: 0 }}
              >
                <Typography variant="h2">
                  {mainLabel ?? <Skeleton width={300} />}
                </Typography>
              </Button>
            ) : (
              <Typography variant="h2">
                {mainLabel ?? <Skeleton width={300} />}
              </Typography>
            )}

            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleCloseMenu}
              keepMounted
            >
              {allRoutesFormatted.map((routeName, idx) => (
                <Box key={idx} sx={{ px: 2, py: 1 }}>
                  <Typography variant="h4" component="p">
                    {routeName}
                  </Typography>
                </Box>
              ))}
            </Menu>

            <TabList
              onChange={handleTabChange}
              aria-label="SM Popup Tabs"
              centered
              sx={{ visibility: multiselect ? 'hidden' : 'visible' }}
            >
              <Tab label="Overview" value="1" />
              <Tab label="Charts" value="2" />
            </TabList>
          </Box>

          <Divider />

          <TabPanel value="1" sx={{ p: 0 }}>
            <Box
              width="100%"
              my={2}
              display="flex"
              flexDirection="column"
              justifyContent="center"
            >
              <StaticMap routes={routes} />
              <Paper variant="outlined" sx={{ px: 2 }}>
                <Box display="flex" gap={2} m={1}>
                  <Box display="flex" alignItems="center">
                    <Typography variant="subtitle2" fontSize="10px">
                      Percent Observed:
                    </Typography>
                    <Typography
                      variant="body2"
                      fontSize="10px"
                      sx={{ ml: '4px' }}
                    >
                      {formatPercent(properties.percentObserved)}
                    </Typography>
                  </Box>
                  <Box display="flex" alignItems="center">
                    <Typography variant="subtitle2" fontSize="10px">
                      Time Range:
                    </Typography>
                    <Typography
                      variant="body2"
                      fontSize="10px"
                      sx={{ ml: '4px' }}
                    >
                      {`${startDate} - ${endDate}`}
                    </Typography>
                  </Box>
                </Box>
                <Divider />
                <TableContainer>
                  <Table
                    size="small"
                    sx={{
                      [`& .${tableCellClasses.root}`]: {
                        borderBottom: 'none',
                      },
                    }}
                  >
                    <TableBody>
                      {organizedRows.map((row, rowIndex) => (
                        <TableRow key={rowIndex}>
                          {row.map((prop, subIndex) => (
                            <React.Fragment key={subIndex}>
                              <TableCell>
                                <Typography variant="subtitle2">
                                  {prop.label}
                                </Typography>
                              </TableCell>
                              <TableCell
                                align="right"
                                sx={{
                                  borderRight:
                                    subIndex !== columnCount - 1
                                      ? '1px solid #e0e0e0'
                                      : 'none',
                                }}
                              >
                                <Typography variant="body2">
                                  {prop.value}
                                </Typography>
                              </TableCell>
                            </React.Fragment>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Paper>
            </Box>
            {!multiselect && (
              <SM_GeneralCharts selectedRouteId={properties.route_id} />
            )}
          </TabPanel>

          <TabPanel value="2" sx={{ p: 0 }}>
            <SM_Charts routes={routes} />
          </TabPanel>
        </TabContext>
      </DialogContent>
    </Dialog>
  )
}

export default SM_Popup
