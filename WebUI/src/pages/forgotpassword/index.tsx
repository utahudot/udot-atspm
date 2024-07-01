import { useForgotPassword } from '@/features/identity/api/forgotPassword'
import {
  Alert,
  Box,
  Button,
  Container,
  Grid,
  Snackbar,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

const ForgotPassword = () => {
  const [email, setEmail] = useState<string>('')
  const [invalidEmail, setInvalidEmail] = useState(false)
  const [showSnackbar, setShowSnackbar] = useState(false)

  const { refetch, data } = useForgotPassword({ email })

  const handleSnackbarClose = () => {
    setShowSnackbar(false)
  }

  useEffect(() => {
    if (data) {
      setShowSnackbar(true)
      window.location.href = '/locations'
    }
  }, [data])

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(email)) {
      setInvalidEmail(true)
      setShowSnackbar(false)
      return
    }

    setInvalidEmail(false)
    refetch()
    // setShowSnackbar(true)
  }

  return (
    <Container component="main" maxWidth="xs">
      <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
        Forgot Password
      </Typography>
      <Typography variant="body2" sx={{ textAlign: 'center', mt: 2 }}>
        {`Enter the email linked to your account. We'll send you a link to reset
        your password.`}
      </Typography>
      <Box
        sx={{
          marginTop: 4,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Box component="form" noValidate onSubmit={handleSubmit} sx={{ mt: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextField
                required
                fullWidth
                id="email"
                label="Email Address"
                name="email"
                autoComplete="email"
                onChange={(e) => setEmail(e.target.value)}
                error={invalidEmail}
                helperText={
                  invalidEmail ? 'Please enter a valid email address' : ''
                }
              />
            </Grid>
          </Grid>
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2 }}
          >
            Submit
          </Button>
        </Box>
      </Box>

      {/* Snackbar for successful submission */}
      <Snackbar
        open={showSnackbar}
        autoHideDuration={6000}
        onClose={handleSnackbarClose}
      >
        <Alert
          elevation={6}
          variant="filled"
          onClose={handleSnackbarClose}
          severity="success"
        >
          Request submitted successfully. Check your email for further
          instructions.
        </Alert>
      </Snackbar>
    </Container>
  )
}

export default ForgotPassword
