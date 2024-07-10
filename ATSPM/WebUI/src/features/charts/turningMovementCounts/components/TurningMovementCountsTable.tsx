import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { addHours } from 'date-fns'

interface VolumeData {
  value: number
  timestamp: string
}

interface TrafficData {
  direction: string
  movementType: string
  laneType: string
  volumes: VolumeData[]
  peakHourVolume: {
    key: string
    value: number
  }
}

interface TurningMovementCountsTableProps {
  chartData: {
    data: {
      table: TrafficData[]
      peakHourFactor: number
      peakHour: VolumeData
    }
  }
}

const TurningMovementCountsTable = ({
  chartData,
}: TurningMovementCountsTableProps) => {
  const { table, peakHourFactor, peakHour } = chartData.data
  const timestamps = [
    ...new Set(
      table.flatMap((data) => data.volumes.map((volume) => volume.timestamp))
    ),
  ].sort()

  const findVolumeByTimestamp = (
    direction: string,
    movementType: string,
    timestamp: string
  ) => {
    return (
      table
        .filter(
          (data) =>
            data.direction === direction && data.movementType === movementType
        )
        .flatMap((data) => data.volumes)
        .find((volume) => volume.timestamp === timestamp)?.value || 0
    )
  }

  const formatTime = (timestamp: string) => {
    const formatter = new Intl.DateTimeFormat('default', {
      hour: 'numeric',
      minute: 'numeric',
      hour12: false,
    })

    console.log('timestamp', timestamp)

    const date = new Date(timestamp)
    return formatter.format(date)
  }

  const directions = ['Eastbound', 'Westbound', 'Northbound', 'Southbound']
  const movementTypes = ['Left', 'Thru', 'Right']

  const totalPerColumn = directions.map((direction) =>
    movementTypes.map((type) =>
      timestamps.reduce(
        (sum, timestamp) =>
          sum + findVolumeByTimestamp(direction, type, timestamp),
        0
      )
    )
  )

  const totalPerDirection = totalPerColumn.map((totals) =>
    totals.reduce((acc, curr) => acc + curr, 0)
  )

  const grandTotal = totalPerDirection.reduce((acc, curr) => acc + curr, 0)

  return (
    <Box sx={{ mt: 4 }}>
      <Accordion disableGutters>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4">Data Table</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <TableContainer>
            <Table sx={{ minWidth: 800 }} aria-label="traffic table">
              <TableHead
                sx={{
                  '& .MuiTableCell-head': {
                    fontSize: '0.8rem',
                    bgcolor: 'white',
                    lineHeight: '1rem',
                    padding: '0.5rem',
                  },
                }}
              >
                <TableRow>
                  <TableCell sx={{ borderBottom: 'none', pb: 0 }} />
                  {directions.map((direction) => (
                    <TableCell
                      key={direction}
                      colSpan={4}
                      sx={{ borderBottom: 'none', pb: 0 }}
                    >
                      <Divider>{direction}</Divider>
                    </TableCell>
                  ))}
                </TableRow>
                <TableRow>
                  <TableCell align="center">Bin</TableCell>
                  {directions.map((direction) => (
                    <>
                      <TableCell align="center" key={`${direction}-L`}>
                        L
                      </TableCell>
                      <TableCell align="center" key={`${direction}-T`}>
                        T
                      </TableCell>
                      <TableCell align="center" key={`${direction}-R`}>
                        R
                      </TableCell>
                      <TableCell align="center" key={`${direction}-Total`}>
                        Total
                      </TableCell>
                    </>
                  ))}
                  <TableCell align="center">Bin Total</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {timestamps.map((timestamp) => (
                  <TableRow
                    key={timestamp}
                    sx={{
                      '& .MuiTableCell-body': {
                        fontSize: '0.9rem',
                        borderRight: '1px solid #e0e0e0',
                      },
                      '&:nth-of-type(odd)': {
                        backgroundColor: '#f4f4f4',
                      },
                    }}
                  >
                    <TableCell>{formatTime(timestamp)}</TableCell>
                    {directions.map((direction) => {
                      const totals = movementTypes.map((type) =>
                        findVolumeByTimestamp(direction, type, timestamp)
                      )
                      const directionTotal = totals.reduce(
                        (acc, curr) => acc + curr,
                        0
                      )
                      return (
                        <>
                          {totals.map((total, index) => (
                            <TableCell
                              key={`${direction}-${movementTypes[index]}-${timestamp}`}
                            >
                              {total}
                            </TableCell>
                          ))}
                          <TableCell
                            key={`${direction}-total-${timestamp}`}
                            sx={{ backgroundColor: '#90EE9050' }}
                          >
                            {directionTotal}
                          </TableCell>
                        </>
                      )
                    })}
                    <TableCell sx={{ backgroundColor: '#ADDFFF50' }}>
                      {directions
                        .reduce(
                          (acc, direction) =>
                            acc +
                            movementTypes.reduce(
                              (sum, type) =>
                                sum +
                                findVolumeByTimestamp(
                                  direction,
                                  type,
                                  timestamp
                                ),
                              0
                            ),
                          0
                        )
                        .toLocaleString()}
                    </TableCell>
                  </TableRow>
                ))}
                <TableRow
                  sx={{
                    '& .MuiTableCell-body': {
                      fontSize: '0.9rem',
                      borderRight: '1px solid #d0d0d0',
                      bgcolor: '#e0e0e0',
                    },
                  }}
                >
                  <TableCell>Total</TableCell>
                  {totalPerColumn.flatMap((totals, dirIndex) => (
                    <>
                      {totals.map((total, typeIndex) => (
                        <TableCell
                          key={`total-${directions[dirIndex]}-${movementTypes[typeIndex]}`}
                        >
                          {total.toLocaleString()}
                        </TableCell>
                      ))}
                      <TableCell
                        key={`dir-total-${directions[dirIndex]}`}
                        sx={{ backgroundColor: '#90EE9050' }}
                      >
                        {totalPerDirection[dirIndex].toLocaleString()}
                      </TableCell>
                    </>
                  ))}
                  <TableCell sx={{ backgroundColor: '#ADDFFF50' }}>
                    {grandTotal.toLocaleString()}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>
          <TableContainer sx={{ marginTop: 4 }}>
            <Table sx={{ minWidth: 800 }} aria-label="peak hour volume table">
              <TableHead
                sx={{
                  '& .MuiTableCell-head': {
                    fontSize: '0.8rem',
                    bgcolor: 'white',
                    lineHeight: '1rem',
                    padding: '0.5rem',
                  },
                }}
              >
                <TableRow>
                  <TableCell colSpan={7} sx={{ borderBottom: 'none', pb: 0 }} />
                  <TableCell
                    colSpan={4}
                    sx={{ borderBottom: 'none', pb: 0, textAlign: 'center' }}
                  >
                    Peak Hour (PHF = {peakHourFactor})
                  </TableCell>
                  <TableCell colSpan={7} sx={{ borderBottom: 'none', pb: 0 }} />
                </TableRow>
                <TableRow>
                  <TableCell sx={{ borderBottom: 'none', pb: 0 }} />
                  {directions.map((direction) => (
                    <TableCell
                      key={direction}
                      colSpan={4}
                      sx={{ borderBottom: 'none', pb: 0 }}
                    >
                      <Divider>{direction}</Divider>
                    </TableCell>
                  ))}
                </TableRow>
                <TableRow>
                  <TableCell align="center">Bin</TableCell>
                  {directions.map((direction) => (
                    <>
                      <TableCell align="center" key={`${direction}-L`}>
                        L
                      </TableCell>
                      <TableCell align="center" key={`${direction}-T`}>
                        T
                      </TableCell>
                      <TableCell align="center" key={`${direction}-R`}>
                        R
                      </TableCell>
                      <TableCell align="center" key={`${direction}-Total`}>
                        Total
                      </TableCell>
                    </>
                  ))}
                  <TableCell align="center">Bin Total</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow
                  sx={{
                    '& .MuiTableCell-body': {
                      fontSize: '0.9rem',
                      borderRight: '1px solid #d0d0d0',
                    },
                  }}
                >
                  <TableCell>
                    {/* add hour to end time */}
                    {formatTime(peakHour.key)} -{' '}
                    {formatTime(
                      addHours(new Date(peakHour.key), 1).toISOString()
                    )}
                  </TableCell>
                  {directions.map((direction) => {
                    const directionData = table.filter(
                      (data) => data.direction === direction
                    )
                    const totals = movementTypes.map((type) => {
                      const typeData = directionData.find(
                        (data) => data.movementType === type
                      )
                      return typeData?.peakHourVolume.value || 0
                    })
                    const directionTotal = totals.reduce(
                      (acc, curr) => acc + curr,
                      0
                    )
                    return (
                      <>
                        {totals.map((total, index) => (
                          <TableCell
                            key={`${direction}-${movementTypes[index]}-peak`}
                          >
                            {total.toLocaleString()}
                          </TableCell>
                        ))}
                        <TableCell
                          key={`${direction}-total-peak`}
                          sx={{ backgroundColor: '#90EE9050' }}
                        >
                          {directionTotal.toLocaleString()}
                        </TableCell>
                      </>
                    )
                  })}
                  <TableCell sx={{ backgroundColor: '#ADDFFF50' }}>
                    {peakHour.value.toLocaleString()}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>
        </AccordionDetails>
      </Accordion>
    </Box>
  )
}

export default TurningMovementCountsTable
