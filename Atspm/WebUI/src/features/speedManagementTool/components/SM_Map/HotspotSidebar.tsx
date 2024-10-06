import { usePostApiV1MonthlyAggregationHotspots } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  FormControl,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { Fragment, useEffect, useState } from 'react'

const HotspotSidebar = () => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()
  const { mutateAsync: fetchHotspotsAsync } =
    usePostApiV1MonthlyAggregationHotspots()

  const [hotspots, setHotspots] = useState([])
  const [order, setOrder] = useState('DESC')
  const [limit, setLimit] = useState(25)
  const [sortBy, setSortBy] = useState('averageSpeed')
  const [visibleGroups, setVisibleGroups] = useState({
    speeds: true,
    violations: false,
    percentages: false,
    flow: false,
    variability: false,
  })

  const fetchHotspots = async () => {
    const hotspots = await fetchHotspotsAsync({
      data: {
        category: 0,
        timePeriod: 0,
        aggClassification: 0,
        startTime: submittedRouteSpeedRequest.startDate,
        endTime: submittedRouteSpeedRequest.endDate,
        sourceId: submittedRouteSpeedRequest.sourceId,
        order,
        limit,
      },
    })
    setHotspots(hotspots)
  }

  useEffect(() => {
    fetchHotspots()
  }, [submittedRouteSpeedRequest, order, limit])

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

  return (
    <Box
      sx={{
        width: '600px',
        backgroundColor: 'background.paper',
        border: '1px solid',
        borderColor: 'divider',
        borderTop: 'none',
        maxHeight: SM_Height,
        overflowY: 'auto',
      }}
    >
      <Box
        sx={{
          p: 2,
          borderBottom: '1px solid',
          marginLeft: '2px',
          borderColor: 'divider',
          backgroundColor: 'background.default',
          position: 'sticky',
        }}
      >
        <Typography variant="subtitle2">Hotspots</Typography>
        {/* Sort By, Order, and Limit in One Line */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
          <Typography variant="body2">Sort by</Typography>
          <FormControl size="small" sx={{ minWidth: 160 }}>
            {/* <InputLabel id="sort-by-select-label">Sort By</InputLabel> */}
            <Select
              labelId="sort-by-select-label"
              id="sort-by-select"
              value={sortBy}
              //   label="Sort By"

              onChange={handleSortByChange}
            >
              <MenuItem value="averageSpeed">Average Speed</MenuItem>
              <MenuItem value="violations">Violations</MenuItem>
              <MenuItem value="flow">Flow</MenuItem>
              <MenuItem value="variability">Variability</MenuItem>
              {/* Add more sorting options as required */}
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
            {/* <InputLabel id="limit-select-label">Limit</InputLabel> */}
            <Select
              labelId="limit-select-label"
              id="limit-select"
              value={limit}
              //   label="Limit"
              onChange={handleLimitChange}
            >
              <MenuItem value={10}>10</MenuItem>
              <MenuItem value={25}>25</MenuItem>
              <MenuItem value={100}>100</MenuItem>
            </Select>
          </FormControl>
        </Box>
        {/* Group Visibility Toggles
        <FormGroup
          row
          sx={{ mt: 2, display: 'flex', flexWrap: 'wrap', gap: 2 }}
        >
          <FormControlLabel
            labelPlacement="top"
            control={
              <Checkbox
                size="small"
                checked={visibleGroups.speeds}
                onChange={handleGroupVisibilityChange}
                name="speeds"
              />
            }
            label="Speeds (Min, Max, Avg)"
          />
          <FormControlLabel
            labelPlacement="top"
            control={
              <Checkbox
                size="small"
                checked={visibleGroups.violations}
                onChange={handleGroupVisibilityChange}
                name="violations"
              />
            }
            label="Violations (Regular, Extreme)"
          />
          <FormControlLabel
            labelPlacement="top"
            control={
              <Checkbox
                size="small"
                checked={visibleGroups.percentages}
                onChange={handleGroupVisibilityChange}
                name="percentages"
              />
            }
            label="Percentages (Violations, Extreme)"
          />
          <FormControlLabel
            labelPlacement="top"
            control={
              <Checkbox
                size="small"
                checked={visibleGroups.flow}
                onChange={handleGroupVisibilityChange}
                name="flow"
              />
            }
            label="Flow"
          />
          <FormControlLabel
            labelPlacement="top"
            control={
              <Checkbox
                size="small"
                checked={visibleGroups.variability}
                onChange={handleGroupVisibilityChange}
                name="variability"
              />
            }
            label="Variability"
          />
        </FormGroup>*/}
      </Box>

      <Table>
        <TableHead>
          <TableRow>
            <TableCell>
              <Typography fontSize={'.75rem'} fontWeight={'bold'}>
                Hotspot
              </Typography>
            </TableCell>
            {visibleGroups.speeds && (
              <>
                <TableCell>
                  <Typography fontSize={'.75rem'} fontWeight={'bold'}>
                    Min Speed
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography fontSize={'.75rem'} fontWeight={'bold'}>
                    Max Speed
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography fontSize={'.75rem'} fontWeight={'bold'}>
                    Avg Speed
                  </Typography>
                </TableCell>
              </>
            )}
            {visibleGroups.violations && (
              <>
                <TableCell>Violations</TableCell>
                <TableCell>Extreme Violations</TableCell>
              </>
            )}
            {visibleGroups.flow && <TableCell>Flow</TableCell>}
            {visibleGroups.variability && <TableCell>Variability</TableCell>}
            {visibleGroups.percentages && (
              <>
                <TableCell>% Violations</TableCell>
                <TableCell>% Extreme Violations</TableCell>
              </>
            )}
          </TableRow>
        </TableHead>
        <TableBody>
          {hotspots.map((hotspot) => (
            <Fragment key={hotspot.id}>
              <TableRow>
                <TableCell>I215-Counterclockwise - 5 and more stuff</TableCell>
                {visibleGroups.speeds && (
                  <>
                    <TableCell>{hotspot.minSpeed}</TableCell>
                    <TableCell>{hotspot.maxSpeed}</TableCell>
                    <TableCell>{Math.round(hotspot.averageSpeed)}</TableCell>
                  </>
                )}
                {visibleGroups.violations && (
                  <>
                    <TableCell>{hotspot.violations}</TableCell>
                    <TableCell>{hotspot.extremeViolations}</TableCell>
                  </>
                )}
                {visibleGroups.flow && <TableCell>{hotspot.flow}</TableCell>}
                {visibleGroups.variability && (
                  <TableCell>{hotspot.variability}</TableCell>
                )}
                {visibleGroups.percentages && (
                  <>
                    <TableCell>
                      {Math.round(hotspot.percentViolations)}
                    </TableCell>
                    <TableCell>
                      {Math.round(hotspot.percentExtremeViolations)}
                    </TableCell>
                  </>
                )}
              </TableRow>
            </Fragment>
          ))}
        </TableBody>
      </Table>
    </Box>
  )
}

export default HotspotSidebar
