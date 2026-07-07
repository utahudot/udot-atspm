import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useApiKeys } from '@/features/identity/api/apiKeys'
import ApiKeyManagement from '@/features/identity/components/apiKeys/ApiKeyManagement'
import { useEditUserInfo } from '@/features/identity/api/editUserInfo'
import { useUserInfo } from '@/features/identity/api/getUserInfo'
import { zodResolver } from '@hookform/resolvers/zod'
import CloseIcon from '@mui/icons-material/Close'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import LockResetIcon from '@mui/icons-material/LockReset'
import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined'
import {
  Box,
  Button,
  Chip,
  Divider,
  Grid,
  Link,
  Paper,
  Stack,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material'
import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { ReactNode, useEffect, useMemo, useRef, useState } from 'react'
import { Controller, SubmitHandler, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  firstName: z.string().min(1, { message: 'First Name is required' }),
  lastName: z.string().min(1, { message: 'Last Name is required' }),
  agency: z.string().min(1, { message: 'Agency is required' }),
  email: z.string().email({ message: 'Invalid email address' }),
})

type FormData = z.infer<typeof schema>

const getCookieClaims = () =>
  Cookies.get('claims')
    ?.split(',')
    .map((claim) => claim.trim())
    .filter(Boolean) ?? []

const TabPanel = ({
  children,
  index,
  value,
}: {
  children: ReactNode
  index: number
  value: number
}) => {
  if (value !== index) return null

  return (
    <Box
      role="tabpanel"
      id={`profile-tabpanel-${index}`}
      aria-labelledby={`profile-tab-${index}`}
      sx={{ pt: 3 }}
    >
      {children}
    </Box>
  )
}

