import { Location } from '@/features/locations/types'
import { useSidebarStore } from '@/stores/sidebar'
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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
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
import React, { useState } from 'react'
import RoleModal from './RoleModal'
import UserModal from './UserModal'

export const modalStyle = {
  position: 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: 'none',
  borderRadius: '10px',
  boxShadow: 24,
  p: 4,
  '@media (max-width: 400px)': {
    width: '100%',
  },
}

export const modalButtonLocation = {
  display: 'flex',
  justifyContent: 'flex-end',
  alignItems: 'flex-end',
  paddingTop: '25px',
  width: '100%',
}
interface GenericChartProps {
  headers: GridColDef[]
  data: any
  pageName: any
  baseRowType: any
  onDelete(data: any): void
  onEdit(data: any): void
  onCreate(data: any): void
  customModal?: React.ReactNode
  protectedItems?: string[]
  locations?: Location[]
  hasEditPrivileges: boolean
  hasDeletePrivileges: boolean
}
interface EditToolbarProps {
  baseRowType: string
  setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void
  setRowModesModel: (
    newModel: (oldModel: GridRowModesModel) => GridRowModesModel
  ) => void
}

function EditToolbar(props: EditToolbarProps) {
  ;<GridToolbarContainer>
    <GridToolbarColumnsButton />
    <GridToolbarFilterButton />
    <GridToolbarExport />
  </GridToolbarContainer>
}

