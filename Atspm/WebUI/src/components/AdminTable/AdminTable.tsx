import { useSidebarStore } from '@/stores/sidebar'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import {
  Box,
  Button,
  IconButton,
  ListItemIcon,
  Menu,
  MenuItem,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Typography,
  useTheme,
} from '@mui/material'
import type { ReactElement } from 'react'
import React, { cloneElement, useMemo, useState } from 'react'

interface HasId {
  id: number
  name: string
}

interface DeleteModalProps<T> {
  id: number
  name: string
  selectedRow: T
  open: boolean
  onClose?: () => void
}
interface EditModalProps<T> {
  id: number
  data: T | null
  open: boolean
  onClose?: () => void
}

interface CreateModalProps {
  open: boolean
  onClose?: () => void
}

interface CustomCellConfig<T> {
  headerKey: keyof T
  component: (value: T[keyof T], row: T) => React.ReactNode
}

interface AdminChartProps<T extends HasId> {
  headers: string[]
  headerKeys: (keyof T)[]
  data: T[]
  pageName: string
  hasEditPrivileges?: boolean
  hasDeletePrivileges?: boolean
  protectedFromDeleteItems?: string[]
  customEditFunction?: (selectedRow: T | null) => void
  editModal?: ReactElement<EditModalProps<T>>
  deleteModal?: ReactElement<DeleteModalProps<T>>
  createModal?: ReactElement<CreateModalProps>
  customCellRender?: CustomCellConfig<T>[]
}

type Order = 'asc' | 'desc'

