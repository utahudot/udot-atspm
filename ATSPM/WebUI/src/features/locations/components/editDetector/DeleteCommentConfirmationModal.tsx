import { Box, Button, Divider, Modal, Typography } from '@mui/material'

interface DeleteConfirmationModalProps {
  open: boolean
  onClose: () => void
  onDelete: () => void
  commentText: string
}

const DeleteCommentConfirmationModal = ({
  open,
  onClose,
  onDelete,
  commentText,
}: DeleteConfirmationModalProps): JSX.Element => {
  const modalStyle = {
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: 400,
    bgcolor: 'background.paper',
    boxShadow: 24,
    p: 4,
  }

  const modalButtonLocation = {
    mt: 2,
    display: 'flex',
    justifyContent: 'space-between',
  }

  return (
    <Modal open={open} onClose={onClose}>
      <Box sx={modalStyle}>
        <Typography sx={{ fontWeight: 'bold' }}>Delete Comment</Typography>
        <Divider sx={{ margin: '10px 0', backgroundColor: 'gray' }} />
        <Typography>Are you sure you want to delete this comment?</Typography>
        <Typography sx={{ fontWeight: 'bold', mt: 2 }}>
          {commentText}
        </Typography>
        <Box sx={modalButtonLocation}>
          <Button onClick={onClose}>Cancel</Button>
          <Button onClick={onDelete} style={{ color: 'red' }}>
            Delete
          </Button>
        </Box>
      </Box>
    </Modal>
  )
}

export default DeleteCommentConfirmationModal
