import { Region } from '@/features/regions/types'
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
  description: z.string().min(1, { message: 'Name is required' }),
})

type FormData = z.infer<typeof schema>

type RegionEditorModalProps = {
  data?: Region
  isOpen: boolean
  onClose: () => void
  onSave: (Region: Region) => void
}

const RegionEditorModal = ({
  data: region,
  isOpen,
  onClose,
  onSave,
}: RegionEditorModalProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      description: region?.description || '',
    },
  })

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedRegion = { ...region, ...data } as Region
    onSave(updatedRegion)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} aria-labelledby="form-dialog-title">
      <DialogTitle id="form-dialog-title">Edit Region</DialogTitle>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            {...register('description')}
            autoFocus
            margin="dense"
            id="description"
            label="Name"
            type="text"
            fullWidth
            error={!!errors.description}
            helperText={errors.description ? errors.description.message : ''}
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

export default RegionEditorModal
