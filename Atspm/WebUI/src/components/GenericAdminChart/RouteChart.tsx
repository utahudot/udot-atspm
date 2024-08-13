import { RolesResponse } from '@/features/identity/types/roles'
import { navigateToPage } from '@/utils/routes'
import AddIcon from '@mui/icons-material/Add'
import CancelIcon from '@mui/icons-material/Close'
import DeleteIcon from '@mui/icons-material/DeleteOutlined'
import EditIcon from '@mui/icons-material/Edit'
import SaveIcon from '@mui/icons-material/Save'
import {
  Box,
  Button,
  Divider,
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
  GridRowModes,
  GridRowModesModel,
  GridRowsProp,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarExport,
  GridToolbarFilterButton,
  gridClasses,
  useGridApiRef,
} from '@mui/x-data-grid'
import { useState } from 'react'
import { modalStyle } from './GenericAdminChart'

const fullScreenModalStyle = {
  position: 'absolute',
  top: '0%',
  left: '-50%',
  width: '100%',
  height: '60%',
  overflow: 'auto',
  bgcolor: 'background.paper',
  boxShadow: 24,
}

export const modalButtonLocation = {
  display: 'flex',
  justifyContent: 'flex-end',
  alignItems: 'flex-end',
  paddingTop: '25px',
  width: '100%',
}
const style = {
  position: 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
}
interface GenericChartProps {
  headers: GridColDef[]
  data: any
  pageName: string
  baseRowType: any
  urlPath: string
  hasEditPrivileges: boolean
  hasDeletePrivileges: boolean
  roles?: RolesResponse
  protectedItems?: string[]
  onDelete(data: any): void
  onEdit(data: any): void
  onCreate(data: any): void
}

interface EditToolbarProps {
  baseRowType: any
  setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void
  setRowModesModel: (
    newModel: (oldModel: GridRowModesModel) => GridRowModesModel
  ) => void
}

function EditToolbar(props: EditToolbarProps) {
  const { baseRowType, setRows, setRowModesModel } = props

  return (
    <GridToolbarContainer>
      <GridToolbarColumnsButton />
      <GridToolbarFilterButton />
      <GridToolbarExport />
    </GridToolbarContainer>
  )
}

