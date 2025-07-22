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
import { Fragment } from 'react'

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

  const timestamps = Array.from(
    new Set(table.flatMap((d) => d.volumes.map((v) => v.timestamp)))
  ).sort((a, b) => a.localeCompare(b))

  const findVolume = (
    direction: string,
    movementType: string,
    timestamp: string
  ) =>
    table
      .filter(
        (d) => d.direction === direction && d.movementType === movementType
      )
      .flatMap((d) => d.volumes)
      .find((v) => v.timestamp === timestamp)?.value || 0

  const formatTime = (ts: string) =>
    new Intl.DateTimeFormat('default', {
      hour: 'numeric',
      minute: 'numeric',
      hour12: false,
    }).format(new Date(ts))

  const directions = ['Eastbound', 'Westbound', 'Northbound', 'Southbound']

  // 4) build, per-direction, the list of movementTypes actually present
  const preferredOrder = ['Left', 'Thru', 'Thru-Right', 'Right']
  const movementTypesByDirection: Record<string, string[]> = {}
  directions.forEach((dir) => {
    const types = Array.from(
      new Set(
        table.filter((d) => d.direction === dir).map((d) => d.movementType)
      )
    )
    types.sort((a, b) => {
      const ia = preferredOrder.indexOf(a)
      const ib = preferredOrder.indexOf(b)
      if (ia === -1 || ib === -1) return a.localeCompare(b)
      return ia - ib
    })
    movementTypesByDirection[dir] = types
  })

  // 5) precompute totals for footer
  const totalPerColumn = directions.map((dir) =>
    movementTypesByDirection[dir].map((mt) =>
      timestamps.reduce((sum, ts) => sum + findVolume(dir, mt, ts), 0)
    )
  )
  const totalPerDirection = totalPerColumn.map((arr) =>
    arr.reduce((a, b) => a + b, 0)
  )
  const grandTotal = totalPerDirection.reduce((a, b) => a + b, 0)

  return (
    <Box sx={{ mt: 4 }}>
      <Accordion disableGutters>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" component="h2">
            Data Table
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          {/* --- Main Volume Table --- */}
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
                  {directions.map((dir) => (
                    <TableCell
                      key={dir}
                      colSpan={movementTypesByDirection[dir].length + 1}
                      sx={{ borderBottom: 'none', pb: 0 }}
                    >
                      <Divider>{dir}</Divider>
                    </TableCell>
                  ))}
                </TableRow>
                <TableRow>
                  <TableCell align="center">Bin</TableCell>
                  {directions.map((dir) => (
                    <Fragment key={dir}>
                      {movementTypesByDirection[dir].map((mt) => (
                        <TableCell align="center" key={`${dir}-${mt}`}>
                          {mt}
                        </TableCell>
                      ))}
                      <TableCell align="center">Total</TableCell>
                    </Fragment>
                  ))}
                  <TableCell align="center">Bin Total</TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                {timestamps.map((ts) => (
                  <TableRow
                    key={ts}
                    sx={{
                      '& .MuiTableCell-body': {
                        fontSize: '0.9rem',
                        borderRight: '1px solid #e0e0e0',
                      },
                      '&:nth-of-type(odd)': { backgroundColor: '#f4f4f4' },
                    }}
                  >
                    <TableCell>{formatTime(ts)}</TableCell>

                    {directions.map((dir) => {
                      const counts = movementTypesByDirection[dir].map((mt) =>
                        findVolume(dir, mt, ts)
                      )
                      const dirSum = counts.reduce((a, b) => a + b, 0)

                      return (
                        <Fragment key={dir}>
                          {counts.map((c, i) => (
                            <TableCell key={`${dir}-${i}-${ts}`}>{c}</TableCell>
                          ))}
                          <TableCell sx={{ backgroundColor: '#90EE9050' }}>
                            {dirSum}
                          </TableCell>
                        </Fragment>
                      )
                    })}

                    <TableCell sx={{ backgroundColor: '#ADDFFF50' }}>
                      {directions
                        .reduce(
                          (sumD, dir) =>
                            sumD +
                            movementTypesByDirection[dir].reduce(
                              (sumM, mt) => sumM + findVolume(dir, mt, ts),
                              0
                            ),
                          0
                        )
                        .toLocaleString()}
                    </TableCell>
                  </TableRow>
                ))}

                {/* Footer Totals */}
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
                  {directions.map((dir, di) => (
                    <Fragment key={dir}>
                      {totalPerColumn[di].map((tot, ti) => (
                        <TableCell key={`tot-${dir}-${ti}`}>
                          {tot.toLocaleString()}
                        </TableCell>
                      ))}
                      <TableCell sx={{ backgroundColor: '#90EE9050' }}>
                        {totalPerDirection[di].toLocaleString()}
                      </TableCell>
                    </Fragment>
                  ))}
                  <TableCell sx={{ backgroundColor: '#ADDFFF50' }}>
                    {grandTotal.toLocaleString()}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>

          {/* --- Peak Hour Table --- */}
          <TableContainer sx={{ marginTop: 4 }}>
            <Table sx={{ minWidth: 800 }} aria-label="peak hour volume table">
              <caption
                style={{
                  captionSide: 'top',
                  textAlign: 'center',
                  padding: '0.5rem',
                  fontSize: '0.9rem',
                  fontWeight: 500,
                }}
              >
                Peak Hour (PHF = {peakHourFactor})
              </caption>

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
                  {directions.map((dir) => (
                    <TableCell
                      key={dir}
                      colSpan={movementTypesByDirection[dir].length + 1}
                      sx={{ borderBottom: 'none', pb: 0 }}
                    >
                      <Divider>{dir}</Divider>
                    </TableCell>
                  ))}
                </TableRow>
                <TableRow>
                  <TableCell align="center">Hour</TableCell>
                  {directions.map((dir) => (
                    <Fragment key={dir}>
                      {movementTypesByDirection[dir].map((mt) => {
                        return (
                          <TableCell align="center" key={`${dir}-peak-${mt}`}>
                            {mt}
                          </TableCell>
                        )
                      })}
                      <TableCell align="center">Total</TableCell>
                    </Fragment>
                  ))}
                  <TableCell align="center">Bin Total</TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                <TableRow
                  sx={{
                    '& .MuiTableCell-body': {
                      fontSize: '0.9rem',
                      borderRight: '1px solid #e0e0e0',
                    },
                  }}
                >
                  <TableCell>
                    {formatTime(peakHour.key)} –{' '}
                    {formatTime(
                      addHours(new Date(peakHour.key), 1).toISOString()
                    )}
                  </TableCell>

                  {directions.map((dir) => {
                    const counts = movementTypesByDirection[dir].map((mt) => {
                      const rec = table.find(
                        (d) => d.direction === dir && d.movementType === mt
                      )
                      return rec?.peakHourVolume.value || 0
                    })
                    const dirSum = counts.reduce((a, b) => a + b, 0)

                    return (
                      <Fragment key={`peak-${dir}`}>
                        {counts.map((c, i) => (
                          <TableCell key={`${dir}-peak-${i}`}>
                            {c.toLocaleString()}
                          </TableCell>
                        ))}
                        <TableCell sx={{ backgroundColor: '#90EE9050' }}>
                          {dirSum.toLocaleString()}
                        </TableCell>
                      </Fragment>
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
