import { Product } from '@/api/config'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'
import { SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  manufacturer: z.string().min(1, { message: 'Manufacturer is required' }),
  model: z.string().min(1, { message: 'Model is required' }),
  webPage: z.string().optional(),
  notes: z.string().optional(),
})

type FormData = z.infer<typeof schema>

type ProductEditorModalProps = {
  data?: Product
  isOpen: boolean
  onClose: () => void
  onSave: (Product: Product) => void
}

const ProductEditorModal = ({
  data: product,
  isOpen,
  onClose,
  onSave,
}: ProductEditorModalProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      manufacturer: product?.manufacturer || '',
      model: product?.model || '',
      webPage: product?.webPage || '',
      notes: product?.notes || '',
    },
  })

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedproduct = { ...product, ...data } as Product
    onSave(updatedproduct)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} aria-labelledby="form-dialog-title">
      <DialogTitle id="form-dialog-title">Edit product</DialogTitle>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            {...register('manufacturer')}
            autoFocus
            margin="dense"
            id="manufacturer"
            label="Manufacturer"
            type="text"
            fullWidth
            error={!!errors.manufacturer}
            helperText={errors.manufacturer ? errors.manufacturer.message : ''}
          />
          <TextField
            {...register('model')}
            margin="dense"
            id="model"
            label="Model"
            type="text"
            fullWidth
            error={!!errors.model}
            helperText={errors.model ? errors.model.message : ''}
          />
          <TextField
            {...register('webPage')}
            margin="dense"
            id="webPage"
            label="Web Page"
            type="text"
            fullWidth
            error={!!errors.webPage}
            helperText={errors.webPage ? errors.webPage.message : ''}
          />
          <TextField
            {...register('notes')}
            margin="dense"
            id="notes"
            label="Notes"
            type="text"
            fullWidth
            error={!!errors.notes}
            helperText={errors.notes ? errors.notes.message : ''}
          />
          <DialogActions>
            <Button onClick={onClose} color="primary">
              Cancel
            </Button>
            <Button type="submit" color="primary">
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default ProductEditorModal
