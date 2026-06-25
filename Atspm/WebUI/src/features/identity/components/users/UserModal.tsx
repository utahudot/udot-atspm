import ATSPMDialog from '@/components/ATSPMDialog/ATSPMDialog'
import CustomSelect from '@/components/customSelect/CustomSelect'
import { useGetAreas } from '@/features/areas/api/areaApi'
import { useGetRoles } from '@/features/identity/api/getRoles'
import { useGetJurisdiction } from '@/features/jurisdictions/api/jurisdictionApi'
import { useGetRegion } from '@/features/region/api/regionApi'
import { zodResolver } from '@hookform/resolvers/zod'
import { TextField } from '@mui/material'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface User {
  userId: string
  firstName: string
  lastName: string
  userName: string
  agency: string
  email: string
  roles: string[]
  areas: { id: number; name: string }[]
  areaIds: number[]
  regions: { id: number; description: string }[]
  regionIds: number[]
  jurisdictions: { id: number; name: string }[]
  jurisdictionIds: number[]
}

interface ModalProps {
  isOpen?: boolean
  open?: boolean
  onClose: () => void
  data: User | null
  onSave: (user: UserFormData) => void | Promise<void>
}

// Define Zod schema
const userSchema = z.object({
  userId: z.string(),
  firstName: z.string().min(1, { message: 'First name required' }),
  lastName: z.string().min(1, { message: 'Last name required' }),
  userName: z.string().min(1, { message: 'Username is required' }),
  agency: z.string().min(1, { message: 'Agency required' }),
  email: z.string().email('Please enter a valid email address'),
  roles: z.array(z.string()),
  areaIds: z.array(z.number()),
  regionIds: z.array(z.number()),
  jurisdictionIds: z.array(z.number()),
})

type UserFormData = z.infer<typeof userSchema>

const normalizeNumberArray = (value: unknown): number[] => {
  if (typeof value === 'string') {
    return value
      .split(',')
      .map((item) => Number(item))
      .filter((item) => !Number.isNaN(item))
  }

  if (Array.isArray(value)) {
    return value
      .map((item) => Number(item))
      .filter((item) => !Number.isNaN(item))
  }

  return []
}

const UserModal = ({ isOpen, open, onClose, data, onSave }: ModalProps) => {
  const { data: roles, isLoading } = useGetRoles()
  const { data: areasData, isLoading: areasLoading } = useGetAreas()
  const { data: jurisdictionsData, isLoading: jurisdictionsLoading } =
    useGetJurisdiction()
  const { data: regionsData, isLoading: regionsLoading } = useGetRegion()
  const [isSaving, setIsSaving] = useState(false)
  const {
    control,
    formState: { errors },
    getValues,
    reset,
  } = useForm<UserFormData>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      userId: data?.userId || '',
      firstName: data?.firstName || '',
      lastName: data?.lastName || '',
      userName: data?.userName || '',
      agency: data?.agency || '',
      email: data?.email || '',
      roles: data?.roles || [],
      areaIds: data?.areaIds || [],
      regionIds: data?.regionIds || [],
      jurisdictionIds: data?.jurisdictionIds || [],
    },
  })

  // Reset form when data changes
  useEffect(() => {
    if (data) {
      reset(data)
    }
  }, [data, reset])

  const onSubmit = async (formData: UserFormData) => {
    setIsSaving(true)
    try {
      await Promise.resolve(onSave(formData))
      onClose()
    } finally {
      setIsSaving(false)
    }
  }

  const handleSaveClick = async () => {
    await onSubmit(getValues())
  }

  const selectLoading =
    isLoading || areasLoading || jurisdictionsLoading || regionsLoading
  const modalOpen = open ?? isOpen ?? false

  if (selectLoading) return <div>content is loading...</div>

  return (
    <ATSPMDialog
      isOpen={modalOpen}
      onClose={onClose}
      title="User Details"
      auditInfo={data}
      dialogProps={{ sx: { width: 500, pt: 0 } }}
      saveButtonProps={{
        disabled: isSaving,
        onClick: handleSaveClick,
      }}
    >
      <Controller
        name="firstName"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            autoFocus
            margin="dense"
            label="First Name"
            type="text"
            fullWidth
            error={!!errors.firstName}
            helperText={errors.firstName?.message}
          />
        )}
      />

      <Controller
        name="lastName"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            margin="dense"
            label="Last Name"
            type="text"
            fullWidth
            error={!!errors.lastName}
            helperText={errors.lastName?.message}
          />
        )}
      />

      <Controller
        name="userName"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            margin="dense"
            label="Username"
            type="text"
            fullWidth
            error={!!errors.userName}
            helperText={errors.userName?.message}
          />
        )}
      />

      <Controller
        name="agency"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            margin="dense"
            label="Agency"
            type="text"
            fullWidth
            error={!!errors.agency}
            helperText={errors.agency?.message}
          />
        )}
      />

      <Controller
        name="email"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            margin="dense"
            label="Email"
            type="email"
            fullWidth
            error={!!errors.email}
            helperText={errors.email?.message}
          />
        )}
      />

      <Controller
        name="roles"
        control={control}
        render={({ field }) => (
          <CustomSelect
            label="Roles"
            name="roles"
            value={field.value}
            data={roles
              ?.sort((a, b) => a.role.localeCompare(b.role))
              .map((role) => ({
                id: role.role,
                role: role.role.replace(/([A-Z])/g, ' $1').trim(),
              }))}
            onChange={(event) => field.onChange(event.target.value as string[])}
            onDelete={(id) =>
              field.onChange(field.value.filter((value) => value !== id))
            }
            displayProperty="role"
            fullWidth
            multiple
            size="small"
          />
        )}
      />

      <Controller
        name="regionIds"
        control={control}
        render={({ field }) => (
          <CustomSelect
            label="Regions"
            name="regionIds"
            value={field.value}
            data={regionsData?.value}
            onChange={(event) => field.onChange(normalizeNumberArray(event.target.value))}
            onDelete={(id) =>
              field.onChange(field.value.filter((value) => value !== Number(id)))
            }
            displayProperty="description"
            fullWidth
            multiple
            size="small"
          />
        )}
      />

      <Controller
        name="jurisdictionIds"
        control={control}
        render={({ field }) => (
          <CustomSelect
            label="Jurisdictions"
            name="jurisdictionIds"
            value={field.value}
            data={jurisdictionsData?.value}
            onChange={(event) => field.onChange(normalizeNumberArray(event.target.value))}
            onDelete={(id) =>
              field.onChange(field.value.filter((value) => value !== Number(id)))
            }
            displayProperty="name"
            fullWidth
            multiple
            size="small"
          />
        )}
      />

      <Controller
        name="areaIds"
        control={control}
        render={({ field }) => (
          <CustomSelect
            label="Areas"
            name="areaIds"
            value={field.value}
            data={areasData?.value}
            onChange={(event) => field.onChange(normalizeNumberArray(event.target.value))}
            onDelete={(id) =>
              field.onChange(field.value.filter((value) => value !== Number(id)))
            }
            displayProperty="name"
            fullWidth
            multiple
            size="small"
          />
        )}
      />
    </ATSPMDialog>
  )
}

export default UserModal
