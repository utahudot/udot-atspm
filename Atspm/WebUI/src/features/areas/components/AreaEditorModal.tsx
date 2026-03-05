import { Area } from '@/api/config'
import ATSPMDialog from '@/components/ATSPMDialog'
import { zodResolver } from '@hookform/resolvers/zod'
import { TextField } from '@mui/material'
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
    <ATSPMDialog
      isOpen={isOpen}
      onSubmit={handleSubmit(onSubmit)}
      onClose={onClose}
      title="Edit Area"
      auditInfo={area}
      dialogProps={{ sx: { minWidth: 400, pt: 0 } }}
    >
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
    </ATSPMDialog>
  )
}

export default AreaEditorModal