const AdminTable = <T extends HasId>({
  headers,
  headerKeys,
  data,
  pageName,
  hasEditPrivileges,
  hasDeletePrivileges,
  protectedFromDeleteItems,
  customEditFunction,
  editModal,
  deleteModal,
  createModal,
  customCellRender,
}: AdminChartProps<T>) => {
  const theme = useTheme()
  const { isSidebarOpen } = useSidebarStore()
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [selectedRow, setSelectedRow] = useState<T | null>(null)
  const [order, setOrder] = useState<Order>('asc')
  const [orderBy, setOrderBy] = useState<keyof T>('id')
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false)

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>, item: T) => {
    setAnchorEl(event.currentTarget)
    setSelectedRow(item)
  }

  const handleClose = () => {
    setAnchorEl(null)
    setSelectedRow(null)
    setIsDeleteModalOpen(false)
    setIsEditModalOpen(false)
    setIsCreateModalOpen(false)
  }

  const handleEditClick = () => {
    if (customEditFunction) {
      customEditFunction(selectedRow)
    } else {
      setIsEditModalOpen(true)
    }
  }

  const handleDeleteClick = () => {
    setIsDeleteModalOpen(true)
  }

  const handleRequestSort = (property: keyof T) => {
    const isAsc = orderBy === property && order === 'asc'
    setOrder(isAsc ? 'desc' : 'asc')
    setOrderBy(property)
  }

  const sortedData = useMemo(
    () =>
      [...data].sort((a, b) => {
        let aValue = a[orderBy] as string | number
        let bValue = b[orderBy] as string | number

        if (typeof aValue === 'string' && typeof bValue === 'string') {
          aValue = aValue.toLowerCase()
          bValue = bValue.toLowerCase()
        }

        if (orderBy === '') return 0
        if (aValue < bValue) return order === 'asc' ? -1 : 1
        if (aValue > bValue) return order === 'asc' ? 1 : -1
        return 0
      }),
    [data, order, orderBy]
  )

  let deleteModalWithId
  if (deleteModal) {
    deleteModalWithId = cloneElement(deleteModal, {
      id: selectedRow?.id,
      name: selectedRow?.name,
      open: isDeleteModalOpen,
      selectedRow: selectedRow ?? undefined,
      onClose: handleClose,
    })
  }
  let editModalWithId
  if (editModal) {
    editModalWithId = cloneElement(editModal, {
      id: selectedRow?.id,
      data: selectedRow,
      open: isEditModalOpen,
      onClose: handleClose,
    })
  }

  let createModalWithProps

  if (createModal) {
    createModalWithProps = cloneElement(createModal, {
      open: isCreateModalOpen,
      onClose: handleClose,
    })
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        minWidth: '375px',
        marginTop: -4,
        [theme.breakpoints.up('md')]: {
          maxWidth: isSidebarOpen ? 'calc(100vw - 340px)' : '100%',
        },
      }}
    >
      <Box display="flex" justifyContent="flex-end" alignItems="center">
        {createModal && (
          <Button
            variant="contained"
            color="success"
            startIcon={<AddIcon />}
            sx={{ mb: 3 }}
            onClick={() => setIsCreateModalOpen(true)}
          >
            New {pageName}
          </Button>
        )}

        {!createModal && <Box sx={{ mb: 7.5 }}></Box>}
      </Box>

      <Box>
        <TableContainer
          component={Paper}
          sx={{ maxHeight: 'calc(100vh - 190px)' }}
        >
          <Table stickyHeader size="small">
            <TableHead>
              <TableRow>
                {headers.map((header) => (
                  <TableCell
                    key={header}
                    sortDirection={orderBy === header ? order : false}
                    sx={{ backgroundColor: '#dae5f0' }}
                  >
                    <TableSortLabel
                      active={orderBy === header}
                      direction={orderBy === header ? order : 'asc'}
                      onClick={() => handleRequestSort(header)}
                    >
                      {header}
                    </TableSortLabel>
                  </TableCell>
                ))}
                {(hasEditPrivileges || hasDeletePrivileges) && (
                  <TableCell
                    sx={{
                      width: 100,
                      textAlign: 'right',
                      position: 'sticky',
                      top: 0,
                      backgroundColor: '#dae5f0',
                    }}
                  >
                    Actions
                  </TableCell>
                )}
              </TableRow>
            </TableHead>
            <TableBody>
              {sortedData.map((row) => (
                <TableRow hover key={row.id}>
                  {headerKeys.map((header) => {
                    const customRenderer = customCellRender?.find(
                      (config) => config.headerKey === header
                    )

                    return (
                      <TableCell
                        key={
                          typeof header === 'symbol'
                            ? header.toString()
                            : header
                        }
                        sx={{ height: '53px' }}
                      >
                        {customRenderer
                          ? customRenderer.component(row[header], row)
                          : (row[header] as string | number)}
                      </TableCell>
                    )
                  })}
                  {(hasEditPrivileges || hasDeletePrivileges) && (
                    <TableCell sx={{ width: 100, textAlign: 'right' }}>
                      <IconButton
                        aria-label="more"
                        aria-controls="actions-menu"
                        aria-haspopup="true"
                        onClick={(event) => handleMenuClick(event, row)}
                      >
                        <MoreVertIcon />
                      </IconButton>
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>

      <Menu
        id="actions-menu"
        anchorEl={anchorEl}
        keepMounted
        open={Boolean(anchorEl)}
        onClose={handleClose}
      >
        {hasEditPrivileges && (
          <MenuItem onClick={handleEditClick}>
            <ListItemIcon>
              <EditIcon />
            </ListItemIcon>
            <Typography>Edit</Typography>
          </MenuItem>
        )}
        {hasDeletePrivileges &&
          selectedRow &&
          !protectedFromDeleteItems?.includes(
            selectedRow[headerKeys[0]] as string
          ) && (
            <MenuItem onClick={handleDeleteClick}>
              <ListItemIcon>
                <DeleteIcon />
              </ListItemIcon>
              <Typography>Delete</Typography>
            </MenuItem>
          )}
      </Menu>

      {isDeleteModalOpen && deleteModalWithId}
      {isEditModalOpen && editModalWithId}
      {isCreateModalOpen && createModalWithProps}
    </Box>
  )
}

export default AdminTable
