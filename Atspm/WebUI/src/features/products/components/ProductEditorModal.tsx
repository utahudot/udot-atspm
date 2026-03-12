import { Product } from '@/api/config'
import ATSPMDialog from '@/components/ATSPMDialog'
import { zodResolver } from '@hookform/resolvers/zod'
import { TextField } from '@mui/material'
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
    <ATSPMDialog
      isOpen={isOpen}
      onClose={onClose}
      onSubmit={handleSubmit(onSubmit)}
      title="Edit product"
      auditInfo={product}
    >
      <TextField
        {...register('manufacturer')}
        autoFocus
        margin="dense"
        id="manufacturer"
        label="Manufacturer"
        type="text"
        fullWidth
        error={!!errors.manufacturer}
      />
      <TextField
        {...register('model')}
        margin="dense"
        id="model"
        label="Model"
        type="text"
        fullWidth
        error={!!errors.model}
      />
      <TextField
        {...register('webPage')}
        margin="dense"
        id="webPage"
        label="Web Page"
        type="text"
        fullWidth
        error={!!errors.webPage}
      />
      <TextField
        {...register('notes')}
        margin="dense"
        id="notes"
        label="Notes"
        type="text"
        fullWidth
        error={!!errors.notes}
      />
    </ATSPMDialog>
  )
}

export default ProductEditorModal
