import { Area } from '@/api/config'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  name: z.string().min(1, { message: 'Name is required' }),
})

type FormData = z.infer<typeof schema>

type AreaEditorModalProps = {
  data?: Area
  isOpen: boolean
  onClose: () => void
  onSave: (area: Area) => void
}

const AreaEditorModal = ({
  data: area,
  isOpen,
  onClose,
  onSave,
}: AreaEditorModalProps) => {
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: area?.name || '',
    },
  })

  useEffect(() => {
    if (area) {
      setValue('name', area.name)
    }
  }, [area, setValue])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedArea = { ...area, ...data } as Area
    onSave(updatedArea)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} aria-labelledby="form-dialog-title">
      <DialogTitle id="form-dialog-title">Edit Area</DialogTitle>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            {...register('name')}
            autoFocus
            margin="dense"
            id="name"
            label="Name"
            type="text"
            fullWidth
            error={!!errors.name}
            helperText={errors.name ? errors.name.message : ''}
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

export default AreaEditorModal
