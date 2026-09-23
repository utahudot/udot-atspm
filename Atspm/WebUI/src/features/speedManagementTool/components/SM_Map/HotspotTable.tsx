import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import InfoOutlinedIcon from '@mui/icons-material/HelpOutline'
import {
  Box,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material'
import { format } from 'date-fns'
import { Fragment } from 'react'

type HotspotTableProps = {
  hotspots: Array<any>
  handleRouteDoubleClick: (routeId: string) => void
  setHoveredHotspot: (routeId: string | null) => void
  compareDates: boolean
  selectedHotspotType: string
  isLoading: boolean
}

const columnMapping = {
  'Average Speed': [
    'averageSpeed',
    'speedLimit',
    'avgSpeedVsSpeedLimit',
    'averageEightyFifthSpeed',
    'maxSpeed',
    'flow',
    'percentObserved',
  ],
  'Average 85th Percentile Speed': [
    'averageEightyFifthSpeed',
    'speedLimit',
    'eightyFifthSpeedVsSpeedLimit',
    'averageSpeed',
    'maxSpeed',
    'flow',
    'percentObserved',
  ],
  'Maximum Speed': [
    'maxSpeed',
    'variability',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'percentViolations',
    'percentExtremeViolations',
    'speedLimit',
    'flow',
    'percentObserved',
  ],
  'Minimum Speed': [
    'minSpeed',
    'variability',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'percentViolations',
    'percentExtremeViolations',
    'speedLimit',
    'flow',
    'percentObserved',
  ],
  Variability: [
    'variability',
    'maxSpeed',
    'minSpeed',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'speedLimit',
    'flow',
    'percentObserved',
  ],
  'Percentage of Violations': [
    'percentViolations',
    'percentExtremeViolations',
    'violations',
    'extremeViolations',
    'maxSpeed',
    'speedLimit',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'flow',
    'percentObserved',
  ],
  'Percentage of Extreme Violations': [
    'percentExtremeViolations',
    'percentViolations',
    'extremeViolations',
    'violations',
    'maxSpeed',
    'speedLimit',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'flow',
    'percentObserved',
  ],
  'Average Speed vs Speed Limit': [
    'avgSpeedVsSpeedLimit',
    'averageSpeed',
    'speedLimit',
    'averageEightyFifthSpeed',
    'maxSpeed',
    'flow',
    'percentObserved',
  ],
  '85th Percentile Speed vs Speed Limit': [
    'eightyFifthSpeedVsSpeedLimit',
    'averageEightyFifthSpeed',
    'speedLimit',
    'averageSpeed',
    'maxSpeed',
    'flow',
    'percentObserved',
  ],
  'Percentage Observed': [
    'percentObserved',
    'averageEightyFifthSpeed',
    'averageSpeed',
    'speedLimit',
    'minSpeed',
    'maxSpeed',
    'variability',
    'flow',
  ],
}

const columnNames = {
  averageSpeed: 'Avg Speed',
  speedLimit: 'Speed Limit',
  avgSpeedVsSpeedLimit: 'Avg vs Limit',
  averageEightyFifthSpeed: '85th %ile Speed',
  eightyFifthSpeedVsSpeedLimit: '85th %ile vs Limit',
  maxSpeed: 'Max Speed',
  minSpeed: 'Min Speed',
  variability: 'Variability',
  percentViolations: '% Violations',
  percentExtremeViolations: '% Ext. Violations',
  violations: 'Violations',
  extremeViolations: 'Ext. Violations',
  flow: 'Flow',
  percentObserved: '% Observed',
}

const formatValue = (value: any) => {
  if (typeof value === 'number') {
    const roundedValue = Math.round(value)
    return roundedValue.toLocaleString()
  }
  return value || 'N/A'
}

interface StyledTableCellProps {
  value: number
  compareDates: boolean
  isLoading: boolean
}

const StyledTableCell = ({
  value,
  compareDates,
  isLoading,
}: StyledTableCellProps) => {
  if (isLoading) {
    return (
      <TableCell align="right">
        <Skeleton variant="text" width={50} />
      </TableCell>
    )
  }
  const formattedValue = formatValue(value)
  const roundedValue = Math.round(value)
  const isZero = roundedValue === 0
  const displayValue = isZero
    ? '0'
    : compareDates && roundedValue > 0
      ? `+${formattedValue}`
      : formattedValue
  let backgroundColor = 'inherit'
  if (roundedValue > 0) {
    backgroundColor = 'rgba(173, 216, 230, 0.3)'
  } else if (roundedValue < 0) {
    backgroundColor = 'rgba(255, 143, 10, 0.3)'
  }
  return (
    <TableCell
      align="right"
      sx={{
        backgroundColor: compareDates ? backgroundColor : 'inherit',
        borderRight: '1px solid #d0d0d0',
        padding: '1px 10px',
      }}
    >
      {displayValue}
    </TableCell>
  )
}

const HotspotTable = ({
  hotspots,
  handleRouteDoubleClick,
  setHoveredHotspot,
  compareDates,
  selectedHotspotType,
  isLoading,
}: HotspotTableProps) => {
  const { zoomToHotspot, mapRef } = useSpeedManagementStore()

  const columns =
    columnMapping[selectedHotspotType as keyof typeof columnMapping] || []

  const violationHeaders = ['violations', 'percentViolations']

  const extremeViolationsHeaders = [
    'extremeViolations',
    'percentExtremeViolations',
  ]

  return (
    <Table stickyHeader sx={{ backgroundColor: 'white' }}>
      <TableHead>
        <TableRow>
          <TableCell>
            <Typography fontSize=".70rem" fontWeight="bold">
              Rank
            </Typography>
          </TableCell>
          <TableCell sx={{ minWidth: 220 }}>
            <Typography fontSize=".70rem" fontWeight="bold" noWrap>
              Hotspot
            </Typography>
          </TableCell>
          {!compareDates && (
            <TableCell>
              <Typography fontSize=".70rem" fontWeight="bold">
                Month
              </Typography>
            </TableCell>
          )}
          {columns.map((column) => {
            const label = columnNames[column]
            const needsViolationsTooltip = violationHeaders.includes(column)
            const needsExtremeViolationsTooltip =
              extremeViolationsHeaders.includes(column)
            return (
              <TableCell key={column}>
                {needsViolationsTooltip || needsExtremeViolationsTooltip ? (
                  <Tooltip
                    title={
                      needsViolationsTooltip
                        ? 'Violation: 2+ mph over speed limit'
                        : 'Extreme Violation: 10+ mph over speed limit on freeways or 7+ mph over speed limit on arterials'
                    }
                    arrow
                  >
                    <Box
                      component="span"
                      sx={{ display: 'inline-flex', alignItems: 'center' }}
                    >
                      <Typography
                        fontSize=".7rem"
                        fontWeight="bold"
                        whiteSpace="nowrap"
                      >
                        {label}
                      </Typography>
                      <InfoOutlinedIcon fontSize="inherit" sx={{ ml: 0.25 }} />
                    </Box>
                  </Tooltip>
                ) : (
                  <Typography
                    fontSize=".7rem"
                    fontWeight="bold"
                    whiteSpace="nowrap"
                  >
                    {label}
                  </Typography>
                )}
              </TableCell>
            )
          })}
        </TableRow>
      </TableHead>

      <TableBody>
        {isLoading
          ? Array.from({ length: 25 }).map((_, index) => (
              <TableRow key={index}>
                <TableCell>
                  <Skeleton variant="text" width={20} />
                </TableCell>
                <TableCell>
                  <Skeleton variant="text" width={130} />
                </TableCell>
                {columns.map((column, idx) => (
                  <StyledTableCell
                    key={idx}
                    value={0}
                    isLoading={true}
                    compareDates={false}
                  />
                ))}
              </TableRow>
            ))
          : hotspots.map((hotspot, index) => {
              const dateValue = hotspot.properties.binStartTime
              let formattedMonth = 'N/A'
              if (!isNaN(Date.parse(dateValue))) {
                formattedMonth = format(dateValue, 'MMM')
              }
              return (
                <Fragment key={index}>
                  <TableRow
                    hover
                    onClick={() =>
                      zoomToHotspot(
                        hotspot.geometry.coordinates,
                        mapRef?.getZoom()
                      )
                    }
                    onDoubleClick={() =>
                      handleRouteDoubleClick(hotspot.properties.route_id)
                    }
                    onMouseEnter={() =>
                      setHoveredHotspot(hotspot.properties.route_id)
                    }
                    onMouseLeave={() => setHoveredHotspot(null)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                      {index + 1}
                    </TableCell>
                    <TableCell
                      sx={{
                        minWidth: 220,
                        maxWidth: 220,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                        borderRight: '1px solid #d0d0d0',
                      }}
                    >
                      <Tooltip
                        title={hotspot.properties.name || 'N/A'}
                        placement="top"
                        arrow
                      >
                        <span>{hotspot.properties.name || 'N/A'}</span>
                      </Tooltip>
                    </TableCell>
                    {!compareDates && (
                      <TableCell sx={{ borderRight: '1px solid #d0d0d0' }}>
                        {formattedMonth}
                      </TableCell>
                    )}
                    {columns.map((column) => (
                      <StyledTableCell
                        key={column}
                        value={
                          hotspot.properties[column] !== undefined
                            ? hotspot.properties[column]
                            : 'N/A'
                        }
                        compareDates={
                          column === 'speedLimit' ? false : compareDates
                        }
                        isLoading={false}
                      />
                    ))}
                  </TableRow>
                </Fragment>
              )
            })}
      </TableBody>
    </Table>
  )
}

export default HotspotTable