const ProfilePage = () => {
  const [isEditing, setIsEditing] = useState(false)
  const [activeTab, setActiveTab] = useState(0)
  const router = useRouter()

  const { data: profile } = useUserInfo({ config: { enabled: true } })
  const { mutate: saveUser } = useEditUserInfo()
  const currentClaims = getCookieClaims()
  const isGlobalAdmin = currentClaims.includes('Admin')
  const canViewApiKeys =
    isGlobalAdmin || currentClaims.includes('ApiKey:View')
  const {
    data: apiKeys = [],
    isLoading: isApiKeysLoading,
    refetch: refetchApiKeys,
  } = useApiKeys({ enabled: canViewApiKeys, scope: 'mine' })
  const {
    data: allApiKeys = [],
    isLoading: isAllApiKeysLoading,
    refetch: refetchAllApiKeys,
  } = useApiKeys({
    enabled: isGlobalAdmin && activeTab === 3,
    scope: 'all',
  })

  const roles = useMemo(() => {
    const rawRoles = profile?.roles as unknown
    if (Array.isArray(rawRoles)) return rawRoles
    if (typeof rawRoles === 'string') {
      return rawRoles
        .split(',')
        .map((role) => role.trim())
        .filter(Boolean)
    }
    return []
  }, [profile?.roles])

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

  useEffect(() => {
    if (!router.isReady || !isGlobalAdmin) return

    const tab = Array.isArray(router.query.tab)
      ? router.query.tab[0]
      : router.query.tab

    if (tab === 'all-api-keys') {
      setActiveTab(3)
    }
  }, [isGlobalAdmin, router.isReady, router.query.tab])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    saveUser({ ...data, roles })
    setIsEditing(false)
  }

  const cancel = () => {
    if (initial.current) reset(initial.current)
    setIsEditing(false)
  }
  return (
    <ResponsivePageLayout title="Profile" width={960} noBottomMargin>
      <Box
        sx={{
          mt: 2,
          mx: 'auto',
          width: '100%',
        }}
      >
        <Tabs
          value={activeTab}
          onChange={(_, value) => setActiveTab(value)}
          aria-label="Profile sections"
          variant="fullWidth"
          sx={{
            borderBottom: 1,
            borderColor: 'divider',
            minHeight: 48,
            '& .MuiTabs-indicator': { display: 'none' },
            '& .MuiTab-root': {
              minHeight: 48,
              px: { xs: 1, sm: 2 },
              borderBottom: 2,
              borderBottomColor: 'transparent',
              color: 'text.secondary',
              fontWeight: 600,
              textTransform: 'none',
              bgcolor: 'transparent',
            },
            '& .Mui-selected': {
              color: 'primary.main',
              borderBottomColor: 'primary.main',
            },
          }}
        >
          <Tab
            id="profile-tab-0"
            aria-controls="profile-tabpanel-0"
            label="Account"
          />
          <Tab
            id="profile-tab-1"
            aria-controls="profile-tabpanel-1"
            label="Security"
          />
          <Tab
            id="profile-tab-2"
            aria-controls="profile-tabpanel-2"
            label="API Keys"
          />
          {isGlobalAdmin && (
            <Tab
              id="profile-tab-3"
              aria-controls="profile-tabpanel-3"
              label="All API Keys"
            />
          )}
        </Tabs>

        <TabPanel value={activeTab} index={0}>
          <Paper
            variant="outlined"
            sx={{
              p: { xs: 2, sm: 4 },
              borderRadius: 1,
            }}
          >
            <Box
              display="flex"
              justifyContent="space-between"
              mb={3}
              sx={{
                alignItems: { xs: 'flex-start', sm: 'center' },
                flexDirection: { xs: 'column', sm: 'row' },
                gap: 2,
              }}
            >
              <Typography
                variant="h6"
                sx={{
                  letterSpacing: 1,
                  textTransform: 'uppercase',
                  color: 'text.secondary',
                }}
              >
                Your Information
              </Typography>

              <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                <Button
                  variant={isEditing ? 'contained' : 'outlined'}
                  startIcon={
                    isEditing ? <SaveOutlinedIcon /> : <EditOutlinedIcon />
                  }
                  onClick={
                    isEditing
                      ? handleSubmit(onSubmit)
                      : () => setIsEditing(true)
                  }
                >
                  {isEditing ? 'Save' : 'Edit'}
                </Button>
                {isEditing && (
                  <Button
                    onClick={cancel}
                    variant="outlined"
                    color="inherit"
                    startIcon={<CloseIcon />}
                  >
                    Cancel
                  </Button>
                )}
              </Box>
            </Box>

            <Grid container spacing={2.5}>
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

            {roles.length > 0 && (
              <>
                <Divider sx={{ my: 3 }} />
                <Box>
                  <Typography variant="subtitle1" sx={{ mb: 1 }}>
                    Roles
                  </Typography>
                  <Stack direction="row" gap={1} flexWrap="wrap">
                    {roles.map((role) => (
                      <Chip key={role} label={role} variant="outlined" />
                    ))}
                  </Stack>
                </Box>
              </>
            )}
          </Paper>
        </TabPanel>

        <TabPanel value={activeTab} index={1}>
          <Paper
            variant="outlined"
            sx={{
              p: { xs: 2, sm: 4 },
              borderRadius: 1,
            }}
          >
            <Typography
              variant="h6"
              sx={{
                letterSpacing: 1,
                textTransform: 'uppercase',
                color: 'text.secondary',
                mb: 1,
              }}
            >
              Security
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Update your password and account access settings.
            </Typography>
            <Link href="/password-reset" underline="none">
              <Button variant="outlined" startIcon={<LockResetIcon />}>
                Update Password
              </Button>
            </Link>
          </Paper>
        </TabPanel>

        <TabPanel value={activeTab} index={2}>
          <ApiKeyManagement
            currentClaims={currentClaims}
            apiKeys={apiKeys}
            isApiKeysLoading={isApiKeysLoading}
            refetchApiKeys={refetchApiKeys}
          />
        </TabPanel>

        {isGlobalAdmin && (
          <TabPanel value={activeTab} index={3}>
            <ApiKeyManagement
              mode="admin"
              currentClaims={currentClaims}
              apiKeys={allApiKeys}
              isApiKeysLoading={isAllApiKeysLoading}
              refetchApiKeys={refetchAllApiKeys}
            />
          </TabPanel>
        )}
      </Box>
    </ResponsivePageLayout>
  )
}

export default ProfilePage
