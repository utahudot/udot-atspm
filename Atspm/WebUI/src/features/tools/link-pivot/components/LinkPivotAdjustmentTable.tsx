import LinkPivotEditableCell from '@/features/tools/link-pivot/components/LinkPivotEditableCell/LinkPivotEditableCell'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import { useEffect, useMemo, useState } from 'react'
import { AdjustmentDto, TransformedAdjustmentDto } from '../types'

const HeaderCell = ({
  label,
  width = '100px',
}: {
  label: string
  width?: string
}) => (
  <TableCell sx={{ minWidth: width, verticalAlign: 'top' }}>
    <Box sx={{ fontWeight: 700 }}>{label}</Box>
  </TableCell>
)

interface LinkPivotAdjustmentTableProps {
  data: AdjustmentDto[]
  cycleLength: number
}

const getTransformedData = (
  data: AdjustmentDto[]
): TransformedAdjustmentDto[] => {
  return data?.map((adjustment, index) => {
    const existingOffset = adjustment.existingOffset ?? 0

    return {
      ...adjustment,
      editLinkData: adjustment.delta,
      existingOffset,
      newOffset: existingOffset + adjustment.delta,
      index: index,
    }
  })
}

const LinkPivotAdjustmentTable = ({
  data,
  cycleLength,
}: LinkPivotAdjustmentTableProps) => {
  const transformedData = useMemo(() => getTransformedData(data), [data])
  const [rows, setRows] = useState(transformedData)

  useEffect(() => {
    setRows(transformedData)
  }, [transformedData])

  if (data === undefined) {
    return null
  }

  const updateLinkDelta = (row: TransformedAdjustmentDto, value: string) => {
    const updatedRows = rows
      .map((oldRow) =>
        oldRow.index === row.index
          ? {
              ...oldRow,
              editLinkData: value !== '' ? parseInt(value) : 0,
            }
          : oldRow
      )
      .map((oldRow, index) => ({
        ...oldRow,
        newOffset: calculateNewOffset(index, oldRow.existingOffset),
      }))
    setRows(updatedRows)
  }

  const calculateNewOffset = (index: number, existingOffset: number) => {
    let cumulativeChange = 0
    for (let i = index; i < rows.length; i++) {
      cumulativeChange += rows[i].editLinkData
    }
    const newOffset = cumulativeChange + existingOffset
    if (!Number.isFinite(cycleLength) || cycleLength <= 0) {
      return newOffset
    }

    return newOffset >= cycleLength ? newOffset % cycleLength : newOffset
  }

  const calculateOffsetToOffset = (index: number) => {
    let cumulativeChange = 0
    for (let i = index; i < rows.length; i++) {
      cumulativeChange += rows[i].editLinkData
    }
    return cumulativeChange
  }

  return (
    <Box>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <HeaderCell label="Link" />
              <HeaderCell label="Location" width="260px" />
              <HeaderCell label="Recommended Change" />
              <HeaderCell label="Manual Adjustment" />
              <HeaderCell label="Cumulative Change" />
              <HeaderCell label="Existing Offset" />
              <HeaderCell label="Target Offset" />
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row, index) => (
              <TableRow
                key={index}
                sx={{
                  '& .MuiTableCell-body': {
                    fontSize: '1rem',
                    lineHeight: 'inherit',
                  },
                }}
              >
                <TableCell sx={{ width: '160px' }}>{row.linkNumber}</TableCell>
                <TableCell sx={{ width: '460px' }}>
                  <Box component="span" sx={{ fontWeight: 700 }}>
                    {row.locationIdentifier}
                  </Box>
                  {' - '}
                  {row.location}
                </TableCell>
                <TableCell sx={{ width: '160px' }}>{row.delta}</TableCell>
                <LinkPivotEditableCell
                  value={row.editLinkData}
                  onUpdate={(value) => {
                    updateLinkDelta(row, value?.toString() ?? '')
                  }}
                  sx={{
                    width: '160px',
                  }}
                />
                <TableCell sx={{ width: '160px' }}>
                  {calculateOffsetToOffset(index)}
                </TableCell>
                <TableCell sx={{ width: '160px' }}>
                  {row.existingOffset}
                </TableCell>
                <TableCell sx={{ width: '160px' }}>
                  {calculateNewOffset(index, row.existingOffset)}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}

export default LinkPivotAdjustmentTable
