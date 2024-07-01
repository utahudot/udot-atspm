import { ChangePasswordOptionsComponent } from '@/features/identity/components/changePassword/changePasswordOptionsComponent'
import {
  useChangePasswordHandler,
  useVerifyTokenHandler,
} from '@/features/identity/components/handlers/ChangePasswordHandler'
import { Alert, Box, Container, Snackbar, Typography } from '@mui/material'

const ChangePassword = () => {
  const validTokenHandler = useVerifyTokenHandler()
  const changePasswordHandler = useChangePasswordHandler()

  if (validTokenHandler.isLoadingValidity || !validTokenHandler.isValidToken) {
    return
  }

  if (
    changePasswordHandler.submitted &&
    changePasswordHandler.responseSuccess
  ) {
    window.location.href = '/locations'
  }

  return (
    <Box>
      <Container component="main" maxWidth="xs">
        <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
          Change Password
        </Typography>
        <Box
          sx={{
            marginTop: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
          }}
        >
          <ChangePasswordOptionsComponent handler={changePasswordHandler} />
        </Box>
      </Container>
      <Snackbar
        open={
          changePasswordHandler.submitted &&
          changePasswordHandler.responseSuccess
        }
        autoHideDuration={6000}
        onClose={() => changePasswordHandler.handleResponseSuccess(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          onClose={() => changePasswordHandler.handleResponseSuccess(false)}
          severity="success"
        >
          New Password saved!
        </Alert>
      </Snackbar>
      <Snackbar
        open={
          changePasswordHandler.submitted && changePasswordHandler.responseError
        }
        autoHideDuration={6000}
        onClose={() => changePasswordHandler.handleResponseError(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          onClose={() => changePasswordHandler.handleResponseError(false)}
          severity="error"
        >
          Error in saving new password, Try Again!
        </Alert>
      </Snackbar>
    </Box>
  )
}
export default ChangePassword