function GenericAdminChart({
  headers,
  data,
  pageName,
  baseRowType,
  onDelete,
  onEdit,
  onCreate,
  customModal,
  protectedItems,
  locations,
  hasEditPrivileges,
  hasDeletePrivileges,
}: GenericChartProps) {
  const [rows, setRows] = useState(data)
  const [rowModesModel, setRowModesModel] = useState<GridRowModesModel>({})
  const [open, setOpen] = useState(false)
  const [id, setId] = useState(-1)
  const [modalOpen, setModalOpen] = useState(false)
  const [modalData, setModalData] = useState(null)
  const theme = useTheme()
  const mode = theme.palette.mode
  const apiRef = useGridApiRef()
  const { isSidebarOpen } = useSidebarStore()
  const singularPageName = pageName.endsWith('s')
    ? pageName.slice(0, -1).charAt(0).toUpperCase() + pageName.slice(1, -1)
    : pageName
  const handleClose = () => setOpen(false)

  const handleClick = () => {
    const id = Math.max(...(apiRef.current.getAllRowIds() as number[])) + 1
    const keys = Object.keys(baseRowType)
    if (customModal && customModal?.type !== RoleModal) {
      setModalOpen(true)
      setModalData(baseRowType)
    } else {
      setRows((oldRows) => [{ ...baseRowType, id, isNew: true }, ...oldRows])
      setRowModesModel((oldModel) => ({
        ...oldModel,
        [id]: { mode: GridRowModes.Edit, fieldToFocus: keys[0] },
      }))
    }
  }
  const handleRowEditStop: GridEventListener<'rowEditStop'> = (
    params,
    event
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true
    }
  }

  const handleModalClose = () => {
    setModalOpen(false)
    setModalData(null)
  }

  const handleEditClick = (id: GridRowId) => () => {
    if (customModal) {
      const rowData = rows.find((row: any) => row.id === id)
      setModalOpen(true)
      setModalData(rowData)
    } else {
      setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.Edit } })
    }
  }

  const handleSaveClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View },
    })
  }
  const handleDeleteClick = (id: GridRowId) => {
    setOpen(false)
    setRows(rows.filter((row: any) => row.id !== (id as number)))
    setId(-1)
    onDelete(rows.find((row: any) => row.id === (id as number)))
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

  const handleModalSave = (row: any) => {
    if (customModal) {
      if (row.id) {
        setRows((prevRows) =>
          prevRows.map((newRow: any) => (newRow.id === row.id ? row : newRow))
        )
      } else {
        const newId = Math.max(...rows.map((newRow: any) => newRow.id), 0) + 1
        const newRow = { ...row, id: newId }
        setRows((prevRows) => [newRow, ...prevRows])
      }
    }
    handleModalClose()
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
  const listOfAssociatedLocations = (() => {
    switch (pageName) {
      case 'Areas':
        return (
          locations?.filter((location) => location.areas.includes(id)) || []
        )
      case 'Regions':
        return locations?.filter((location) => location.regionId === id) || []
      case 'Jurisdictions':
        return (
          locations?.filter((location) => location.jurisdictionId === id) || []
        )
      case 'Device Configurations':
        return (
          locations?.filter((device) => device.deviceConfigurationId === id) ||
          []
        )
      default:
        return []
    }
  })()

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        minWidth: '375px',
        padding: '1px',
        [theme.breakpoints.up('md')]: {
          maxWidth: isSidebarOpen ? 'calc(100vw - 340px)' : '100%',
        },
      }}
    >
      <Box display="flex" justifyContent="flex-end" alignItems="center">
        {customModal?.type !== UserModal && (
          <Button
            variant="contained"
            color="success"
            startIcon={<AddIcon />}
            onClick={handleClick}
            sx={{ marginBottom: 1 }}
          >
            Add {pageName}
          </Button>
        )}
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
              top: '0px',
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
          {listOfAssociatedLocations.length > 0 &&
          pageName == 'Device Configurations' ? (
            <>
              <Typography sx={{ mt: 2 }}>
                The following locations are configued with this device
                configuration. Are you sure you want to delete it?
              </Typography>
              <Box
                sx={{
                  maxHeight: '300px',
                  overflowY: 'auto',
                  marginTop: '10px',
                }}
              >
                <TableContainer>
                  <Table stickyHeader>
                    <TableBody>
                      {listOfAssociatedLocations.map((item) => (
                        <TableRow key={item.id}>
                          <TableCell component="th" scope="row">
                            {item.location.primaryName} &{' '}
                            {item.location.secondaryName}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
              <Box sx={modalButtonLocation}>
                <Button onClick={handleClose}>No</Button>
                <Button
                  onClick={() => handleDeleteClick(id)}
                  style={{ color: 'red' }}
                >
                  Delete {singularPageName}
                </Button>
              </Box>
            </>
          ) : null}

          {listOfAssociatedLocations.length > 0 &&
          pageName != 'Device Configurations' ? (
            <>
              <Typography sx={{ mt: 2 }}>
                The following locations are associated with this{' '}
                {singularPageName}. Please reassign them before deleting this{' '}
                {singularPageName}:
              </Typography>
              <Box
                sx={{
                  maxHeight: '300px',
                  overflowY: 'auto',
                  marginTop: '10px',
                }}
              >
                <TableContainer>
                  <Table stickyHeader>
                    <TableBody>
                      {listOfAssociatedLocations.map((location) => (
                        <TableRow key={location.id}>
                          <TableCell component="th" scope="row">
                            {location.primaryName} & {location.secondaryName}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            </>
          ) : null}
          {listOfAssociatedLocations.length < 1 ? (
            <>
              <Divider sx={{ margin: '10px 0', backgroundColor: 'gray' }} />
              <Typography>
                Are you sure you want to delete this {singularPageName}?
              </Typography>
              <br />
              <Typography sx={{ fontWeight: 'bold' }}>
                {data[id - 1]?.header}
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
            </>
          ) : null}
        </Box>
      </Modal>

      {customModal && (
        <Modal open={modalOpen} onClose={handleModalClose}>
          {React.cloneElement(customModal as React.ReactElement, {
            open: modalOpen,
            onClose: handleModalClose,
            data: modalData,
            onSave: handleModalSave,
          })}
        </Modal>
      )}
    </Box>
  )
}

export default GenericAdminChart
