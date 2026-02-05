import { UsageEntry } from '@/api/config'
import UsageEntryDrawer from '@/features/data/components/UsageEntryDrawer'
import { formatMs } from '@/utils/formatting'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import { Box, Chip } from '@mui/material'
import {
  DataGrid,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarExport,
  GridToolbarFilterButton,
} from '@mui/x-data-grid'
import { GridColDef } from '@mui/x-data-grid/models/colDef/gridColDef'
import React from 'react'

interface UsageTableProps {
  isLoading: boolean
  rows: UsageEntry[]
}

export default function UsageTable({ isLoading, rows }: UsageTableProps) {
  const [active, setActive] = React.useState<UsageEntry | null>(null)

  const columns = React.useMemo<GridColDef<UsageEntry>[]>(
    () => [
      {
        field: 'timestamp',
        headerName: 'Timestamp',
        width: 190,
        valueGetter: (_, row) => row.timestamp,
      },
      { field: 'apiName', headerName: 'API', width: 200 },
      { field: 'method', headerName: 'Method', width: 90 },
      {
        field: 'statusCode',
        headerName: 'Status',
        width: 90,
        type: 'number',
      },
      {
        field: 'success',
        headerName: 'Success',
        width: 110,
        sortable: false,
        renderCell: (p) =>
          p.row.success ? (
            <Chip
              size="small"
              icon={<CheckCircleIcon />}
              label="Success"
              color="success"
              variant="outlined"
            />
          ) : (
            <Chip
              size="small"
              icon={<ErrorOutlineIcon />}
              label="Failed"
              color="error"
              variant="outlined"
            />
          ),
      },
      {
        field: 'durationMs',
        headerName: 'Duration',
        width: 120,
        type: 'number',
        valueFormatter: (v) => formatMs(Number(v)),
      },
      { field: 'user', headerName: 'User', width: 160 },
      { field: 'route', headerName: 'Route', width: 280 },
      { field: 'controller', headerName: 'Controller', width: 160 },
      { field: 'action', headerName: 'Action', width: 160 },
      {
        field: 'resultCount',
        headerName: 'Count',
        width: 90,
        type: 'number',
      },
    ],
    []
  )

  return (
    <Box sx={{ height: 720 }}>
      <DataGrid
        rows={rows}
        columns={columns}
        loading={isLoading}
        onRowClick={(p) => setActive(p.row)}
        slots={{ toolbar: UsageGridToolbar }}
        hideFooterSelectedRowCount
        sx={{
          border: 0,
          '& .MuiDataGrid-toolbarContainer': { px: 1 },
          '@media print': { zoom: 0.65 },
        }}
      />
      <UsageEntryDrawer active={active} setActive={setActive} />
    </Box>
  )
}

function UsageGridToolbar() {
  return (
    <GridToolbarContainer sx={{ px: 1, py: 0.5, gap: 1 }}>
      <GridToolbarColumnsButton />
      <GridToolbarFilterButton />
      <GridToolbarExport
        printOptions={{ hideFooter: true, hideToolbar: true }}
      />
    </GridToolbarContainer>
  )
}
