import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
} from '@mui/material'

const SpeedComplianceChartsContainer = ({ chartData }: { chartData: any }) => {
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

        <TableContainer
          sx={{
            mt: 4,
            maxWidth: '1100px',
            mx: 'auto',
            overflowX: 'inherit',
          }}
        >
          <Table size="small" stickyHeader>
            <TableBody>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                  }}
                >
                  Milepoint Extents
                </TableCell>

                {tableData.milePoints.map((row, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey' }}
                    align="right"
                    key={i}
                  >
                    {row
                      ? `MP ${row[0].toFixed(1)} - ${row[1].toFixed(1)}`
                      : 'No Data'}
                  </TableCell>
                ))}
              </TableRow>

              {/* Length Row */}
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                  }}
                >
                  Length&nbsp;(mi)
                </TableCell>

                {tableData.milePoints.map((row, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey' }}
                    align="right"
                    key={i}
                  >
                    {row ? (row[1] - row[0]).toFixed(1) : 'No Data'}
                  </TableCell>
                ))}
              </TableRow>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                  }}
                >
                  Speed Limit&nbsp;(MPH)
                </TableCell>

                {tableData.speedLimit.map((speed, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey' }}
                    align="right"
                    key={i}
                  >
                    {speed}
                  </TableCell>
                ))}
              </TableRow>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                    borderBottom: 0,
                  }}
                >
                  Avg Speed&nbsp;(MPH)
                </TableCell>

                {tableData.avgSpeed.map((speed, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey', borderBottom: 0 }}
                    align="right"
                    key={i}
                  >
                    {speed}
                  </TableCell>
                ))}
              </TableRow>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                    borderTop: 0,
                  }}
                >
                  Difference&nbsp;(MPH)
                </TableCell>

                {tableData.avgSpeed.map((avgSpeed, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey', borderTop: 0 }}
                    align="right"
                    key={i}
                  >
                    {avgSpeed !== null && tableData.speedLimit[i] !== null
                      ? (avgSpeed - tableData.speedLimit[i]).toFixed(1)
                      : 'No Data'}
                  </TableCell>
                ))}
              </TableRow>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                    borderBottom: 0,
                  }}
                >
                  85th%ile Speed&nbsp;(MPH)
                </TableCell>

                {tableData.eightyFifth.map((speed, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey', borderBottom: 0 }}
                    align="right"
                    key={i}
                  >
                    {speed}
                  </TableCell>
                ))}
              </TableRow>
              <TableRow>
                <TableCell
                  sx={{
                    width: '200px',
                    backgroundColor: '#F9F9FB',
                    fontWeight: 'bold',
                    border: '1px solid lightgrey',
                    borderTop: 0,
                  }}
                >
                  Difference&nbsp;(MPH)
                </TableCell>
                {tableData.eightyFifth.map((eightyFifth, i) => (
                  <TableCell
                    sx={{ border: '1px solid lightgrey', borderTop: 0 }}
                    align="right"
                    key={i}
                  >
                    {eightyFifth !== null && tableData.speedLimit[i] !== null
                      ? (eightyFifth - tableData.speedLimit[i]).toFixed(1)
                      : 'No Data'}
                  </TableCell>
                ))}
              </TableRow>
            </TableBody>
          </Table>
        </TableContainer>
    </>
  )
}

export default SpeedComplianceChartsContainer
