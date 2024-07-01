
import { useGetRouteLocations } from '@/features/routes/api/getRouteLocations'
import { RouteConfiguration } from '@/features/routes/routeConfiguration/routeConfiguration'
import DeleteIcon from '@mui/icons-material/DeleteOutlined'
import EditIcon from '@mui/icons-material/Edit'
import {
  Box,
  Button,
  Container,
  Modal,
  Paper,
  Typography,
  useTheme,
} from '@mui/material'
import {
  DataGrid,
  GridActionsCellItem,
  GridColDef,
  GridEventListener,
  GridRowEditStopReasons,
  GridRowId,
  GridRowModel,
  GridRowModesModel,
  gridClasses,
} from '@mui/x-data-grid'
import { useState } from 'react'

const editModalStyle = {
  transform: 'translate(5%, 5%)',
  width: '90%',
  height: '90%',
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
  overflow: 'auto',
}

export const RouteAdminChart = ({
  headers,
  data,
  baseRowType,
  onDelete,
  onEdit,
  onCreate,
}: DataGridChartProps) => {
  const [rows, setRows] = useState(data)
  const [rowModesModel, setRowModesModel] = useState<GridRowModesModel>({})
  const [open, setOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [id, setId] = useState(-1)
  const theme = useTheme()
  const mode = theme.palette.mode

  const { refetch, data: routeLocationsData } = useGetRouteLocations({
    routeId: id.toString(),
  })

  const handleClose = () => setOpen(false)

  const handleRowEditStop: GridEventListener<'rowEditStop'> = (
    params,
    event
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true
    }
  }

  const handleDeleteClick = (id: GridRowId) => {
    console.log('delete')
    setOpen(false)
    setRows(rows?.filter((row: any) => row.id !== (id as number)))
    setId(-1)
    onDelete(rows?.find((row: any) => row.id === (id as number)))
  }

  const processRowUpdate = (newRow: GridRowModel, oldRow: GridRowModel) => {
    if (newRow?.isNew) {
      onCreate(newRow)
    } else if (
      Object.values(newRow).toString() !== Object.values(oldRow).toString()
    ) {
      onEdit(newRow)
    }

    const updatedRow = { ...newRow, isNew: false }
    setRows(rows.map((row: any) => (row.id === newRow.id ? updatedRow : row)))
    return updatedRow
  }

  const handleRowModesModelChange = (newRowModesModel: GridRowModesModel) => {
    setRowModesModel(newRowModesModel)
  }

  const actionColumns: GridColDef[] = [
    {
      field: 'edit',
      type: 'actions',
      headerName: 'Edit',
      width: 100,
      cellClassName: 'edit-action',
      getActions: ({ id }) => {
        return [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Edit"
            className="textPrimary"
            onClick={() => {
              setEditOpen(true)
              setId(id as number)
            }}
            color="inherit"
            key={`e${id}`}
          />,
        ]
      },
    },
    {
      field: 'delete',
      type: 'actions',
      headerName: 'Delete',
      width: 100,
      cellClassName: 'delete-action',
      getActions: ({ id }) => {
        return [
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Delete"
            onClick={() => {
              setOpen(true)
              setId(id as number)
              refetch()
            }}
            color="inherit"
            key={`d${id}`}
          />,
        ]
      },
    },
  ]

  return (
    <>
      <Container>
        <Paper>
          <DataGrid
            rows={rows}
            columns={headers.concat(actionColumns)}
            getRowHeight={() => 'auto'}
            getEstimatedRowHeight={() => 200}
            editMode="row"
            rowModesModel={rowModesModel}
            onRowModesModelChange={handleRowModesModelChange}
            onRowEditStop={handleRowEditStop}
            processRowUpdate={processRowUpdate}
            autoHeight
            slots={{ toolbar: EditToolbar }}
            slotProps={{ toolbar: { baseRowType, setRows, setRowModesModel } }}
            pageSizeOptions={[{ value: 100, label: '100' }]}
            sx={{
              [`& .${gridClasses.cell}`]: {
                paddingTop: '20px',
                paddingBottom: '20px',
              },
              [`& .${gridClasses.columnHeaders}`]: {
                position: 'sticky',
                top: '40px',
                backgroundColor: mode === 'light' ? 'aliceblue' : 'black',
                zIndex: '1',
              },
              [`& .${gridClasses.toolbarContainer}`]: {
                position: 'sticky',
                top: '0',
                backgroundColor: mode === 'light' ? 'aliceblue' : 'black',
                zIndex: '1',
              },
              [`& .${gridClasses.main}`]: {
                overflow: 'inherit',
              },
            }}
          />
        </Paper>
      </Container>
      <Modal key={`M${id}`} open={open} onClose={handleClose}>
        <Box sx={modalStyle}>
          <Typography>Are you sure you want to delete this?</Typography>
          <Button onClick={() => handleDeleteClick(id)}>Yes</Button>
          <Button onClick={handleClose}>No</Button>
        </Box>
      </Modal>
      <Modal
        key={`EM${id}`}
        open={editOpen}
        onClose={() => {
          setEditOpen(false)
        }}
      >
        <Box sx={editModalStyle}>
          <RouteConfiguration routeLocations={routeLocationsData} />
        </Box>
      </Modal>
    </>
  )
}
