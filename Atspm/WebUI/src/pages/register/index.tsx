import { useRegistrationHandler } from '@/features/identity/components/handlers/RegistrationHandler'
import { RegistrationOptionsComponent } from '@/features/identity/components/register/registrationOptionsComponent'
import MeetingRoomSharpIcon from '@mui/icons-material/MeetingRoomSharp'
import { Alert, Box, Container, Snackbar, Typography } from '@mui/material'

function Register() {
  const handler = useRegistrationHandler()
  return (
    <Container component="main" maxWidth="xs">
      <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
        Registration
      </Typography>
      <Box
        sx={{
          marginTop: 4,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <MeetingRoomSharpIcon sx={{ fontSize: '36px' }} />
        <Typography component="h1" variant="h5" fontSize="24px">
          Sign up
        </Typography>
        <RegistrationOptionsComponent handler={handler} />
      </Box>
      <Snackbar
        open={handler.responseSuccess}
        onClose={() => handler.handleResponseSuccess(false)}
      >
        <Alert elevation={6} variant="filled" severity="success">
          New Password saved!
        </Alert>
      </Snackbar>
      <Snackbar
        open={handler.responseError}
        onClose={() => handler.handleResponseError(false)}
      >
        <Alert elevation={6} variant="filled" severity="error">
          {handler.data?.message}
        </Alert>
      </Snackbar>
    </Container>
  )
}

export default Register
