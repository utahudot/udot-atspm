import {
  useGetApiV1MonthlyAggregationFilteringTimePeriods,
  useGetApiV1MonthlyAggregationMonthAggClassifications,
  useGetApiV1MonthlyAggregationSpeedCategoryFilters,
  usePostApiV1MonthlyAggregationHotspots,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  CircularProgress,
  FormControl,
  LinearProgress,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { Fragment, useCallback, useEffect, useState } from 'react'

const HotspotSidebar = () => {
  const {
    submittedRouteSpeedRequest,
    hotspotRoutes,
    setHotspotRoutes,
    setHoveredHotspot,
  } = useSpeedManagementStore()
  const { mutateAsync: fetchHotspotsAsync } =
    usePostApiV1MonthlyAggregationHotspots()

  const { data: categoryFilters } =
    useGetApiV1MonthlyAggregationSpeedCategoryFilters()
  const { data: aggregationClassifications } =
    useGetApiV1MonthlyAggregationMonthAggClassifications()
  const { data: timePeriods } =
    useGetApiV1MonthlyAggregationFilteringTimePeriods()

  const [order, setOrder] = useState('DESC')
  const [limit, setLimit] = useState(25)
  const [sortBy, setSortBy] = useState(0)
  const [visibleGroups, setVisibleGroups] = useState({
    speeds: true,
    violations: true,
    percentages: true,
    flow: true,
    variability: true,
  })
  const [isLoading, setIsLoading] = useState(true)

  const fetchHotspots = useCallback(async () => {
    try {
      setIsLoading(true)
      const hotspots = await fetchHotspotsAsync({
        data: {
          category: sortBy,
          timePeriod: 0,
          aggClassification: 0,
          startTime: submittedRouteSpeedRequest.startDate,
          endTime: submittedRouteSpeedRequest.endDate,
          sourceId: submittedRouteSpeedRequest.sourceId,
          order,
          limit,
        },
      })
      setHotspotRoutes(hotspots.features)
      setIsLoading(false)
    } catch (error) {
      console.error('Failed to fetch hotspots', error)
      setIsLoading(false)
    }
  }, [
    fetchHotspotsAsync,
    order,
    limit,
    setHotspotRoutes,
    submittedRouteSpeedRequest,
    sortBy,
  ])

  useEffect(() => {
    fetchHotspots()
  }, [submittedRouteSpeedRequest, order, limit, fetchHotspots])

  const handleLimitChange = (event) => {
    setLimit(event.target.value)
  }

  const handleOrderChange = (event) => {
    setOrder(event.target.value)
  }

  const handleSortByChange = (event) => {
    setSortBy(event.target.value)
  }

  const handleGroupVisibilityChange = (event) => {
    setVisibleGroups({
      ...visibleGroups,
      [event.target.name]: event.target.checked,
    })
  }

  const progress = isLoading ? (
    <LinearProgress sx={{ height: '5px' }} />
  ) : (
    <Box height={'5px'} />
  )

  return (
    <Box
      display={'flex'}
      flexDirection={'column'}
      sx={{
        maxHeight: SM_Height,
        width: '700px',
        border: '1px solid',
        borderColor: 'divider',
        borderTop: 'none',
      }}
    >
      <Box
        sx={{
          backgroundColor: 'background.paper',
          position: 'sticky',
          top: 0,
        }}
      >
        <Box
          sx={{
            p: 2,
            borderBottom: '1px solid',
            marginLeft: '2px',
            borderColor: 'divider',
            backgroundColor: 'background.default',
            width: '100%',
          }}
        >
          <Typography variant="subtitle2">Hotspots</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
            <Typography variant="body2">Sort by</Typography>
            <FormControl size="small" sx={{ width: 250 }}>
              <Select
                labelId="sort-by-select-label"
                id="sort-by-select"
                value={sortBy || 0}
                onChange={handleSortByChange}
              >
                {categoryFilters?.map((filter) => (
                  <MenuItem key={filter.number} value={filter.number}>
                    {filter.displayName}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl size="small" sx={{ minWidth: 100 }}>
              <Select
                labelId="order-select-label"
                id="order-select"
                value={order}
                onChange={handleOrderChange}
              >
                <MenuItem value="ASC">Asc</MenuItem>
                <MenuItem value="DESC">Desc</MenuItem>
              </Select>
            </FormControl>

            <Typography variant="body2">Limit:</Typography>
            <FormControl size="small" sx={{ minWidth: 80 }}>
              <Select
                labelId="limit-select-label"
                id="limit-select"
                value={limit}
                onChange={handleLimitChange}
              >
                <MenuItem value={10}>10</MenuItem>
                <MenuItem value={25}>25</MenuItem>
                <MenuItem value={100}>100</MenuItem>
              </Select>
            </FormControl>
            {isLoading && <CircularProgress size={20} />}
          </Box>
        </Box>
      </Box>

      <Box sx={{ overflowX: 'auto' }}>
        <Table
          stickyHeader
          sx={{ backgroundColor: 'white', minHeight: '100%' }}
        >
          <TableHead>
            <TableRow>
              <TableCell>
                <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                  Rank
                </Typography>
              </TableCell>
              <TableCell>
                <Typography
                  fontSize={'.70rem'}
                  fontWeight={'bold'}
                  minWidth={'170px'}
                >
                  Hotspot
                </Typography>
              </TableCell>
              {visibleGroups.speeds && (
                <>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      Min Speed
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      Max Speed
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      Avg Speed
                    </Typography>
                  </TableCell>
                </>
              )}
              {visibleGroups.violations && (
                <>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      Violations
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {' '}
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      Extreme Violations
                    </Typography>
                  </TableCell>
                </>
              )}
              {visibleGroups.percentages && (
                <>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      % Violations
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                      % Extreme Violations
                    </Typography>
                  </TableCell>
                </>
              )}
              {visibleGroups.flow && (
                <TableCell>
                  <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                    Flow
                  </Typography>
                </TableCell>
              )}
              {visibleGroups.variability && (
                <TableCell>
                  <Typography fontSize={'.70rem'} fontWeight={'bold'}>
                    Variability
                  </Typography>
                </TableCell>
              )}
            </TableRow>
          </TableHead>
          <TableBody>
            {hotspotRoutes.map((hotspot, index) => (
              <Fragment key={hotspot.properties.route_id}>
                <TableRow
                  hover
                  onMouseEnter={() =>
                    setHoveredHotspot(hotspot.properties.route_id)
                  }
                  onMouseLeave={() => setHoveredHotspot(null)}
                >
                  <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                    {index + 1}
                  </TableCell>
                  <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                    <Typography>{hotspot.properties.name}</Typography>
                  </TableCell>
                  {visibleGroups.speeds && (
                    <>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {hotspot.properties.minSpeed}
                      </TableCell>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {hotspot.properties.maxSpeed}
                      </TableCell>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {Math.round(hotspot.properties.averageSpeed)}
                      </TableCell>
                    </>
                  )}
                  {visibleGroups.violations && (
                    <>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {hotspot.properties.violations.toLocaleString()}
                      </TableCell>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {hotspot.properties.extremeViolations.toLocaleString()}
                      </TableCell>
                    </>
                  )}
                  {visibleGroups.percentages && (
                    <>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {Math.round(hotspot.properties.percentViolations)}%
                      </TableCell>
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {Math.round(
                          hotspot.properties.percentExtremeViolations
                        )}
                        %
                      </TableCell>
                    </>
                  )}
                  {visibleGroups.flow && (
                    <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                      {hotspot.properties.flow.toLocaleString()}
                    </TableCell>
                  )}
                  {visibleGroups.variability && (
                    <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                      {Math.round(hotspot.properties.variability)}
                    </TableCell>
                  )}
                </TableRow>
              </Fragment>
            ))}
          </TableBody>
        </Table>
      </Box>
      {/* <Box sx={{ height: 150 }}></Box> */}
    </Box>
  )
}

export default HotspotSidebar
