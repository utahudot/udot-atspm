import { SpeedVariabilityDto } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import { ExtendedEChartsOption } from '@/features/charts/types'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'

const SpeedVariabilityChartContainer = ({
  chartData,
}: {
  chartData: ExtendedEChartsOption
}) => {
  const chartOptions = chartData.charts[0]
  const response: SpeedVariabilityDto = chartOptions.displayProp?.data
  const variabilityData = response.data

  const formattedDate = (date: string | undefined) => {
    if (!date) return ''

    const dateObj = new Date(date)
    return dateObj.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
  }
  return (
    <>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignContent: 'center',
          pt: 2,
        }}
      >
        <ApacheEChart
          id="speed-variability-chart"
          option={chartOptions}
          style={{ width: '1100px', height: '500px' }}
          hideInteractionMessage
        />
      </Box>
      <TableContainer
        sx={{
          mt: 4,
          maxWidth: '1100px',
          mx: 'auto',
          overflowX: 'inherit',
        }}
      >
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Date
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Max Speed (mph)
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Min Speed (mph)
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Daily Variability (mph)
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {variabilityData.map((v, index) => (
              <TableRow key={index}>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {formattedDate(v.date)}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {v.maxSpeed !== null ? Math.round(v.maxSpeed as number) : '-'}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {v.minSpeed !== null ? Math.round(v.minSpeed as number) : '-'}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {v.speedVariability !== null
                    ? Math.round(v.speedVariability as number)
                    : '-'}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  )
}

export default SpeedVariabilityChartContainer
