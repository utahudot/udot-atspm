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
import { useMemo } from 'react'

interface ColumnGroup {
  title: string | null
  columns: string[]
}
interface Labels {
  columnGroups: ColumnGroup[]
  flatColumns: string[]
}
type TableRow = (string | number)[]
interface TurningMovementCountsTableProps {
  chartData: {
    data: {
      labels: Labels
      table: TableRow[]
      peakHourFactor: number
      peakHour?: TableRow[]
    }
  }
}

const TurningMovementCountsTable = ({
  chartData,
}: TurningMovementCountsTableProps) => {
  const { labels, table, peakHour } = chartData.data

  /* indices needing background colours */
  const { dirTotalIdx, binTotalIdx } = useMemo(() => {
    const dirTotal = new Set<number>()
    let binTotal = -1
    let i = 0
    labels.columnGroups.forEach((group) => {
      group.columns.forEach((col, ci) => {
        if (col === 'Total' && group.title) dirTotal.add(i)
        if (col === 'Bin Total') binTotal = i
        i++
      })
    })
    return { dirTotalIdx: dirTotal, binTotalIdx: binTotal }
  }, [labels.columnGroups])

  const cellStyle = (ci: number) => {
    if (ci === binTotalIdx) return { backgroundColor: '#ADDFFF50' }
    if (dirTotalIdx.has(ci)) return { backgroundColor: '#90EE9050' }
    return {}
  }

  const renderBodyRows = (rows: TableRow[]) =>
    rows.map((row, ri) => {
      const isFooter = row[0] === 'Total'
      return (
        <TableRow
          key={ri}
          sx={{
            '& .MuiTableCell-body': {
              fontSize: '0.9rem',
              borderRight: '1px solid #e0e0e0',
              padding: '.7rem',
            },
            ...(isFooter
              ? { bgcolor: '#e0e0e0' }
              : { '&:nth-of-type(odd)': { backgroundColor: '#f4f4f4' } }),
          }}
        >
          {row.map((cell, ci) => (
            <TableCell key={ci} sx={cellStyle(ci)}>
              {typeof cell === 'number' ? cell.toLocaleString() : cell}
            </TableCell>
          ))}
        </TableRow>
      )
    })

  return (
    <Box sx={{ mt: 4 }}>
      <Accordion disableGutters>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" component="h2">
            Data Table
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          {/* ---------------- Main Table ---------------- */}
          <TableContainer>
            <Table sx={{ minWidth: 800 }}>
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
                {/* first header row (direction titles) */}
                <TableRow>
                  {labels.columnGroups.map((g, gi) => (
                    <TableCell
                      key={gi}
                      colSpan={g.columns.length}
                      sx={{ borderBottom: 'none', pb: 0 }}
                    >
                      {g.title && <Divider>{g.title}</Divider>}
                    </TableCell>
                  ))}
                </TableRow>

                {/* second header row (column names) */}
                <TableRow>
                  {labels.flatColumns.map((col, ci) => (
                    <TableCell key={ci} align="center">
                      {col}
                    </TableCell>
                  ))}
                </TableRow>
              </TableHead>

              <TableBody>{renderBodyRows(table)}</TableBody>
            </Table>
          </TableContainer>

          {/* ---------------- Peak‑Hour Table ---------------- */}
          {peakHour ? (
            <TableContainer sx={{ mt: 4 }}>
              <Table sx={{ minWidth: 800 }}>
                <caption
                  style={{
                    captionSide: 'top',
                    textAlign: 'center',
                    padding: '0.5rem',
                    fontSize: '0.9rem',
                    fontWeight: 500,
                  }}
                >
                  Peak Hour{' '}
                  {peakHour.peakHourFactor
                    ? `(PHF = ${peakHour.peakHourFactor.toFixed(2)})`
                    : null}
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
                    {labels.columnGroups.map((g, gi) => (
                      <TableCell
                        key={gi}
                        colSpan={g.columns.length}
                        sx={{ borderBottom: 'none', pb: 0 }}
                      >
                        {g.title && <Divider>{g.title}</Divider>}
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    {labels.flatColumns.map((col, ci) => (
                      <TableCell key={ci} align="center">
                        {col === 'Hour' ? 'Hour' : col}
                      </TableCell>
                    ))}
                  </TableRow>
                </TableHead>

                <TableBody>{renderBodyRows(peakHour.peakHourData)}</TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Box
              sx={{
                height: 100,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Typography
                variant="body2"
                color="textSecondary"
                sx={{ mt: 2, textAlign: 'center' }}
              >
                Select a time range 1 hour or greater to view peak hour data.
              </Typography>
            </Box>
          )}
        </AccordionDetails>
      </Accordion>
    </Box>
  )
}

export default TurningMovementCountsTable
