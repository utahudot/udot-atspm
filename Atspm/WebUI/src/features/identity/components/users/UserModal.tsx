import { useGetRoles } from '@/features/identity/api/getRoles'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Button,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
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
}

interface ModalProps {
  isOpen: boolean
  onClose: () => void
  data: User | null
  onSave: (user: User) => void
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
})

type UserFormData = z.infer<typeof userSchema>

const UserModal = ({ isOpen, onClose, data, onSave }: ModalProps) => {
  const { data: roles, isLoading } = useGetRoles()
  const {
    control,
    handleSubmit,
    formState: { errors },
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
    },
  })

  // Reset form when data changes
  useEffect(() => {
    if (data) {
      reset(data)
    }
  }, [data, reset])

  const onSubmit = (formData: UserFormData) => {
    onSave(formData)
    onClose()
  }

  if (isLoading) return <div>content is loading...</div>

  return (
    <Dialog open={isOpen} onClose={onClose}>
      <DialogTitle sx={{ fontSize: '1.3rem' }} id="role-permissions-label">
        User Details
      </DialogTitle>
      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
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
              <FormControl fullWidth margin="dense">
                <InputLabel>Roles</InputLabel>
                <Select
                  {...field}
                  multiple
                  label="Roles"
                  renderValue={(selected) => (
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {selected.map((value) => (
                        <Chip
                          key={value}
                          label={value.replace(/([A-Z])/g, ' $1').trim()}
                        />
                      ))}
                    </Box>
                  )}
                >
                  {roles
                    ?.sort((a, b) => a.role.localeCompare(b.role))
                    .map((role) => (
                      <MenuItem key={role.role} value={role.role}>
                        <Checkbox checked={field.value.includes(role.role)} />
                        {role.role.replace(/([A-Z])/g, ' $1').trim()}
                      </MenuItem>
                    ))}
                </Select>
              </FormControl>
            )}
          />
        </form>
      </DialogContent>
      <DialogActions>
        <Box sx={{ marginRight: '1rem', marginBottom: '.5rem' }}>
          <Button onClick={onClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit(onSubmit)}>
            Update User
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  )
}

export default UserModal
