import {
  useCreateDetectorComment,
  useDeleteDetectorComment,
  useGetDetectorComments,
  useUpdateDetectorComment,
} from '@/features/locations/api/detector'
import { Detector } from '@/features/locations/types'
import ChatBubbleIcon from '@mui/icons-material/ChatBubble'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import {
  Badge,
  Box,
  Button,
  Divider,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Modal,
  Popover,
  TableCell,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'
import DeleteConfirmationModal from './DeleteCommentConfirmationModal'

interface CommentCellProps {
  detector: Detector
}

const CommentCell = ({ detector }: CommentCellProps) => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const [modalOpen, setModalOpen] = useState(false)
  const [deleteModalOpen, setDeleteModalOpen] = useState(false)
  const [editCommentId, setEditCommentId] = useState<string | null>(null)
  const [selectedComment, setSelectedComment] = useState<string | null>(null)
  const [commentText, setCommentText] = useState('')

  const { refetch, data: commentsData } = useGetDetectorComments(
    detector?.id?.toString()
  )
  const { mutate: addComment } = useCreateDetectorComment()
  const { mutate: deleteComment } = useDeleteDetectorComment()
  const { mutate: updateComment } = useUpdateDetectorComment()

  if (!commentsData?.value) return null

  const comments = commentsData.value
    .filter((comment) => comment.detectorId === detector.id)
    .sort(
      (a, b) =>
        new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime()
    )

  const handleOpenPopover = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClosePopover = () => {
    setAnchorEl(null)
  }

  const handleOpenModal = (
    commentId: string | null = null,
    currentText = ''
  ) => {
    setEditCommentId(commentId)
    setCommentText(currentText)
    setModalOpen(true)
  }

  const handleCloseModal = () => {
    setModalOpen(false)
    setEditCommentId(null)
  }

  const handleSaveComment = () => {
    if (editCommentId) {
      updateComment(
        { id: editCommentId, data: { comment: commentText } },
        { onSuccess: refetch }
      )
    } else {
      addComment(
        {
          comment: commentText,
          detectorId: detector.id,
          timeStamp: new Date().toISOString(),
        },
        { onSuccess: refetch }
      )
    }
    setCommentText('')
    handleCloseModal()
  }

  const handleOpenDeleteModal = (commentId: string) => {
    setSelectedComment(commentId)
    setDeleteModalOpen(true)
  }

  const handleCloseDeleteModal = () => {
    setDeleteModalOpen(false)
    setSelectedComment(null)
  }

  const handleConfirmDelete = () => {
    if (selectedComment) {
      deleteComment(selectedComment, { onSuccess: refetch })
    }
    handleCloseDeleteModal()
  }

  return (
    <TableCell>
      {detector.id ? (
        <IconButton onClick={handleOpenPopover}>
          <Badge badgeContent={comments.length} color="primary">
            <ChatBubbleIcon />
          </Badge>
        </IconButton>
      ) : (
        <Tooltip title="Save detector to add comments">
          <span>
            <IconButton disabled>
              <Badge badgeContent={comments.length} color="primary">
                <ChatBubbleIcon />
              </Badge>
            </IconButton>
          </span>
        </Tooltip>
      )}
      <Popover
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        onClose={handleClosePopover}
        sx={{ mt: 2, overflow: 'visible' }}
      >
        <Box p={2} sx={{ width: 400 }}>
          <List sx={{ width: '100%' }}>
            {comments.length === 0 && (
              <ListItem>
                <ListItemText primary="No comments" />
              </ListItem>
            )}
            {comments.map((comment) => (
              <ListItem
                key={comment.id}
                divider
                sx={{ alignItems: 'flex-start', width: '100%' }}
              >
                <ListItemText
                  primary={comment.comment}
                  secondary={
                    <>
                      {new Date(comment.timeStamp).toLocaleString()}
                      <Box
                        sx={{
                          display: 'flex',
                          mt: 1,
                          justifyContent: 'flex-end',
                          width: '100%',
                        }}
                      >
                        <IconButton
                          onClick={() =>
                            handleOpenModal(comment.id, comment.comment)
                          }
                          size="small"
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                        <IconButton
                          onClick={() => handleOpenDeleteModal(comment.id)}
                          size="small"
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Box>
                    </>
                  }
                />
              </ListItem>
            ))}
          </List>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2 }}>
            <Button
              variant="contained"
              color="primary"
              onClick={() => handleOpenModal(null)}
            >
              Add New
            </Button>
          </Box>
        </Box>
      </Popover>
      <Modal
        open={modalOpen}
        onClose={handleCloseModal}
        aria-labelledby="edit-comment-modal"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 400,
            bgcolor: 'background.paper',
            boxShadow: 24,
            p: 4,
          }}
        >
          <Typography sx={{ fontWeight: 'bold' }}>
            {editCommentId ? 'Edit Comment' : 'Add New Comment'}
          </Typography>
          <Divider sx={{ my: 2 }} />
          <TextField
            fullWidth
            multiline
            rows={4}
            value={commentText}
            onChange={(e) => setCommentText(e.target.value)}
            label="Comment"
            variant="outlined"
          />
          <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={handleCloseModal}>Cancel</Button>
            <Button
              onClick={handleSaveComment}
              style={{ marginLeft: '10px' }}
              color="primary"
              variant="contained"
            >
              Save Comment
            </Button>
          </Box>
        </Box>
      </Modal>
      <DeleteConfirmationModal
        open={deleteModalOpen}
        onClose={handleCloseDeleteModal}
        onDelete={handleConfirmDelete}
        commentText={
          comments.find((comment) => comment.id === selectedComment)?.comment ||
          ''
        }
      />
    </TableCell>
  )
}

export default CommentCell
