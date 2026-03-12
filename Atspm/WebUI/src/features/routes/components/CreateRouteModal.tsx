import { Route } from '@/api/config'
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

type CreateRouteModalProps = {
  data?: Route
  isOpen: boolean
  onClose: () => void
  onSave: (route: Route) => void
}

const CreateRouteModal = ({
  data: route,
  isOpen,
  onClose,
  onSave,
}: CreateRouteModalProps) => {
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: route?.name || '',
    },
  })

  useEffect(() => {
    if (route) {
      setValue('name', route.name)
    }
  }, [route, setValue])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updatedRoute = { ...route, ...data } as Route
    onSave(updatedRoute)
    onClose()
  }

  return (
    <ATSPMDialog
      onClose={onClose}
      isOpen={isOpen}
      title="Create Route"
      auditInfo={route}
      onSubmit={handleSubmit(onSubmit)}
      dialogProps={{ sx: { minWidth: 500 } }}
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

export default CreateRouteModal
