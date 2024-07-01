import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import React from 'react'

const DataTable = ({ option }) => {
  const axisData = option.xAxis[0].data
  const series = option.series

  return (
    <TableContainer component={Paper} style={{ maxWidth: '100%' }}>
      <Table aria-label="ECharts Data Table">
        <TableHead>
          <TableRow>
            {series.map((s, index) => (
              <TableCell key={index} colSpan={2} align="center">
                {s.name}
              </TableCell>
            ))}
          </TableRow>
          <TableRow>
            {series.map((_, index) => (
              <React.Fragment key={index}>
                <TableCell align="center">Timestamp</TableCell>
                <TableCell align="center">Value</TableCell>
              </React.Fragment>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {axisData.map((timestamp, rowIndex) => (
            <TableRow key={rowIndex}>
              {series.map((s, seriesIndex) => (
                <React.Fragment key={seriesIndex}>
                  <TableCell align="center">{timestamp}</TableCell>
                  <TableCell align="center">{s.data[rowIndex]}</TableCell>
                </React.Fragment>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default DataTable
