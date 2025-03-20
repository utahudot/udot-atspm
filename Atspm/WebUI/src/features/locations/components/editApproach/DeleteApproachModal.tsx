import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material'

interface DeleteApproachModalProps {
  openModal: boolean
  setOpenModal: (open: boolean) => void
  confirmDeleteApproach: () => void
}

const DeleteApproachModal = ({
  openModal,
  setOpenModal,
  confirmDeleteApproach,
}: DeleteApproachModalProps) => {
  return (
    <Dialog open={openModal} onClose={() => setOpenModal(false)}>
      <DialogTitle sx={{ fontWeight: 'bold' }} id="delete-confirmation">
        Delete Approach
      </DialogTitle>
      <DialogContent>
        <Typography id="confirm-delete-approach">
          Are you sure you want to delete this approach?
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => setOpenModal(false)} variant="outlined">
          Cancel
        </Button>
        <Button
          onClick={confirmDeleteApproach}
          color="error"
          variant="contained"
        >
          Delete Approach
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default DeleteApproachModal
