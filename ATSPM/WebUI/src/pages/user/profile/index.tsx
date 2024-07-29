import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useEditUserInfo } from '@/features/identity/api/editUserInfo'
import { useUserInfo } from '@/features/identity/api/getUserInfo'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Alert,
  Box,
  Button,
  Link,
  Paper,
  Snackbar,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState, useRef } from 'react'
import { SubmitHandler, useForm, Controller } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  firstName: z.string().min(1, { message: 'First Name is required' }),
  lastName: z.string().min(1, { message: 'Last Name is required' }),
  agency: z.string().min(1, { message: 'Agency is required' }),
  email: z.string().email({ message: 'Invalid email address' }),
  phoneNumber: z
    .string()
    .regex(/^(\+1|1)?[-.\s]?\(?[2-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/, {
      message: 'Must be a valid phone number',
    }),
  roles: z.string(),
})

type FormData = z.infer<typeof schema>

const ProfilePage = () => {
  const [activeTab, setActiveTab] = useState(0)
  const [isEditing, setIsEditing] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const [responseSuccess, setResponseSuccess] = useState(false)
  const [responseError, setResponseError] = useState(false)
  const initialProfileData = useRef<FormData | null>(null)
  const { mutate } = useEditUserInfo()
  const { data: profileData, error: userError, isLoading } = useUserInfo({
    config: { enabled: true },
  })
  const {
    control,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      firstName: '',
      lastName: '',
      agency: '',
      email: '',
      phoneNumber: '',
      roles: '',
    },
  })

  useEffect(() => {
    if (profileData) {
      const { firstName, lastName, agency, email, phoneNumber, roles } =
        profileData
      const initialData = {
        firstName,
        lastName,
        agency,
        email,
        phoneNumber: phoneNumber || '',
        roles,
      }
      initialProfileData.current = initialData
      reset(initialData)
      setIsEditing(false)
    }
  }, [profileData, reset])

  const onSubmit: SubmitHandler<FormData> = async (data) => {
    mutate(data, {
      onSuccess: () => {
        setIsEditing(false)
        setResponseSuccess(true)
        setSubmitted(true)
      },
      onError: () => {
        setResponseError(true)
      },
    })
  }

  const handleCancel = () => {
    if (initialProfileData.current) {
      reset(initialProfileData.current)
    }
    setIsEditing(false)
  }

  const handleTabChange = (_: unknown, newValue: number) => {
    setActiveTab(newValue)
  }

  return (
    <ResponsivePageLayout title={'Profile Page'}>
      <Tabs value={activeTab} onChange={handleTabChange}>
        <Tab label="Info" />
        <Tab label="Settings" />
      </Tabs>

      {activeTab === 0 && (
        <Paper elevation={3} sx={{ p: 3, m: 3 }}>
          <Typography variant="h2">Profile</Typography>
          <Box sx={{ display: 'flex', gap: '20px', mb: '20px', mt: '20px' }}>
            <Controller
              name="firstName"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="First Name"
                  disabled={!isEditing}
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
                  label="Last Name"
                  disabled={!isEditing}
                  error={!!errors.lastName}
                  helperText={errors.lastName?.message}
                />
              )}
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <Controller
              name="agency"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Agency"
                  disabled={!isEditing}
                  error={!!errors.agency}
                  helperText={errors.agency?.message}
                />
              )}
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <Controller
              name="email"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Email"
                  disabled={!isEditing}
                  error={!!errors.email}
                  helperText={errors.email?.message}
                />
              )}
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <Controller
              name="phoneNumber"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Phone Number"
                  disabled={!isEditing}
                  error={!!errors.phoneNumber}
                  helperText={errors.phoneNumber?.message}
                  required
                />
              )}
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <Controller
              name="roles"
              control={control}
              render={({ field }) => (
                <TextField {...field} label="Roles" disabled />
              )}
            />
          </Box>

          <Button
            variant="contained"
            color="primary"
            onClick={() => {
              if (isEditing) {
                handleSubmit(onSubmit)()
              } else {
                setIsEditing(true)
              }
            }}
            disabled={isEditing && !!errors.phoneNumber}
          >
            {isEditing ? 'Save' : 'Edit'}
          </Button>
          {isEditing && (
            <Button onClick={handleCancel}>Cancel</Button>
          )}
        </Paper>
      )}
      {activeTab === 1 && (
        <Paper elevation={3} sx={{ p: 3, m: 3 }}>
          <Typography variant="h2">Settings</Typography>
          <Box sx={{ mb: '20px' }}>
            <Link href="/password-reset" underline="none">
              <Box
                sx={{
                  p: '10px',
                  borderRadius: '5px',
                  cursor: 'pointer',
                  width: 'fit-content',
                }}
              >
                Update Password
              </Box>
            </Link>
          </Box>
        </Paper>
      )}

      <Snackbar
        open={submitted && responseSuccess}
        onClose={() => setResponseSuccess(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          severity="success"
          onClose={() => setResponseSuccess(false)}
        >
          Profile Data Saved!
        </Alert>
      </Snackbar>
      <Snackbar
        open={submitted && responseError}
        onClose={() => setResponseError(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          severity="error"
          onClose={() => setResponseError(false)}
        >
          Error Saving Profile
        </Alert>
      </Snackbar>
    </ResponsivePageLayout>
  )
}

export default ProfilePage
