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
import { useState } from 'react'
import { AdjustmentDto, TransformedAdjustmentDto } from '../types'

const StyledTC = ({
  children,
  width = '100px',
}: {
  children: React.ReactNode
  width?: string
}) => <TableCell sx={{ minWidth: width }}>{children}</TableCell>

interface LinkPivotAdjustmentTableProps {
  data: AdjustmentDto[]
  cycleLength: number
}

const getTransformedData = (
  data: AdjustmentDto[]
): TransformedAdjustmentDto[] => {
  return data?.map((adjustment, index) => {
    return {
      ...adjustment,
      editLinkData: adjustment.delta,
      existingOffset: 0,
      newOffset: adjustment.delta,
      index: index,
    }
  })
}

const LinkPivotAdjustmentTable = ({
  data,
  cycleLength,
}: LinkPivotAdjustmentTableProps) => {
  const transformedData = getTransformedData(data)
  const [rows, setRows] = useState(transformedData)

  if (data === undefined) {
    return null
  }

  const updateExistingOffset = (
    row: TransformedAdjustmentDto,
    value: string
  ) => {
    const updatedRows = rows.map((oldRow) =>
      oldRow.index === row.index
        ? {
            ...oldRow,
            existingOffset: value !== '' ? parseInt(value) : 0,
            newOffset: calculateNewOffset(
              oldRow.index,
              value !== '' ? parseInt(value) : 0
            ),
          }
        : oldRow
    )
    setRows(updatedRows)
  }

  const updateLinkDelta = (row: TransformedAdjustmentDto, value: string) => {
    const updatedRows = rows
      .map((oldRow, index) =>
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
              <TableCell>Link</TableCell>
              <TableCell>Location Identifier</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Link Delta</TableCell>
              <StyledTC>Edit Link Delta</StyledTC>
              <TableCell>Offset(+ To Offset)</TableCell>
              <StyledTC>Existing Offset</StyledTC>
              <TableCell>New Offset</TableCell>
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
                <TableCell sx={{ width: '160px' }}>{index + 1}</TableCell>
                <TableCell sx={{ width: '160px' }}>
                  {row.locationIdentifier}
                </TableCell>
                <TableCell sx={{ width: '300px' }}>{row.location}</TableCell>
                <TableCell sx={{ width: '160px' }}>{row.delta}</TableCell>
                <LinkPivotEditableCell
                  value={row.editLinkData}
                  onUpdate={(value) => {
                    updateLinkDelta(row, value.toString())
                  }}
                  sx={{
                    width: '160px',
                  }}
                />
                <TableCell sx={{ width: '160px' }}>
                  {calculateOffsetToOffset(index)}
                </TableCell>
                <LinkPivotEditableCell
                  value={row.existingOffset}
                  onUpdate={(value) => {
                    updateExistingOffset(row, value.toString())
                  }}
                  sx={{
                    width: '160px',
                  }}
                />
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
