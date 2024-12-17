import TextEditor from '@/components/TextEditor/JoditTextEditor'
import { zodResolver } from '@hookform/resolvers/zod'
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
import { SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  header: z.string().min(1, { message: 'Name is required' }),
  body: z.string().optional(),
  displayOrder: z.number().optional(),
})

type FormData = z.infer<typeof schema>

interface ModalProps {
  data?: Faq
  isOpen: boolean
  onClose: () => void
  onSave: (faq: Faq) => void
}


const FaqEditorModal: React.FC<ModalProps> = ({
  data: faq,
  isOpen,
  onClose,
  onSave,
}) => {
  const [createOrEditText, setCreateOrEditText] = useState('')

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      header: faq?.header || '',
      body: faq?.body || '',
      displayOrder: faq?.displayOrder || 0,
    },
  })

  useEffect(()=>{
    if (faq?.id !== undefined) {
      setCreateOrEditText('Edit');
    } else {
      setCreateOrEditText('Create');
    }
  },[faq])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedfaq = { ...faq, ...data } as Faq
    onSave(updatedfaq)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} key={isOpen ? 'open' : 'closed'} maxWidth="md" fullWidth>
      <DialogTitle id="form-dialog-title">{createOrEditText} FAQ</DialogTitle>

      <DialogContent>
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'row',
            gap: 2,
            marginBottom: 2,
          }}
        >
          <FormControl fullWidth margin="normal" sx={{ flex: 7 }} error={!!errors.header}>
            <InputLabel htmlFor="faq-header">Header</InputLabel>
            <OutlinedInput
              id="faq-header"
              {...register('header')}
              label="Header"
              error={!!errors.header}
            />
            {errors.header && <span>{errors.header.message}</span>}
          </FormControl>
          <FormControl fullWidth margin="normal" sx={{ flex: 1 }} error={!!errors.displayOrder}>
            <InputLabel htmlFor="faq-display-order">Display Order</InputLabel>
            <OutlinedInput
              id="faq-display-order"
              {...register('displayOrder', { valueAsNumber: true })}
              label="Display Order"
              type="number"
              inputProps={{ style: { textAlign: 'center' } }}
            />
            {errors.displayOrder && <span>{errors.displayOrder.message}</span>}
          </FormControl>
        </Box>
        <TextEditor 
          data={faq?.body || ''} 
          onChange={(value: string) => setValue('body', value)}
        />
      </DialogContent>
      <DialogActions>
        <Box sx={{ marginRight: '1rem', marginBottom: '.5rem' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit(onSubmit)}>
            {faq?.id ? 'Edit FAQ' : 'Create FAQ'}
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  )
}

export default FaqEditorModal