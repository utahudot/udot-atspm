import type { TimeOfDayResult } from '@/api/reports'
import {
  Alert,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { formatNumber } from '../../transformers'
import {
  compactReportTableContainerSx,
  compactReportTableHeadSx,
  compactReportTableRowSx,
  numericReportTableCellSx,
} from './timeOfDayReportTableStyles'

const formatBoolean = (value?: boolean | null) => {
  if (value === undefined || value === null) return '-'

  return value ? 'Yes' : 'No'
}

const formatPercent = (value?: number | null) => {
  if (value === undefined || value === null) return '-'

  return `${formatNumber(value, 1)}%`
}

const formatRatio = (value?: number | null) => {
  if (value === undefined || value === null || Number.isNaN(value)) return '-'

  return (value / 100).toFixed(2)
}

const formatLocation = (
  identifier?: string | null,
  description?: string | null
) => {
  if (description) {
    return !identifier || description.includes(identifier)
      ? description
      : `${identifier} - ${description}`
  }

  return identifier ?? '-'
}

export default function TimeOfDayLocationData({
  result,
}: {
  result: TimeOfDayResult
}) {
  const locations = result.locations ?? []

  return (
    <Paper sx={{ p: { xs: 2, md: 3 } }}>
      <Stack spacing={0.5} sx={{ mb: 2 }}>
        <Typography variant="h5" component="h2">
          Location Supporting Data
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Coverage, peak demand, occupancy, and review details for{' '}
          {locations.length} selected{' '}
          {locations.length === 1 ? 'location' : 'locations'}.
        </Typography>
      </Stack>
      {locations.length === 0 ? (
        <Alert severity="warning">No location supporting data available.</Alert>
      ) : (
        <TableContainer
          component={Paper}
          variant="outlined"
          sx={compactReportTableContainerSx}
        >
          <Table
            size="small"
            aria-label="time of day location supporting data"
            sx={{ minWidth: 1500 }}
          >
            <TableHead
              sx={{
                ...compactReportTableHeadSx,
                '& .MuiTableRow-root:first-of-type .MuiTableCell-head:not([rowspan])':
                  {
                    bgcolor: 'grey.50',
                    color: 'text.secondary',
                    fontSize: '0.7rem',
                    letterSpacing: '0.04em',
                    textTransform: 'uppercase',
                  },
              }}
            >
              <TableRow>
                <TableCell rowSpan={2} scope="col">
                  Location
                </TableCell>
                <TableCell align="center" colSpan={2} scope="colgroup">
                  Coverage
                </TableCell>
                <TableCell align="center" colSpan={3} scope="colgroup">
                  Peak Demand
                </TableCell>
                <TableCell align="center" colSpan={3} scope="colgroup">
                  Occupancy
                </TableCell>
                <TableCell align="center" colSpan={5} scope="colgroup">
                  Review
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell align="right" scope="col">
                  Days
                </TableCell>
                <TableCell scope="col">Fallback</TableCell>
                <TableCell align="right" scope="col">
                  Raw
                </TableCell>
                <TableCell align="right" scope="col">
                  Smoothed
                </TableCell>
                <TableCell align="right" scope="col">
                  Hourly Rate
                </TableCell>
                <TableCell align="right" scope="col">
                  V/C Ratio
                </TableCell>
                <TableCell align="right" scope="col">
                  AM Peak
                </TableCell>
                <TableCell align="right" scope="col">
                  PM Peak
                </TableCell>
                <TableCell scope="col">AM Direction</TableCell>
                <TableCell scope="col">PM Direction</TableCell>
                <TableCell scope="col">Cross Traffic</TableCell>
                <TableCell scope="col">Data Quality</TableCell>
                <TableCell scope="col">Notes</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {locations.map((location) => (
                <TableRow
                  key={location.locationIdentifier}
                  hover
                  sx={compactReportTableRowSx}
                >
                  <TableCell sx={{ minWidth: 260 }}>
                    <Typography
                      variant="body2"
                      noWrap
                      title={formatLocation(
                        location.locationIdentifier,
                        location.locationDescription
                      )}
                    >
                      {formatLocation(
                        location.locationIdentifier,
                        location.locationDescription
                      )}
                    </Typography>
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatNumber(location.daysWithData)}
                  </TableCell>
                  <TableCell>
                    {formatBoolean(location.coverageFallbackUsed)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatNumber(location.summary?.peakRawVolume)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatNumber(location.summary?.peakSmoothedVolume)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatNumber(location.summary?.peakHourlyRate)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatRatio(location.summary?.peakOccupancyPercent)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatPercent(location.summary?.amPeakOccupancyPercent)}
                  </TableCell>
                  <TableCell align="right" sx={numericReportTableCellSx}>
                    {formatPercent(location.summary?.pmPeakOccupancyPercent)}
                  </TableCell>
                  <TableCell sx={{ minWidth: 160 }}>
                    {location.summary?.amDirectionExceptionMessage ?? '-'}
                  </TableCell>
                  <TableCell sx={{ minWidth: 160 }}>
                    {location.summary?.pmDirectionExceptionMessage ?? '-'}
                  </TableCell>
                  <TableCell sx={{ minWidth: 160 }}>
                    {location.summary?.crossTrafficReview ?? '-'}
                  </TableCell>
                  <TableCell>{location.dataQualityFlag ?? '-'}</TableCell>
                  <TableCell sx={{ minWidth: 180 }}>
                    {location.summary?.notes?.trim() || '-'}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Paper>
  )
}
