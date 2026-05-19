import {
  useGetApiV1MonthlyAggregationSpeedCategoryFilters,
  usePostApiV1ImpactHotspots,
  usePostApiV1MonthlyAggregationHotspots,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map'
import HotspotTable from '@/features/speedManagementTool/components/SM_Map/HotspotTable'
import ImpactHotspotTable from '@/features/speedManagementTool/components/SM_Map/ImpactHotspotsTable'
import { DataSource } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { addSpaces } from '@/utils/string'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import HelpOutlineIcon from '@mui/icons-material/HelpOutline'
import {
  Box,
  Button,
  Checkbox,
  Divider,
  FormControl,
  FormControlLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Popover,
  Select,
  Typography,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { parseISO } from 'date-fns'
import { throttle } from 'lodash'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'

export const ImpactSpeedAggregationCategoryFilter = {
  ChangeInAverageSpeed: 'ChangeInAverageSpeed',
  ChangeInEightyFifthPercentileSpeed: 'ChangeInEightyFifthPercentileSpeed',
  ChangeInVariability: 'ChangeInVariability',
  ChangeInPercentViolations: 'ChangeInPercentViolations',
  ChangeInPercentExtremeViolations: 'ChangeInPercentExtremeViolations',
  BeforeFlow: 'BeforeFlow',
  BeforeMinSpeed: 'BeforeMinSpeed',
  BeforeMaxSpeed: 'BeforeMaxSpeed',
  BeforeAverageSpeed: 'BeforeAverageSpeed',
  BeforeAverageEightyFifthSpeed: 'BeforeAverageEightyFifthSpeed',
  BeforeVariability: 'BeforeVariability',
  BeforePercentViolations: 'BeforePercentViolations',
  BeforePercentExtremeViolations: 'BeforePercentExtremeViolations',
  AfterFlow: 'AfterFlow',
  AfterMinSpeed: 'AfterMinSpeed',
  AfterMaxSpeed: 'AfterMaxSpeed',
  AfterAverageSpeed: 'AfterAverageSpeed',
  AfterAverageEightyFifthSpeed: 'AfterAverageEightyFifthSpeed',
  AfterVariability: 'AfterVariability',
  AfterPercentViolations: 'AfterPercentViolations',
  AfterPercentExtremeViolations: 'AfterPercentExtremeViolations',
} as const

const flipHotspotCoordinates = (hotspots) => {
  return hotspots.features.map((feature) => {
    if (feature.geometry.geometries) {
      // Flip coordinates for each geometry in geometries
      feature.geometry.geometries = feature.geometry.geometries.map(
        (geometry) => {
          if (geometry.coordinates) {
            geometry.coordinates = geometry.coordinates.map((coord) =>
              Array.isArray(coord) ? coord.reverse() : coord
            )
          }
          return geometry
        }
      )
    } else if (feature.geometry.coordinates) {
      // Flip coordinates for geometry.coordinates
      feature.geometry.coordinates = feature.geometry.coordinates.map(
        (coord) => (Array.isArray(coord) ? coord.reverse() : coord)
      )
    }
    return feature
  })
}

const HotspotSidebar = ({ handleRouteSelection }) => {
  const {
    submittedRouteSpeedRequest,
    hotspotRoutes,
    setHotspotRoutes,
    setHoveredHotspot,
  } = useSpeedManagementStore()

  const { mutateAsync: fetchHotspotsAsync } =
    usePostApiV1MonthlyAggregationHotspots()

  const { mutateAsync: fetchImpactHotspots } = usePostApiV1ImpactHotspots()
  const { data: categoryFilters } =
    useGetApiV1MonthlyAggregationSpeedCategoryFilters()

  const [order, setOrder] = useState('DESC')
  const [limit, setLimit] = useState(25)
  const [sortBy, setSortBy] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [width, setWidth] = useState(800)
  const [isCompareDates, setIsCompareDates] = useState(false)
  const [baseMonth, setBaseMonth] = useState(
    parseISO(submittedRouteSpeedRequest.startDate)
  )
  const [comparisonMonth, setComparisonMonth] = useState(
    parseISO(submittedRouteSpeedRequest.endDate)
  )

  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const open = Boolean(anchorEl)
  const id = open ? 'hotspot-popover' : undefined

  const handleOpenPopover = (event: MouseEvent) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClosePopover = () => {
    setAnchorEl(null)
  }

  // New state for selecting the category when sortBy = 100 (Effectiveness of Strategies)
  const [selectedImpactCategory, setSelectedImpactCategory] = useState(
    ImpactSpeedAggregationCategoryFilter.ChangeInAverageSpeed
  )

  // New state for showing/hiding before/after columns
  const [showBeforeAfter, setShowBeforeAfter] = useState(false)

  useEffect(() => {
    setBaseMonth(parseISO(submittedRouteSpeedRequest.startDate))
    setComparisonMonth(parseISO(submittedRouteSpeedRequest.endDate))
  }, [submittedRouteSpeedRequest])

  const containerRef = useRef(null)
  const isDragging = useRef(false)
  const initialMouseX = useRef(0)
  const initialWidth = useRef(0)

  const fetchHotspots = useCallback(async () => {
    try {
      setIsLoading(true)
      const {
        startDate,
        endDate,
        sourceId,
        region,
        county,
        city,
        daysOfWeek,
        functionalType,
        timePeriod,
        aggClassification,
        accessCategory,
      } = submittedRouteSpeedRequest

      const startTime = isCompareDates ? baseMonth : startDate
      const endTime = isCompareDates ? comparisonMonth : endDate

      // If rank by is "Effectiveness of Strategies" (sortBy=100), use the impact hotspots API
      if (sortBy === 100) {
        const payload = {
          category: selectedImpactCategory,
          timePeriod,
          aggClassification,
          order,
          limit,
        }

        let impactHotspots = await fetchImpactHotspots({ data: payload })
        impactHotspots = flipHotspotCoordinates(impactHotspots)
        setHotspotRoutes(impactHotspots)
      } else {
        let hotspots = await fetchHotspotsAsync({
          data: {
            category: sortBy,
            accessCategory,
            functionalType,
            timePeriod,
            aggClassification,
            startTime,
            endTime,
            sourceId,
            region,
            county,
            city,
            daysOfWeek,
            order,
            limit,
            differenceBetweenStartAndEndMonth: isCompareDates,
          },
        })
        hotspots = flipHotspotCoordinates(hotspots)
        setHotspotRoutes(hotspots)
      }

      setIsLoading(false)
    } catch (error) {
      console.error('Failed to fetch hotspots', error)
      setIsLoading(false)
    }
  }, [
    fetchHotspotsAsync,
    fetchImpactHotspots,
    order,
    limit,
    setHotspotRoutes,
    submittedRouteSpeedRequest,
    sortBy,
    isCompareDates,
    baseMonth,
    comparisonMonth,
    selectedImpactCategory,
  ])

  useEffect(() => {
    fetchHotspots()
  }, [
    submittedRouteSpeedRequest,
    order,
    limit,
    sortBy,
    isCompareDates,
    baseMonth,
    comparisonMonth,
    selectedImpactCategory,
    fetchHotspots,
  ])

  const handleLimitChange = (event) => {
    setLimit(event.target.value)
  }

  const handleOrderChange = (event) => {
    setOrder(event.target.value)
  }

  const handleSortByChange = (event) => {
    setSortBy(event.target.value)

    // If we switch away from the categories that use functionalType
    // if (![0, 1].includes(event.target.value)) {
    //   setSelectedFunctionalType(null)
    // }
  }

  // const handleFunctionalTypeChange = (event) => {
  //   const value = event.target.value === 'null' ? null : event.target.value
  //   setSelectedFunctionalType(value)
  // }

  const handleImpactCategoryChange = (event) => {
    setSelectedImpactCategory(event.target.value)
  }

  const handleMouseDown = useCallback(
    (event) => {
      isDragging.current = true
      initialMouseX.current = event.clientX
      initialWidth.current =
        containerRef.current?.getBoundingClientRect().width || width

      document.body.style.userSelect = 'none'
    },
    [width]
  )

  const handleMouseUp = useCallback(() => {
    isDragging.current = false
    document.body.style.userSelect = 'auto'
  }, [])

  const handleMouseMove = useCallback((event) => {
    if (isDragging.current) {
      const deltaX = event.clientX - initialMouseX.current
      const newWidth = initialWidth.current - deltaX
      setWidth(newWidth)
    }
  }, [])

  const throttledHandleMouseMove = useMemo(
    () => throttle(handleMouseMove, 100),
    [handleMouseMove]
  )

  useEffect(() => {
    window.addEventListener('mousemove', throttledHandleMouseMove)
    window.addEventListener('mouseup', handleMouseUp)

    return () => {
      window.removeEventListener('mousemove', throttledHandleMouseMove)
      window.removeEventListener('mouseup', handleMouseUp)
      throttledHandleMouseMove.cancel()
    }
  }, [throttledHandleMouseMove, handleMouseUp])

  let categoryFilterOptions = categoryFilters || []

  if (submittedRouteSpeedRequest?.sourceId?.includes(DataSource.ClearGuide)) {
    categoryFilterOptions = (categoryFilters || []).filter(
      (filter) =>
        filter.displayName !== 'Average 85th Percentile Speed' &&
        filter.displayName !== 'Percentage of Violations' &&
        filter.displayName !== 'Percentage of Extreme Violations' &&
        filter.displayName !== '85th Percentile Speed vs Speed Limit'
    )
  }

  return (
    <Box
      ref={containerRef}
      display="flex"
      flexDirection="column"
      sx={{
        maxHeight: SM_Height,
        width: `${width}px`,
        border: '1px solid',
        borderColor: 'divider',
        borderTop: 'none',
        position: 'relative',
        minWidth: 620,
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          left: -5,
          top: '50%',
          transform: 'translateY(-50%)',
          width: '20px',
          height: '100%',
          cursor: 'col-resize',
          backgroundColor: 'rgba(0, 0, 0, 0)',
          zIndex: 10,
        }}
        onMouseDown={handleMouseDown}
      />

      <Box
        sx={{
          backgroundColor: 'background.paper',
          position: 'sticky',
          top: 0,
          zIndex: 1,
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
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              mt: 1,
              mb: 3,
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 'bold' }}>
                Hotspot Analysis
              </Typography>
              <IconButton onClick={handleOpenPopover}>
                <HelpOutlineIcon fontSize="small" />
              </IconButton>
            </Box>
            <Popover
              id={id}
              open={open}
              anchorEl={anchorEl}
              onClose={handleClosePopover}
              anchorOrigin={{
                vertical: 'bottom',
                horizontal: 'left',
              }}
              transformOrigin={{
                vertical: 'top',
                horizontal: 'left',
              }}
            >
              <Box sx={{ p: 2, maxWidth: 400 }}>
                <Typography variant="subtitle2" fontWeight="bold" gutterBottom>
                  How Segments Are Ranked
                </Typography>
                <Typography variant="body2">
                  Segments are ranked based on the data source, date range, and
                  any other filters you've chosen in the top bar. For each
                  segment, we look at monthly values within the selected date
                  range and use the single highest monthly value in the chosen
                  category (e.g., average speed).
                  <br />
                  <br />
                  <strong>Effectiveness of Strategies:</strong> This hotspot
                  evaluates <em>all</em> Impacts without regard to date, and
                  ranks segments based on the selected category.
                  <br />
                  <br />
                  <strong>Compare Dates:</strong> Compares two months against
                  each other. The change from the base month to the comparison
                  month is displayed and used to rank the segments.
                </Typography>
              </Box>
            </Popover>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                position: 'relative',
                width: isCompareDates ? '364px' : 'auto',
                transition: 'width 0.3s ease',
              }}
            >
              {sortBy !== 100 && (
                <Button
                  onClick={() => setIsCompareDates((prev) => !prev)}
                  aria-label="compare dates"
                  size="small"
                  sx={{
                    transition: 'transform 0.3s ease',
                    transform: isCompareDates
                      ? 'translateX(-90px)'
                      : 'translateX(0)',
                    textTransform: 'none',
                    width: '140px',
                    '& .MuiButton-startIcon': {
                      marginX: '3px',
                    },
                  }}
                  startIcon={
                    isCompareDates ? <ChevronRightIcon /> : <ChevronLeftIcon />
                  }
                >
                  Compare Dates
                </Button>
              )}
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  position: 'absolute',
                  left: '48px',
                  transition: 'opacity 0.3s ease',
                  opacity: isCompareDates ? 1 : 0,
                  pointerEvents: isCompareDates ? 'auto' : 'none',
                }}
              >
                <DatePicker
                  label="Base Month"
                  views={['month', 'year']}
                  format="MMM yyyy"
                  value={baseMonth}
                  onChange={(val) => setBaseMonth(val)}
                  slotProps={{
                    textField: { size: 'small' },
                  }}
                  sx={{ width: 140 }}
                />
                <ArrowForwardIcon sx={{ mx: 1 }} fontSize="small" />
                <DatePicker
                  label="Comparison Month"
                  views={['month', 'year']}
                  format="MMM yyyy"
                  value={comparisonMonth}
                  onChange={(val) => setComparisonMonth(val)}
                  slotProps={{
                    textField: { size: 'small' },
                  }}
                  sx={{ width: 140 }}
                />
              </Box>
            </Box>
          </Box>
          <Divider />
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mt: 3,
            }}
          >
            {categoryFilterOptions && (
              <>
                <InputLabel
                  sx={{ typography: 'body2', color: 'black' }}
                  htmlFor={'rank-by-select-label'}
                >
                  Rank by
                </InputLabel>
                <FormControl size="small" sx={{ width: 230 }}>
                  <Select
                    labelId="rank-by-select-label"
                    id="rank-by-select"
                    inputProps={{ id: 'rank-by-select-label' }}
                    value={sortBy}
                    onChange={handleSortByChange}
                  >
                    {categoryFilterOptions.map((filter) => (
                      <MenuItem key={filter.number} value={filter.number}>
                        {filter.displayName}
                      </MenuItem>
                    ))}
                    <MenuItem value={100}>Effectiveness of Strategies</MenuItem>
                  </Select>
                </FormControl>
              </>
            )}

            {/* {[0, 1].includes(sortBy) && functionalTypes && (
              <>
                <InputLabel
                  sx={{
                    position: 'absolute',
                    width: '1px',
                    height: '1px',
                    padding: 0,
                    overflow: 'hidden',
                    clip: 'rect(0, 0, 0, 0)',
                    whiteSpace: 'nowrap',
                    border: 0,
                  }}
                  htmlFor="functional-type-select-label"
                >
                  Functional Type
                </InputLabel>

                <FormControl size="small">
                  <Select
                    labelId="functional-type-select-label"
                    id="functional-type-select"
                    inputProps={{ id: 'functional-type-select-label' }}
                    value={
                      selectedFunctionalType === null
                        ? 'null'
                        : selectedFunctionalType
                    }
                    onChange={handleFunctionalTypeChange}
                  >
                    <MenuItem value="null">All</MenuItem>
                    {functionalTypes.map((type) => (
                      <MenuItem key={type.id} value={type.name}>
                        {type.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </>
            )} */}

            {sortBy === 100 && (
              <>
                <InputLabel
                  sx={{
                    //hidden input label that is still accessible to screen readers
                    position: 'absolute',
                    width: '1px',
                    height: '1px',
                    padding: 0,
                    overflow: 'hidden',
                    clip: 'rect(0, 0, 0, 0)',
                    whiteSpace: 'nowrap',
                    border: 0,
                  }}
                  htmlFor="impact-category-select-label"
                >
                  Impact Category
                </InputLabel>
                <FormControl size="small">
                  <Select
                    labelId="impact-category-select-label"
                    id="impact-category-select"
                    inputProps={{ id: 'impact-category-select-label' }}
                    value={selectedImpactCategory}
                    onChange={handleImpactCategoryChange}
                  >
                    {Object.keys(ImpactSpeedAggregationCategoryFilter).map(
                      (key) => (
                        <MenuItem
                          key={key}
                          value={ImpactSpeedAggregationCategoryFilter[key]}
                        >
                          {addSpaces(ImpactSpeedAggregationCategoryFilter[key])}
                        </MenuItem>
                      )
                    )}
                  </Select>
                </FormControl>
              </>
            )}
            <>
              <InputLabel
                sx={{
                  position: 'absolute',
                  width: '1px',
                  height: '1px',
                  padding: 0,
                  overflow: 'hidden',
                  clip: 'rect(0, 0, 0, 0)',
                  whiteSpace: 'nowrap',
                  border: 0,
                }}
                htmlFor="order-select-label"
              >
                Order By
              </InputLabel>
              <FormControl size="small" sx={{ minWidth: 100 }}>
                <Select
                  labelId="order-select-label"
                  id="order-select"
                  value={order}
                  onChange={handleOrderChange}
                  inputProps={{ id: 'order-select-label' }}
                >
                  <MenuItem value="ASC">Ascending</MenuItem>
                  <MenuItem value="DESC">Descending</MenuItem>
                </Select>
              </FormControl>
            </>

            <InputLabel
              sx={{ typography: 'body2', color: 'black' }}
              htmlFor="limit-select-label"
            >
              Limit:
            </InputLabel>
            <FormControl size="small" sx={{ minWidth: 80 }}>
              <Select
                labelId="limit-select-label"
                id="limit-select"
                inputProps={{ id: 'limit-select-label' }}
                value={limit}
                onChange={handleLimitChange}
              >
                <MenuItem value={10}>10</MenuItem>
                <MenuItem value={25}>25</MenuItem>
                <MenuItem value={100}>100</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </Box>
      </Box>

      {/* The checkbox for showBeforeAfter */}
      {sortBy === 100 && (
        <Box sx={{ m: 0, ml: 2, height: '40px' }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={showBeforeAfter}
                onChange={() => setShowBeforeAfter((prev) => !prev)}
                size="small"
              />
            }
            label="Show before/after"
          />
        </Box>
      )}

      <Box sx={{ flex: 1, overflowY: 'auto' }}>
        {sortBy === 100 ? (
          <ImpactHotspotTable
            hotspots={hotspotRoutes}
            handleRouteSelection={handleRouteSelection}
            setHoveredHotspot={setHoveredHotspot}
            showBeforeAfter={showBeforeAfter}
            isLoading={isLoading}
          />
        ) : (
          <HotspotTable
            hotspots={hotspotRoutes}
            handleRouteDoubleClick={handleRouteSelection}
            setHoveredHotspot={setHoveredHotspot}
            compareDates={isCompareDates}
            isLoading={isLoading}
            selectedHotspotType={
              categoryFilters?.find((filter) => filter.number === sortBy)
                ?.displayName
            }
          />
        )}
      </Box>
    </Box>
  )
}

export default HotspotSidebar
