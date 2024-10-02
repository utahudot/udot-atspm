import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
} from '@mui/material'

const EffectivenessOfStrategiesChartsContainer = ({ chartData }: { chartData: any }) => {
  let tableData = chartData.tableData
  console.log(chartData)
  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
        <ApacheEChart
          id="speed-over-time-chart"
          option={chartData}
          style={{ width: '1100px', height: '500px' }}
          hideInteractionMessage
        />
      </Box>


    </>
  )
}

export default EffectivenessOfStrategiesChartsContainer
