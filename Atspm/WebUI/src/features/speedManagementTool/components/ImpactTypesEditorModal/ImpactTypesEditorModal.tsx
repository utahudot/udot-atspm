import { ImpactType } from '@/features/speedManagementTool/types/impact'
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
  description: z.string().optional(),
})

type FormData = z.infer<typeof schema>

type ImpactTypeEditorModalProps = {
  data?: ImpactType
  isOpen: boolean
  onClose: () => void
  onSave: (impactType: ImpactType) => void
}

const ImpactTypeEditorModal = ({
  data: impactType,
  isOpen,
  onClose,
  onSave,
}: ImpactTypeEditorModalProps) => {
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: impactType?.name || '',
      description: impactType?.description || '',
    },
  })

  useEffect(() => {
    if (impactType) {
      setValue('name', impactType.name ?? '')
      setValue('description', impactType.description ?? '')
    } else {
      setValue('name', '')
      setValue('description', '')
    }
  }, [impactType, setValue])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updated: ImpactType = { ...impactType, ...data } as ImpactType
    onSave(updated)
    onClose()
  }

  const title = impactType?.id ? 'Edit Impact Type' : 'New Impact Type'

  return (
    <Dialog
      open={isOpen}
      onClose={onClose}
      aria-labelledby="impacttype-dialog-title"
    >
      <DialogTitle id="impacttype-dialog-title">{title}</DialogTitle>
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
          <TextField
            {...register('description')}
            margin="dense"
            id="description"
            label="Description"
            type="text"
            fullWidth
            multiline
            rows={3}
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

export default ImpactTypeEditorModal
