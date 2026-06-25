import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material'

interface DeleteModalProps<T> {
  id: number
  name: string
  objectType: string
  open: boolean
  onClose: () => void
  onConfirm: (keyValue: string | number) => void
  deleteLabel?: (selectedRow: T) => string
  selectedRow: T
  associatedObjects?: { id: number; name: string }[]
  associatedObjectsLabel?: string
  filterFunction?: (
    id: number,
    associatedObjects: { id: number; name: string }[]
  ) => { id: number; name: string }[]
  deleteByKey?: keyof T
}

const DeleteModal = <T,>({
  id,
  name,
  objectType,
  open,
  onClose,
  onConfirm,
  deleteLabel,
  selectedRow,
  associatedObjects = [],
  associatedObjectsLabel,
  filterFunction,
  deleteByKey, // NEW PROP
}: DeleteModalProps<T>) => {
  const handleConfirm = () => {
    const keyValue = deleteByKey ? selectedRow[deleteByKey] : id
    onConfirm(keyValue as string | number)
    onClose()
  }

  const filteredAssociatedObjects = filterFunction
    ? filterFunction(id, associatedObjects)
    : associatedObjects.filter((obj) => obj.id === id)

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle id="delete-dialog-title">Delete {objectType}?</DialogTitle>
      <DialogContent>
        {filteredAssociatedObjects?.length > 0 && (
          <>
            <Typography>
              <b>{name}</b> is associated with the following{' '}
              {associatedObjectsLabel || 'items'}:
            </Typography>
            <Box sx={{ maxHeight: '300px', overflowY: 'auto' }}>
              <List dense>
                {filteredAssociatedObjects.map((associatedObject) => (
                  <ListItem key={associatedObject.id}>
                    <ListItemText>{associatedObject.name}</ListItemText>
                  </ListItem>
                ))}
              </List>
            </Box>
          </>
        )}
        <Typography>
          Are you sure you want to delete{' '}
          <b>{deleteLabel ? deleteLabel(selectedRow) : name}</b>?
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="outlined">
          Cancel
        </Button>
        <Button onClick={handleConfirm} variant="contained" color="error">
          Delete {objectType}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default DeleteModal
