import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { format } from 'date-fns'
import { useState } from 'react'
import transformEffectivenessOfStrategiesData from './effectivenessOfStrategies.transformer'

const EffectivenessOfStrategiesChartsContainer = ({
  chartData: chartOptions,
}: {
  chartData: ChartData
}) => {
  const chartData = chartOptions.charts[0]
  const [customSpeedLimit, setCustomSpeedLimit] = useState<number | null>(null)
  const [appliedSpeedLimit, setAppliedSpeedLimit] = useState<number | null>(
    null
  )
  const [transformedChartData, setTransformedChartData] =
    useState<ChartData>(chartData)

  const handleSpeedLimitChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = event.target.value ? parseInt(event.target.value) : null
    setCustomSpeedLimit(value)
  }

  const handleApplySpeedLimit = () => {
    const newChartData = transformEffectivenessOfStrategiesData(
      chartData.response,
      customSpeedLimit
    )
    setTransformedChartData(newChartData.charts[0])
  }

  const handleResetSpeedLimit = () => {
    setCustomSpeedLimit(null)
    setAppliedSpeedLimit(null)
    setTransformedChartData(chartData)
  }

  const Overalls = chartData.response

  const beforeMetrics = Overalls[0].before
  const afterMetrics = Overalls[0].after
  const differenceMetrics = {
    averageSpeed: afterMetrics.averageSpeed - beforeMetrics.averageSpeed,
    averageEightyFifthSpeed:
      afterMetrics.averageEightyFifthSpeed -
      beforeMetrics.averageEightyFifthSpeed,
    percentViolations:
      afterMetrics.percentViolations - beforeMetrics.percentViolations,
    percentExtremeViolations:
      afterMetrics.percentExtremeViolations -
      beforeMetrics.percentExtremeViolations,
    variability: afterMetrics.variability - beforeMetrics.variability,
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return format(date, 'MM/dd/yyyy')
  }

  return (
    <>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          flexDirection: 'row',
          gap: 1,
          mr: 2,
        }}
      >
        <TextField
          label="Custom Speed Limit"
          type="number"
          value={customSpeedLimit !== null ? customSpeedLimit : ''}
          onChange={handleSpeedLimitChange}
          InputProps={{ inputProps: { min: 0 } }} // Optional: Prevent negative numbers
          variant="outlined"
          sx={{ width: 200 }}
          size="small"
        />
        <Button
          size="small"
          variant="contained"
          onClick={handleApplySpeedLimit}
          disabled={
            customSpeedLimit === null || customSpeedLimit === appliedSpeedLimit
          }
        >
          Apply
        </Button>
        <Button size="small" variant="outlined" onClick={handleResetSpeedLimit}>
          Reset
        </Button>
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
        <ApacheEChart
          id="speed-over-time-chart"
          option={transformedChartData}
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
        <Table>
          <TableBody>
            <TableRow>
              <TableCell
                colSpan={6}
                sx={{ padding: '20px', border: 'none', textAlign: 'center' }}
              >
                <Typography sx={{ fontWeight: 'bold' }}>Overalls</Typography>
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell />
              <TableCell>Avg Speed (mph)</TableCell>
              <TableCell>85th %ile Speed (mph)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Severe Violations</TableCell>
              <TableCell>Speed Variability (mph)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell>Before</TableCell>
              <TableCell>
                {typeof beforeMetrics.averageSpeed === 'number'
                  ? beforeMetrics.averageSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof beforeMetrics.averageEightyFifthSpeed === 'number'
                  ? beforeMetrics.averageEightyFifthSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof beforeMetrics.percentViolations === 'number'
                  ? `${beforeMetrics.percentViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof beforeMetrics.percentExtremeViolations === 'number'
                  ? `${beforeMetrics.percentExtremeViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof beforeMetrics.variability === 'number'
                  ? beforeMetrics.variability.toFixed(2)
                  : 'N/A'}
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>After</TableCell>
              <TableCell>
                {typeof afterMetrics.averageSpeed === 'number'
                  ? afterMetrics.averageSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof afterMetrics.averageEightyFifthSpeed === 'number'
                  ? afterMetrics.averageEightyFifthSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof afterMetrics.percentViolations === 'number'
                  ? `${afterMetrics.percentViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof afterMetrics.percentExtremeViolations === 'number'
                  ? `${afterMetrics.percentExtremeViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof afterMetrics.variability === 'number'
                  ? afterMetrics.variability.toFixed(2)
                  : 'N/A'}
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Difference</TableCell>
              <TableCell>
                {typeof differenceMetrics.averageSpeed === 'number'
                  ? differenceMetrics.averageSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof differenceMetrics.averageEightyFifthSpeed === 'number'
                  ? differenceMetrics.averageEightyFifthSpeed.toFixed(2)
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof differenceMetrics.percentViolations === 'number'
                  ? `${differenceMetrics.percentViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof differenceMetrics.percentExtremeViolations === 'number'
                  ? `${differenceMetrics.percentExtremeViolations.toFixed(2)}%`
                  : 'N/A'}
              </TableCell>
              <TableCell>
                {typeof differenceMetrics.variability === 'number'
                  ? differenceMetrics.variability.toFixed(2)
                  : 'N/A'}
              </TableCell>
              {/* <TableCell colSpan={6} sx={{ padding: '10px', border: 'none' }} /> */}
            </TableRow>
          </TableBody>
          <TableRow>
            <TableCell
              colSpan={6}
              sx={{ padding: '20px', border: 'none', textAlign: 'center' }}
            >
              <Typography sx={{ fontWeight: 'bold' }}>
                Before Treatment Application
              </Typography>
            </TableCell>
          </TableRow>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Avg Speed (mph)</TableCell>
              <TableCell>85th %ile Speed (mph)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Severe Violations</TableCell>
              <TableCell>Speed Variability (mph)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Overalls[0].weeklyEffectiveness
              .filter(
                (week) =>
                  new Date(week.endDate) >= new Date(beforeMetrics.startDate) &&
                  new Date(week.endDate) <= new Date(beforeMetrics.endDate)
              )
              .map((week) => (
                <TableRow key={week.endDate}>
                  <TableCell>
                    {`${formatDate(week.startDate)} - ${formatDate(
                      week.endDate
                    )}`}
                  </TableCell>
                  <TableCell>
                    {typeof week.averageSpeed === 'number'
                      ? week.averageSpeed.toFixed(2)
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.averageEightyFifthSpeed === 'number'
                      ? week.averageEightyFifthSpeed.toFixed(4)
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.percentViolations === 'number'
                      ? `${week.percentViolations.toFixed(2)}%`
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.percentExtremeViolations === 'number'
                      ? `${week.percentExtremeViolations.toFixed(2)}%`
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.variability === 'number'
                      ? week.variability.toFixed(2)
                      : 'N/A'}
                  </TableCell>
                </TableRow>
              ))}
            <TableCell
              colSpan={6}
              sx={{ padding: '10px', border: 'none' }}
            ></TableCell>
          </TableBody>
          <TableRow>
            <TableCell
              colSpan={6}
              sx={{ padding: '10px', border: 'none', textAlign: 'center' }}
            >
              <Typography sx={{ fontWeight: 'bold' }}>
                After Treatment Application
              </Typography>
            </TableCell>
          </TableRow>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell>Avg Speed (mph)</TableCell>
              <TableCell>85th %ile Speed (mph)</TableCell>
              <TableCell>% Violations</TableCell>
              <TableCell>% Extreme Violations</TableCell>
              <TableCell>Speed Variability (mph)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {Overalls[0].weeklyEffectiveness
              .filter(
                (week) =>
                  new Date(week.endDate) >= new Date(afterMetrics.startDate) &&
                  new Date(week.endDate) <= new Date(afterMetrics.endDate)
              )
              .map((week) => (
                <TableRow key={week.endDate}>
                  <TableCell>
                    {`${formatDate(week.startDate)} - ${formatDate(
                      week.endDate
                    )}`}
                  </TableCell>
                  <TableCell>
                    {typeof week.averageSpeed === 'number'
                      ? week.averageSpeed.toFixed(2)
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.averageEightyFifthSpeed === 'number'
                      ? week.averageEightyFifthSpeed.toFixed(4)
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.percentViolations === 'number'
                      ? `${week.percentViolations.toFixed(2)}%`
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.percentExtremeViolations === 'number'
                      ? `${week.percentExtremeViolations.toFixed(2)}%`
                      : 'N/A'}
                  </TableCell>
                  <TableCell>
                    {typeof week.variability === 'number'
                      ? week.variability.toFixed(2)
                      : 'N/A'}
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>
    </>
  )
}

export default EffectivenessOfStrategiesChartsContainer
