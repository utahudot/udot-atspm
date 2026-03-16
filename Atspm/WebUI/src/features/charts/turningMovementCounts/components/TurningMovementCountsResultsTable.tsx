import {
  Box,
  Divider,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import type { ReactNode } from 'react'
import { useMemo } from 'react'

type TableRowT = (string | number)[]

interface HeaderRow1Cell {
  label: string
  colSpan: number
  divider?: boolean
}

interface HeaderRow2Cell {
  label: ReactNode
  colSpan: number
}

interface TurningMovementCountsResultsTableProps {
  headerRow1: HeaderRow1Cell[]
  headerRow2: HeaderRow2Cell[]
  rows: TableRowT[]
  dirTotalIdx: Set<number>
  binTotalIdx: number
  peakHour?: {
    peakHourFactor: number | null
    peakHourData: TableRowT[]
  } | null
  labels: {
    columnGroups: { title: string | null; columns: string[] }[]
    flatColumns: string[]
  }
  showPeakHour: boolean
}

const HEADER_ROW_1_HEIGHT_PX = 40

export default function TurningMovementCountsResultsTable({
  headerRow1,
  headerRow2,
  rows,
  dirTotalIdx,
  binTotalIdx,
  peakHour,
  labels,
  showPeakHour,
}: TurningMovementCountsResultsTableProps) {
  const peakHourHighlightColumns = useMemo(() => {
    const peakHourDirTotalIdx = new Set<number>()
    let peakHourBinTotalIdx = -1
    let columnIndex = 0

    labels.columnGroups.forEach((group) => {
      if (group.title && group.columns.length) {
        peakHourDirTotalIdx.add(columnIndex + group.columns.length - 1)
      }

      if (!group.title) {
        const binTotalOffset = group.columns.indexOf('Bin Total')
        if (binTotalOffset >= 0) {
          peakHourBinTotalIdx = columnIndex + binTotalOffset
        }
      }

      columnIndex += group.columns.length
    })

    return {
      dirTotalIdx: peakHourDirTotalIdx,
      binTotalIdx: peakHourBinTotalIdx,
    }
  }, [labels.columnGroups])

  const cellStyle = (
    ci: number,
    activeDirTotalIdx: Set<number> = dirTotalIdx,
    activeBinTotalIdx: number = binTotalIdx
  ) => {
    if (ci === activeBinTotalIdx) return { backgroundColor: '#ADDFFF50' }
    if (activeDirTotalIdx.has(ci)) return { backgroundColor: '#90EE9050' }
    return {}
  }

  const renderBodyRows = (
    allRows: TableRowT[],
    activeDirTotalIdx: Set<number> = dirTotalIdx,
    activeBinTotalIdx: number = binTotalIdx
  ) =>
    allRows.map((row, ri) => {
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
          {row?.map((cell, ci) => (
            <TableCell
              key={ci}
              sx={cellStyle(ci, activeDirTotalIdx, activeBinTotalIdx)}
              align={ci === 0 ? 'left' : 'right'}
            >
              {typeof cell === 'number' ? cell.toLocaleString() : cell}
            </TableCell>
          ))}
        </TableRow>
      )
    })

  return (
    <>
      <TableContainer
        component={Paper}
        variant="outlined"
        sx={{
          borderRadius: 2,
          borderColor: 'grey.200',
          maxHeight: '95vh',
          overflow: 'scroll',
        }}
      >
        <Table stickyHeader>
          <TableHead
            sx={{
              '--tmc-header-row1-height': `${HEADER_ROW_1_HEIGHT_PX}px`,
              '& .MuiTableCell-head': {
                fontSize: '0.8rem',
                bgcolor: 'grey.100',
                lineHeight: '1rem',
                padding: '0.5rem',
                borderBottom: '1px solid',
                borderColor: 'divider',
              },
            }}
          >
            <TableRow>
              {headerRow1.map((h, i) => (
                <TableCell
                  key={i}
                  colSpan={h.colSpan}
                  sx={{
                    pb: 0,
                    top: 0,
                    zIndex: 4,
                    height: 'var(--tmc-header-row1-height)',
                  }}
                >
                  {h.divider ? <Divider>{h.label}</Divider> : null}
                </TableCell>
              ))}
            </TableRow>

            <TableRow>
              {headerRow2.map((h, i) => (
                <TableCell
                  key={i}
                  align="center"
                  colSpan={h.colSpan}
                  sx={{
                    fontWeight: 600,
                    top: 'var(--tmc-header-row1-height)',
                    zIndex: 3,
                  }}
                >
                  {h.label}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>{renderBodyRows(rows)}</TableBody>
        </Table>
      </TableContainer>
      {peakHour ? (
        showPeakHour ? (
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
                      {g.title ? <Divider>{g.title}</Divider> : null}
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

              <TableBody>
                {renderBodyRows(
                  peakHour.peakHourData,
                  peakHourHighlightColumns.dirTotalIdx,
                  peakHourHighlightColumns.binTotalIdx
                )}
              </TableBody>
            </Table>
          </TableContainer>
        ) : null
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
    </>
  )
}
