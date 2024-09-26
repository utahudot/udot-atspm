import { SpeedVariabilityDto } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import { ExtendedEChartsOption } from '@/features/charts/types'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
} from '@mui/material'

const SpeedVariabilityChartContainer = ({
  chartData,
}: {
  chartData: ExtendedEChartsOption
}) => {
  const response: SpeedVariabilityDto = chartData.displayProp?.data
  const variabilityData = response.data
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
        <ApacheEChart
          id="speed-variability-chart"
          option={chartData}
          style={{ width: '1100px', height: '500px' }}
          hideInteractionMessage
        />
      </Box>
      <Box
        width="70%"
        sx={{
          mt: 4,
          ml: 2,
        }}
      >
        <Table size="small" sx={{ border: '1px solid #000' }}>
          <TableHead>
            <TableRow>
              <TableCell sx={{ border: '1px solid #000' }}>Date</TableCell>
              <TableCell sx={{ border: '1px solid #000' }}>
                Max Speed (MPH)
              </TableCell>
              <TableCell sx={{ border: '1px solid #000' }}>
                Min Speed (MPH)
              </TableCell>
              <TableCell sx={{ border: '1px solid #000' }}>
                Daily Variability (MPH)
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {variabilityData.map((v, index) => (
              <TableRow key={index}>
                <TableCell sx={{ border: '1px solid #000' }} key={index}>
                  {v.date}
                </TableCell>
                <TableCell sx={{ border: '1px solid #000' }} key={index}>
                  {v.maxSpeed}
                </TableCell>
                <TableCell sx={{ border: '1px solid #000' }} key={index}>
                  {v.minSpeed}
                </TableCell>
                <TableCell sx={{ border: '1px solid #000' }} key={index}>
                  {v.speedVariability}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Box>
    </Box>
  )
}

export default SpeedVariabilityChartContainer
