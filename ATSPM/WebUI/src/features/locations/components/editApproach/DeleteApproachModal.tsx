import { Box, Button, Modal, Typography } from '@mui/material'

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
    <Modal
      open={openModal}
      onClose={() => setOpenModal(false)}
      aria-labelledby="delete-confirmation"
      aria-describedby="confirm-delete-approach"
    >
      <Box
        sx={{
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
        }}
      >
        <Typography id="delete-confirmation" sx={{ fontWeight: 'bold' }}>
          Confirm Delete
        </Typography>
        <Typography id="confirm-delete-approach" sx={{ mt: 2 }}>
          Are you sure you want to delete this approach?
        </Typography>
        <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
          <Button onClick={() => setOpenModal(false)} color="inherit">
            Cancel
          </Button>
          <Button
            onClick={confirmDeleteApproach}
            color="error"
            variant="contained"
          >
            Delete Approach
          </Button>
        </Box>
      </Box>
    </Modal>
  )
}

export default DeleteApproachModal
