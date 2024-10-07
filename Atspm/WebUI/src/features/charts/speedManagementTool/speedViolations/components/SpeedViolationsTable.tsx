import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'

const DailySpeedViolationsTable = ({ response }) => {
  const { dailySpeedViolationsDto } = response
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
    <Box>
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
              <TableCell sx={{ border: '1px solid lightgrey', width: '220px' }}>
                Date Range
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', width: '150px' }}>
                Total Flow
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                Total Violations
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                Total Violations %
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                Total Extreme Violations
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                Total Extreme Violations %
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            <TableRow>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {formattedDate(dailySpeedViolationsDto[0].date)} -{' '}
                {formattedDate(dailySpeedViolationsDto.slice(-1)[0].date)}
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {response.totalFlow.toLocaleString()}
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {response.totalViolationsCount.toLocaleString()}
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {response.percentViolations.toFixed(2)}%
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {response.totalExtremeViolationsCount.toLocaleString()}
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey' }}>
                {response.percentExtremeViolations.toFixed(2)}%
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>

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
              <TableCell
                sx={{
                  border: '1px solid lightgrey',
                  top: '-21px',
                  width: '220px',
                }}
              >
                Date
              </TableCell>
              <TableCell
                sx={{
                  border: '1px solid lightgrey',
                  top: '-21px',
                  width: '150px',
                }}
              >
                Flow
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Violations
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Violations %
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Extreme Violations
              </TableCell>
              <TableCell sx={{ border: '1px solid lightgrey', top: '-21px' }}>
                Extreme Violations %
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {dailySpeedViolationsDto.map((dailyData, index) => (
              <TableRow key={index}>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {formattedDate(dailyData.date)}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {dailyData.dailyFlow.toLocaleString()}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {dailyData.dailyViolationsCount.toLocaleString()}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {(dailyData.dailyPercentViolations * 100).toFixed(2)}%
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {dailyData.dailyExtremeViolationsCount.toLocaleString()}
                </TableCell>
                <TableCell sx={{ border: '1px solid lightgrey' }}>
                  {(dailyData.dailyPercentExtremeViolations * 100).toFixed(2)}%
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}

export default DailySpeedViolationsTable
