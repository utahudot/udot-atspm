import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useEditUserInfo } from '@/features/identity/api/editUserInfo'
import { useUserInfo } from '@/features/identity/api/getUserInfo'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Button,
  Divider,
  Grid,
  Link,
  Paper,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { Controller, SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  firstName: z.string().min(1, { message: 'First Name is required' }),
  lastName: z.string().min(1, { message: 'Last Name is required' }),
  agency: z.string().min(1, { message: 'Agency is required' }),
  email: z.string().email({ message: 'Invalid email address' }),
})

type FormData = z.infer<typeof schema>

const ProfilePage = () => {
  const [isEditing, setIsEditing] = useState(false)

  const { data: profile } = useUserInfo({ config: { enabled: true } })
  const { mutate: saveUser } = useEditUserInfo()

  const initial = useRef<FormData | null>(null)

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      firstName: '',
      lastName: '',
      agency: '',
      email: '',
    },
  })

  useEffect(() => {
    if (profile) {
      const init = {
        firstName: profile.firstName,
        lastName: profile.lastName,
        agency: profile.agency,
        email: profile.email,
      }
      initial.current = init
      reset(init)
      setIsEditing(false)
    }
  }, [profile, reset])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    saveUser({ ...data, roles: profile?.roles })
    setIsEditing(false)
  }

  const cancel = () => {
    if (initial.current) reset(initial.current)
    setIsEditing(false)
  }
  return (
    <ResponsivePageLayout title="Profile" width={800} noBottomMargin>
      <Paper
        sx={{
          p: 2,
          px: 4,
          pb: 4,
          mt: 2,
          mx: 'auto',
        }}
      >
        <Box
          display={'flex'}
          justifyContent="space-between"
          mb={3}
          sx={{ alignItems: 'center' }}
        >
          <Typography variant="h4" sx={{ mb: 0 }}>
            Your Information
          </Typography>

          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
            <Button
              variant="contained"
              onClick={
                isEditing ? handleSubmit(onSubmit) : () => setIsEditing(true)
              }
            >
              {isEditing ? 'Save' : 'Edit'}
            </Button>
            {isEditing && (
              <Button onClick={cancel} variant="outlined" color="inherit">
                Cancel
              </Button>
            )}
          </Box>
        </Box>

        <Grid container spacing={2}>
          {[
            {
              name: 'firstName',
              label: 'First Name',
              xs: 12,
              sm: 6,
              error: errors.firstName,
            },
            {
              name: 'lastName',
              label: 'Last Name',
              xs: 12,
              sm: 6,
              error: errors.lastName,
            },
            {
              name: 'agency',
              label: 'Agency',
              xs: 12,
              sm: 6,
              error: errors.agency,
            },
            {
              name: 'email',
              label: 'Email',
              xs: 12,
              sm: 6,
              error: errors.email,
            },
          ].map(({ name, label, xs, sm, error }) => (
            <Grid item xs={xs} sm={sm} key={name}>
              <Controller
                name={name as keyof FormData}
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={label}
                    fullWidth
                    disabled={!isEditing}
                    error={!!error}
                    helperText={error?.message}
                  />
                )}
              />
            </Grid>
          ))}
        </Grid>

        {profile?.roles && (
          <Typography variant="subtitle2" color="text.secondary" sx={{ mt: 3 }}>
            Roles: {profile.roles}
          </Typography>
        )}

        <Divider sx={{ my: 3 }} />

        <Typography variant="h4" sx={{ mb: 2 }}>
          Security
        </Typography>
        <Link href="/password-reset" underline="none">
          <Button variant="outlined">Update Password</Button>
        </Link>
      </Paper>
    </ResponsivePageLayout>
  )
}

export default ProfilePage
