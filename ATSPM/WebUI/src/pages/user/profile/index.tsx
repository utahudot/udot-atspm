import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useProfileHandler } from '@/features/identity/components/handlers/profileHandler'
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
import { useState } from 'react'

const ProfilePage = () => {
  const [activeTab, setActiveTab] = useState(0)
  const handler = useProfileHandler()

  const handleTabChange = (_: unknown, newValue: number) => {
    setActiveTab(newValue)
  }

  if (handler.isLoading) {
    return
  }

  return (
    <ResponsivePageLayout title={'Profile Page'}>
      {/* <Typography variant="h1">Profile Page</Typography> */}
      {/* Tabs */}
      <Tabs value={activeTab} onChange={handleTabChange}>
        <Tab label="Info" />
        <Tab label="Settings" />
      </Tabs>
      {/* Profile View */}
      {activeTab === 0 && (
        <Paper elevation={3} sx={{ p: 3, m: 3 }}>
          <Typography variant="h2">Profile</Typography>
          <Box sx={{ display: 'flex', gap: '20px', mb: '20px', mt: '20px' }}>
            <TextField
              label="First Name"
              disabled={!handler.isEditing}
              defaultValue={handler.profileData.firstName}
              onChange={(e) =>
                handler.handleInputChange('firstName', e.target.value)
              }
            />
            <TextField
              label="Last Name"
              disabled={!handler.isEditing}
              defaultValue={handler.profileData.lastName}
              onChange={(e) =>
                handler.handleInputChange('lastName', e.target.value)
              }
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <TextField
              label="Company"
              disabled={!handler.isEditing}
              defaultValue={handler.profileData.agency}
              onChange={(e) =>
                handler.handleInputChange('company', e.target.value)
              }
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <TextField
              label="Email"
              disabled={!handler.isEditing}
              defaultValue={handler.profileData.email}
              onChange={(e) =>
                handler.handleInputChange('email', e.target.value)
              }
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <TextField
              label="Phone Number"
              disabled={!handler.isEditing}
              defaultValue={handler.profileData.phoneNumber}
              onChange={(e) =>
                handler.handleInputChange('phoneNumber', e.target.value)
              }
            />
          </Box>
          <Box sx={{ mb: '20px' }}>
            <TextField
              label="Roles"
              disabled
              value={handler.profileData.roles}
            />
          </Box>

          <Button
            variant="contained"
            color="primary"
            onClick={() =>
              handler.isEditing
                ? handler.handleSaveClick()
                : handler.handleEditClick()
            }
          >
            {handler.isEditing ? 'Save' : 'Edit'}
          </Button>
        </Paper>
      )}
      {activeTab === 1 && (
        <Paper elevation={3} sx={{ p: 3, m: 3 }}>
          <Typography variant="h2">Settings</Typography>
          <Box sx={{ mb: '20px' }}>
            <Link href="/verifyUser" underline="none">
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
        open={handler.submitted && handler.responseSuccess}
        onClose={() => handler.handleResponseSuccess(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          severity="success"
          onClose={() => handler.handleResponseSuccess(false)}
        >
          Profile Data Saved!
        </Alert>
      </Snackbar>
      <Snackbar
        open={handler.submitted && handler.responseError}
        onClose={() => handler.handleResponseError(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          severity="error"
          onClose={() => handler.handleResponseError(false)}
        >
          Error Saving Profile
        </Alert>
      </Snackbar>
  </ResponsivePageLayout>
  )
}

export default ProfilePage
