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

interface DeleteModalProps {
  id: number
  name: string
  objectType: string
  open: boolean
  onClose: () => void
  onConfirm: (id: number) => void
  associatedObjects?: { id: number; name: string }[]
  associatedObjectsLabel?: string
  filterFunction?: (
    id: number,
    associatedObjects: { id: number; name: string }[]
  ) => { id: number; name: string }[]
}

const DeleteModal = ({
  id,
  name,
  objectType,
  open,
  onClose,
  onConfirm,
  associatedObjects = [],
  associatedObjectsLabel,
  filterFunction,
}: DeleteModalProps) => {
  const handleConfirm = () => {
    onConfirm(id)
    onClose()
  }

  const filteredAssociatedObjects = filterFunction
    ? filterFunction(id, associatedObjects)
    : associatedObjects.filter((obj) => obj.id === id)

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle
        variant="h4"
        sx={{ fontStyle: 'bold' }}
        id="delete-dialog-title"
      >
        Delete {objectType}?
      </DialogTitle>
      <DialogContent>
        {filteredAssociatedObjects?.length > 0 && (
          <>
            <Typography sx={{ mb: 2 }}>
              <b>{name}</b> is associated with the following{' '}
              {associatedObjectsLabel || 'items'}:
            </Typography>
            <Box
              sx={{
                maxHeight: '300px',
                overflowY: 'auto',
                backgroundColor: 'background.default',
                outline: '1px solid #ccc',
              }}
            >
              <List dense={true}>
                {filteredAssociatedObjects.map((associatedObject) => (
                  <ListItem key={associatedObject.id}>
                    <ListItemText>{associatedObject.name}</ListItemText>
                  </ListItem>
                ))}
              </List>
            </Box>
          </>
        )}
        <Typography sx={{ mt: 2 }}>
          Are you sure you want to delete <b>{name}</b>?
        </Typography>
      </DialogContent>
      <DialogActions sx={{ p: 2 }}>
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
