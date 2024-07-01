import {
  Box,
  Input,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import { useState } from 'react'
import { AdjustmentDto, TransformedAdjustmentDto } from '../types'
import EditableTableCell from '@/features/locations/components/editableTableCell'

const StyledTC = ({
  children,
  width = '100px',
}: {
  children: React.ReactNode
  width?: string
}) => <TableCell sx={{ minWidth: width }}>{children}</TableCell>

interface props {
  data: AdjustmentDto[]
  cycleLength: number
}

// const columns: GridColDef[] = [
//   { field: 'linkNumber', headerName: 'Link', flex: 1 },
//   { field: 'locationIdentifier', headerName: 'Location Identifier', flex: 1 },
//   { field: 'location', headerName: 'Location', flex: 1 },
//   { field: 'delta', headerName: 'Link Delta', flex: 1 },
//   {
//     field: 'editLinkData',
//     headerName: 'Edit Link Delta',
//     flex: 1,
//     renderCell: (params) => (
//       <Input
//         type="number"
//         defaultValue={params.row.delta}
//         style={{ width: '50%' }}
//       />
//     ),
//   },
//   { field: 'adjustment', headerName: 'Offset(+ To Offset)', flex: 1 },
//   {
//     field: 'existingOffset',
//     headerName: 'Existing Offset',
//     flex: 1,
//     renderCell: (params) => (
//       <Box style={{ width: '100%', maxWidth: '150px' }}></Box>
//     ),
//   },
//   {
//     field: 'newOffset',
//     headerName: 'New Offset',
//     flex: 1,
//     valueGetter: (params: GridValueGetterParams) => updateNewOffset(params),
//   },
// ]

const getTransformedData = (
  data: AdjustmentDto[]
): TransformedAdjustmentDto[] => {
  return data?.map((adjustment) => {
    return {
      ...adjustment,
      editLinkData: adjustment.delta,
      existingOffset: 0,
      newOffset: adjustment.adjustment,
    }
  })
}

export const LinkPivotAdjustmentComponent = ({ data, cycleLength }: props) => {
  const tranformData = getTransformedData(data)
  const [rows, setRows] = useState(tranformData)

  if (data === undefined) {
    return
  }
  const updateExistingOffset = (
    row: TransformedAdjustmentDto,
    value: string
  ) => {
    row.existingOffset = value !== '' ? parseInt(value) : 0
    setRows((prevArray) =>
      prevArray.map((oldRow) =>
        oldRow.linkNumber === row.linkNumber ? row : oldRow
      )
    )
  }

  const updateLinkDelta = (row: TransformedAdjustmentDto, value: string) => {
    row.editLinkData = value !== '' ? parseInt(value) : 0
    setRows((prevArray) =>
      prevArray.map((oldRow) =>
        oldRow.linkNumber === row.linkNumber ? row : oldRow
      )
    )
  }

  const adjustOffset = () => {
    const rowCount = rows.length
    let cumulativeChange = 0
    for (let i = rowCount - 1; i >= 0; i--) {
      //Get the offset
      const offset = cumulativeChange + rows[i].editLinkData
      //add it to column 6
      if (offset >= cycleLength) {
        let tempOffset = offset
        while (tempOffset >= cycleLength) {
          tempOffset = tempOffset - cycleLength
        }
        rows[i].adjustment = tempOffset
      } else {
        rows[i].adjustment = offset
      }
      //Get the new offset
      const newOffset = offset + rows[i].existingOffset
      //Check to make sure that the new offset isn't greater than the cycle length and then set it to column 8
      if (newOffset > cycleLength) {
        let tempNewOffset = newOffset
        while (tempNewOffset > cycleLength) {
          tempNewOffset = tempNewOffset - cycleLength
        }
        rows[i].newOffset = tempNewOffset
      } else {
        rows[i].newOffset = newOffset
      }

      //update the cumulative change
      cumulativeChange = offset
      setRows((prevArray) =>
        prevArray.map((oldRow) =>
          oldRow.linkNumber === rows[i].linkNumber ? rows[i] : oldRow
        )
      )
    }
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
                sx={{
                  '& .MuiTableCell-body': {
                    fontSize: '1rem',
                    borderRight: '1px solid #e0e0e0',
                    lineHeight: 'inherit',
                    // width: '90px',
                },
              }}
              >
                <TableCell sx={{ width: '160px' }}>{row.linkNumber}</TableCell>
                <TableCell sx={{ width: '160px' }}>{row.locationIdentifier}</TableCell>
                <TableCell>{row.location}</TableCell>
                <TableCell sx={{ width: '160px' }}>{row.delta}</TableCell>
                <EditableTableCell
                  value={row.editLinkData}
                  onUpdate={(value) => {
                    updateLinkDelta(row, value.toString())
                    adjustOffset()
                  }}
                  sx={{ width: '160px' }}
                />
                <EditableTableCell
                  value={row.adjustment}
                  onUpdate={(value) => {
                    updateExistingOffset(row, value.toString())
                    adjustOffset()
                  }}
                  sx={{ width: '160px' }}
                />
                <TableCell sx={{ width: '160px' }}>{row.existingOffset}</TableCell>
                <TableCell sx={{ width: '160px' }}>{row.newOffset}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}
