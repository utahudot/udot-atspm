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
import React, { ReactElement, cloneElement, useState } from 'react'

interface HasId {
  id: number
  name: string
}

interface DeleteModalProps {
  id: number
  name: string
  open: boolean
  onClose: () => void
}
interface EditModalProps<T> {
  id: number
  data: T | null
  open: boolean
  onClose: () => void
}

interface CreateModalProps {
  open: boolean
  onClose: () => void
}

interface AdminChartProps<T extends HasId> {
  headers: string[]
  headerKeys: string[]
  data: T[]
  pageName: string
  hasEditPrivileges: boolean
  hasDeletePrivileges: boolean
  editModal: ReactElement<EditModalProps<T>>
  deleteModal: ReactElement<DeleteModalProps>
  createModal: ReactElement<CreateModalProps>
}

type Order = 'asc' | 'desc'

const AdminTable = <T extends HasId>({
  headers,
  headerKeys,
  data,
  pageName,
  hasEditPrivileges,
  hasDeletePrivileges,
  editModal,
  deleteModal,
  createModal,
}: AdminChartProps<T>) => {
  const theme = useTheme()
  const { isSidebarOpen } = useSidebarStore()
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [selectedRow, setSelectedRow] = useState<T | null>(null)
  const [order, setOrder] = useState<Order>('asc')
  const [orderBy, setOrderBy] = useState<string>('id')
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
    setIsEditModalOpen(true)
  }

  const handleDeleteClick = () => {
    setIsDeleteModalOpen(true)
  }

  const handleRequestSort = (property: string) => {
    const isAsc = orderBy === property && order === 'asc'
    setOrder(isAsc ? 'desc' : 'asc')
    setOrderBy(property)
  }

  const sortedData = data.sort((a, b) => {
    let aValue = a[orderBy as keyof T] as string | number
    let bValue = b[orderBy as keyof T] as string | number

    if (typeof aValue === 'string' && typeof bValue === 'string') {
      aValue = aValue.toLowerCase()
      bValue = bValue.toLowerCase()
    }

    if (orderBy === '') return 0
    if (aValue < bValue) return order === 'asc' ? -1 : 1
    if (aValue > bValue) return order === 'asc' ? 1 : -1
    return 0
  })

  const deleteModalWithId = cloneElement(deleteModal, {
    id: selectedRow?.id,
    name: selectedRow?.name,
    open: isDeleteModalOpen,
    onClose: handleClose,
  })

  const editModalWithId = cloneElement(editModal, {
    id: selectedRow?.id,
    data: selectedRow,
    open: isEditModalOpen,
    onClose: handleClose,
  })

  const createModalWithProps = cloneElement(createModal, {
    open: isCreateModalOpen,
    onClose: handleClose,
  })

  console.log('Headers:', headers)
  console.log('Data:', data)

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
        <Button
          variant="contained"
          color="success"
          startIcon={<AddIcon />}
          sx={{ mb: 3 }}
          onClick={() => setIsCreateModalOpen(true)}
        >
          New {pageName}
        </Button>
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
                  {headerKeys.map((header) => (
                    <TableCell key={header}>
                      {row[header as keyof T] as string | number}
                    </TableCell>
                  ))}
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
        {hasDeletePrivileges && (
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
