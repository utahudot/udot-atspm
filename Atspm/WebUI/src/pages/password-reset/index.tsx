import { useResetPassword } from '@/features/identity/api/resetPassword'
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
import { useRouter } from 'next/router'
import { useState } from 'react'

const ChangePassword = () => {
  const [email, setEmail] = useState<string>('')
  const [invalidEmail, setInvalidEmail] = useState(false)
  const [showSnackbar, setShowSnackbar] = useState(false)
  const [snackbarMessage, setSnackbarMessage] = useState('')
  const [isSubmitted, setIsSubmitted] = useState(false)

  const { refetch, data } = useResetPassword({ email })
  const router = useRouter()

  const handleSnackbarClose = () => {
    setShowSnackbar(false)
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    // Email validation
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
    if (!emailRegex.test(email)) {
      setInvalidEmail(true)
      setShowSnackbar(false)
      return
    }

    setInvalidEmail(false)
    refetch()
    setIsSubmitted(true)
  }

  const handleHomeButtonClick = () => {
    router.push('/performance-measures')
  }

  return (
    <Container component="main" maxWidth="xs">
      <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
        Change Password
      </Typography>
      <Typography variant="body2" sx={{ textAlign: 'center', mt: 2 }}>
        {`Enter the email linked to your account. We'll send you a link to change your password.`}
      </Typography>
      <Box
        sx={{
          marginTop: 4,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        {isSubmitted ? (
          <>
            <Typography variant="h5" sx={{ textAlign: 'center', mt: 3 }}>
              Please check your email for the change password link
            </Typography>
            <Button
              
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              onClick={handleHomeButtonClick}
            >
              Go Home
            </Button>
          </>
        ) : (
          <Box
            component="form"
            noValidate
            onSubmit={handleSubmit}
            sx={{ mt: 3 }}
          >
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
        )}
      </Box>
      <Snackbar
        open={showSnackbar}
        autoHideDuration={6000}
        onClose={handleSnackbarClose}
      >
        <Alert onClose={handleSnackbarClose} severity="success">
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Container>
  )
}

export default ChangePassword
