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
  Toolbar,
  Typography,
  useTheme,
} from '@mui/material'
import type { ReactElement } from 'react'
import React, { cloneElement, useMemo, useState } from 'react'

interface BaseObj {
  id: number
  name: string
  created: string
  createdBy: string
  modified: string
  modifiedBy: string
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

interface Cell<T> {
  key: string
  label: string
  align?: 'right' | 'left' | 'center'
  component?: (value: T[keyof T], row: T) => React.ReactNode
  customSortFunction?: (a: T, b: T) => number
}

interface AdminChartProps<T extends BaseObj> {
  cells: Cell<T>[]
  data: T[]
  pageName: string
  hasEditPrivileges?: boolean
  hasDeletePrivileges?: boolean
  protectedFromDeleteItems?: string[]
  customEditFunction?: (selectedRow: T | null) => void
  editModal?: ReactElement<EditModalProps<T>>
  deleteModal?: ReactElement<DeleteModalProps<T>>
  createModal?: ReactElement<CreateModalProps>
  hideAuditProperties?: boolean
}

type Order = 'asc' | 'desc'

const AdminTable = <T extends BaseObj>({
  cells,
  data,
  pageName,
  hasEditPrivileges,
  hasDeletePrivileges,
  protectedFromDeleteItems,
  customEditFunction,
  editModal,
  deleteModal,
  createModal,
  hideAuditProperties,
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

  const AdminCell = ({ cell, width }: { cell: Cell<T>; width?: number }) => {
    const { key, label, align } = cell
    return (
      <TableCell
        sortDirection={orderBy === label ? order : false}
        align={align}
        width={width}
        sx={{
          backgroundColor: '#dae5f0',
        }}
      >
        <TableSortLabel
          active={orderBy === key}
          direction={orderBy === key ? order : 'asc'}
          onClick={() => handleRequestSort(key as keyof T)}
        >
          {label}
        </TableSortLabel>
      </TableCell>
    )
  }

  const sortedData = useMemo(
    () =>
      [...data].sort((a, b) => {
        let aValue = a[orderBy] as string | number | null
        let bValue = b[orderBy] as string | number | null

        if (typeof aValue === 'string' && typeof bValue === 'string') {
          aValue = aValue.toLowerCase()
          bValue = bValue.toLowerCase()
        }

        if (orderBy === '') return 0
        if (!aValue) return 1
        if (!bValue) return -1
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
      <Toolbar disableGutters>
        <Box
          sx={{ ml: 'auto', display: 'flex', alignItems: 'center', gap: 1.5 }}
        >
          {createModal && (
            <Button
              variant="contained"
              color="success"
              startIcon={<AddIcon />}
              onClick={() => setIsCreateModalOpen(true)}
            >
              New {pageName}
            </Button>
          )}
        </Box>
      </Toolbar>

      <Box>
        <TableContainer
          component={Paper}
          sx={{ maxHeight: 'calc(100vh - 190px)' }}
        >
          <Table stickyHeader size="small">
            <TableHead>
              <TableRow>
                {cells.map((cell) => (
                  <AdminCell key={cell.key} cell={cell} />
                ))}
                {!hideAuditProperties && (
                  <>
                    <AdminCell
                      cell={{ key: 'modified', label: 'Modified On' }}
                      width={150}
                    />
                    <AdminCell
                      cell={{ key: 'modifiedBy', label: 'Modified By' }}
                      width={150}
                    />
                    <AdminCell
                      cell={{ key: 'created', label: 'Created On' }}
                      width={140}
                    />
                    <AdminCell
                      cell={{ key: 'createdBy', label: 'Created By' }}
                      width={150}
                    />
                  </>
                )}
                {(hasEditPrivileges || hasDeletePrivileges) && (
                  <TableCell
                    align="right"
                    sx={{
                      width: 100,
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
                  {cells.map((cell) => {
                    const Component = cell.component
                    return (
                      <TableCell
                        key={cell.key}
                        align={cell?.align || 'left'}
                        sx={{
                          height: '53px',
                        }}
                      >
                        {Component ? (
                          <Component value={row[cell.key]} row={row} />
                        ) : (
                          (row[cell.key] as string | number)
                        )}
                      </TableCell>
                    )
                  })}
                  {!hideAuditProperties && (
                    <>
                      <TableCell>{row.modified}</TableCell>
                      <TableCell>{row.modifiedBy}</TableCell>
                      <TableCell>{row.created}</TableCell>
                      <TableCell>{row.createdBy}</TableCell>
                    </>
                  )}
                  {(hasEditPrivileges || hasDeletePrivileges) && (
                    <TableCell align="right" sx={{ width: 100 }}>
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
            selectedRow[cells[0].key] as string
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