function RouteChart({
  headers,
  data,
  pageName,
  baseRowType,
  urlPath,
  onDelete,
  onEdit,
  onCreate,
  roles,
  protectedItems,
  hasEditPrivileges,
  hasDeletePrivileges,
}: GenericChartProps) {
  const [rows, setRows] = useState(data)
  const [rowModesModel, setRowModesModel] = useState<GridRowModesModel>({})
  const [open, setOpen] = useState(false)
  const [id, setId] = useState(-1)
  const theme = useTheme()
  const mode = theme.palette.mode
  const apiRef = useGridApiRef()
  const singularPageName = pageName.endsWith('s')
    ? pageName.slice(0, -1).charAt(0).toUpperCase() + pageName.slice(1, -1)
    : pageName

  const handleClose = () => setOpen(false)

  const handleClick = () => {
    const id = Math.max(...(apiRef.current.getAllRowIds() as number[])) + 1
    const keys = Object.keys(baseRowType)
    setRows((oldRows) => [{ ...baseRowType, id, isNew: true }, ...oldRows])
    setRowModesModel((oldModel) => ({
      ...oldModel,
      [id]: { mode: GridRowModes.Edit, fieldToFocus: keys[0] },
    }))
  }

  const handleRowEditStop: GridEventListener<'rowEditStop'> = (
    params,
    event
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true
    }
  }

  const handleEditClick = (id: GridRowId) => () => {
    navigateToPage(`${urlPath}/${id}/edit`)
  }

  const handleSaveClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View },
    })
  }

  const handleDeleteClick = (id: GridRowId) => {
    const itemToDelete = rows.find((row: any) => row.id === (id as number))
    if (
      itemToDelete &&
      (!protectedItems || !protectedItems.includes(itemToDelete.role))
    ) {
      setOpen(false)
      setRows(rows.filter((row: any) => row.id !== (id as number)))
      setId(-1)
      onDelete(itemToDelete)
    } else {
      alert(
        `The role "${itemToDelete.role}"Item cannot be deleted as it is protected by the developers of ATSPM.`
      )
      setOpen(false)
    }
  }

  const handleCancelClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View, ignoreModifications: true },
    })

    const editedRow = rows.find((row: any) => row.id === id)
    if (editedRow?.isNew) {
      setRows(rows.filter((row: any) => row.id !== id))
    }
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
      width: 70,
      cellClassName: 'edit-action',
      getActions: ({ id }) => {
        // const apiRef = useGridApiContext()
        const isInEditMode = rowModesModel[id]?.mode === GridRowModes.Edit

        if (isInEditMode) {
          return [
            <GridActionsCellItem
              icon={<SaveIcon />}
              label="Save"
              sx={{
                color: 'primary.main',
              }}
              onClick={handleSaveClick(id)}
              key={`s${id}`}
            />,
            <GridActionsCellItem
              icon={<CancelIcon />}
              label="Cancel"
              className="textPrimary"
              onClick={handleCancelClick(id)}
              color="inherit"
              key={`c${id}`}
            />,
          ]
        }

        return [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Edit"
            className="textPrimary"
            onClick={handleEditClick(id)}
            color="inherit"
            key={`e${id}`}
            disabled={!hasEditPrivileges}
          />,
        ]
      },
    },
    {
      field: 'delete',
      type: 'actions',
      headerName: 'Delete',
      width: 70,
      cellClassName: 'delete-action',
      getActions: ({ id, row }) => {
        const isProtected =
          (protectedItems && protectedItems.includes(row.role)) ||
          !hasDeletePrivileges

        return [
          <GridActionsCellItem
            icon={<DeleteIcon color={isProtected ? 'disabled' : 'error'} />}
            label="Delete"
            onClick={() => {
              if (!isProtected) {
                setOpen(true)
                setId(id as number)
              }
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
      <Box display="flex" justifyContent="flex-end" alignItems="center">
        <Button
          variant="contained"
          color="success"
          startIcon={<AddIcon />}
          onClick={handleClick}
          sx={{ marginBottom: 1 }}
        >
          Add {pageName}
        </Button>
      </Box>
      <Paper>
        <DataGrid
          rows={rows}
          apiRef={apiRef}
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
              top: '36px',
              backgroundColor: mode === 'light' ? 'white' : '#1F2A40',
              zIndex: '1',
            },
            [`& .${gridClasses.toolbarContainer}`]: {
              position: 'sticky',
              top: '0',
              backgroundColor: mode === 'light' ? 'white' : '#1F2A40',
              zIndex: '1',
            },
            [`& .${gridClasses.main}`]: {
              overflow: 'inherit',
            },
          }}
        />
      </Paper>
      <Modal key={`M${id}`} open={open} onClose={handleClose}>
        <Box sx={modalStyle}>
          <Typography sx={{ fontWeight: 'bold' }}>
            Delete {singularPageName}
          </Typography>

          <Divider sx={{ margin: '10px 0', backgroundColor: 'gray' }} />
          <Typography>
            Are you sure you want to delete this {singularPageName}?
          </Typography>
          <br />
          <Typography sx={{ fontWeight: 'bold' }}>
            {data[id - 1]?.name}
          </Typography>
          <Box sx={modalButtonLocation}>
            <Button onClick={handleClose}>No</Button>
            <Button
              onClick={() => handleDeleteClick(id)}
              style={{ color: 'red' }}
            >
              Delete {singularPageName}
            </Button>
          </Box>
        </Box>
      </Modal>
    </>
  )
}

export default RouteChart
