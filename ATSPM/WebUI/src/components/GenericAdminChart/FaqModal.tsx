import TextEditor from '@/components/TextEditor/JoditTextEditor'

import { Faq } from '@/features/faq/types'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  OutlinedInput,
} from '@mui/material'
import { useEffect, useState } from 'react'

interface ModalProps {
  open: boolean
  onClose: () => void
  data: Faq | null
  onSave: (faq: Faq) => void
  onCreate: (faq: Faq) => void
  onEdit: (faq: Faq) => void
}

export const modalButtonLocation = {
  display: 'flex',
  justifyContent: 'flex-end',
  alignItems: 'flex-end',
  paddingTop: '25px',
  width: '100%',
}

const FaqModal: React.FC<ModalProps> = ({
  open,
  onClose,
  data,
  onCreate,
  onSave,
  onEdit,
}) => {
  const [faqId, setFaqId] = useState(data?.id || '')
  const [faqHeader, setFaqHeader] = useState(data?.header || '')
  const [faqBody, setFaqBody] = useState(data?.body || '')
  const [faqDisplayOrder, setFaqDisplayOrder] = useState(
    data?.displayOrder || 0
  )
  const [createOrEditText, setCreateOrEditText] = useState('')



  const handleSubmit = async () => {
    const dataGridRow = {
      id: faqId,
      header: faqHeader,
      body: faqBody,
      displayOrder: faqDisplayOrder,
    }

    try {
      if (data.id) {
        await onEdit(dataGridRow)
      } else {
        await onCreate(dataGridRow)
      }
      onSave(dataGridRow)
      onClose()
    } catch (error) {
      console.error('Error occurred while editing/creating data:', error)
    }
  }

useEffect(()=>{
  if (data?.id !== undefined) {
    setCreateOrEditText('Edit');
  } else {
    setCreateOrEditText('Create');
  }
},[data])

  return (
    <Dialog open={open} key={open ? 'open' : 'closed'} maxWidth="md" fullWidth>
      <DialogTitle sx={{ fontSize: '1.3rem', margin: '1rem' }} id="role-permissions-label">{createOrEditText} FAQ</DialogTitle>

      <DialogContent>
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'row',
            gap: 2,
            marginBottom: 2,
          }}
        >
          <FormControl fullWidth margin="normal" sx={{ flex: 7 }}>
            <InputLabel htmlFor="faq-header">Header</InputLabel>
            <OutlinedInput
              id="faq-header"
              value={faqHeader}
              onChange={(e) => setFaqHeader(e.target.value)}
              label="Header"
            />
          </FormControl>
          <FormControl fullWidth margin="normal" sx={{ flex: 1 }}>
            <InputLabel htmlFor="faq-display-order">Display Order</InputLabel>
            <OutlinedInput
              id="faq-display-order"
              value={faqDisplayOrder}
              onChange={(e) => setFaqDisplayOrder(e.target.value)}
              label="Display Order"
              type="number"
              inputProps={{ style: { textAlign: 'center' } }}
            />
          </FormControl>
        </Box>
        <TextEditor data={faqBody} onChange={setFaqBody} />
      </DialogContent>
      <DialogActions>
        <Box sx={{ marginRight: '1rem', marginBottom: '.5rem' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit}>
            {data?.id ? 'Edit FAQ' : 'Create FAQ'}
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  )
}

export default FaqModal
