import NextImage from '@/components/NextImage'
import { useLogin } from '@/features/identity/api/getLogin'
import IdentityDto from '@/features/identity/types/identityDto'
import { setSecureCookie } from '@/features/identity/utils'
import { getEnv } from '@/utils/getEnv'
import { LoadingButton } from '@mui/lab'
import { Alert, Button, Divider } from '@mui/material'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import Link from '@mui/material/Link'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import * as React from 'react'
import { useEffect, useState } from 'react'

export default function Signin() {
  const [data, setData] = useState<IdentityDto>()
  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')
  const [errors, setErrors] = useState<string | null>(null)
  const [emailError, setEmailError] = useState<string | null>(null)
  const [passwordError, setPasswordError] = useState<string | null>(null)
  const [identityUrl, setIdentityUrl] = useState<string | null>(null)
  const {
    refetch,
    data: queryData,
    status,
    isLoading,
    error: queryDataError,
  } = useLogin({ email, password })

  useEffect(() => {
    if (queryData) {
      setData(queryData as IdentityDto)
    }
  }, [data, queryData])

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      setIdentityUrl(env?.IDENTITY_URL as string)
    }
    fetchEnv()
  }, [])

  const validateEmail = (email: string) => {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
    return emailRegex.test(email)
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    setErrors(null)
    event.preventDefault()

    let isValid = true

    if (!email) {
      setEmailError('Email is required')
      isValid = false
    } else if (!validateEmail(email)) {
      setEmailError('Invalid email format')
      isValid = false
    } else {
      setEmailError(null)
    }

    if (!password) {
      setPasswordError('Password is required')
      isValid = false
    } else {
      setPasswordError(null)
    }

    if (isValid) {
      refetch()
    }
  }

  useEffect(() => {
    setEmailError(null)
    if (queryDataError) {
      setErrors(queryDataError.response.data.message)
    }
  }, [queryDataError, email])

  useEffect(() => {
    setPasswordError(null)
  }, [password])

  if (status === 'success' && data !== undefined) {
    setSecureCookie('token', data.token)
    setSecureCookie('claims', data.claims.join(','))
    setSecureCookie('loggedIn', 'True')
    window.location.href = '/performance-measures'
  }

  const redirectUser = async () => {
    const env = await getEnv()
    const externalLoginUrl = `${identityUrl}/api/v1/Account/external-login`

    // Open the external login endpoint in a new tab
    window.open(externalLoginUrl, '_self')
  }

  return (
    <Container component="main" maxWidth="xs">
      <Box
        sx={{
          marginY: 6,
          marginX: 1,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Box width={'60%'} marginBottom={'10px'}>
          <NextImage path="/images/atspm-logo-new.png" alt="ATSPM" />
        </Box>
        <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
          <TextField
            margin="normal"
            required
            fullWidth
            id="email"
            label="Email Address"
            name="email"
            autoComplete="email"
            autoFocus
            onChange={(e) => setEmail(e.target.value)}
            error={!!emailError}
            helperText={emailError}
          />
          <TextField
            margin="normal"
            required
            fullWidth
            name="password"
            label="Password"
            type="password"
            id="password"
            autoComplete="current-password"
            onChange={(e) => setPassword(e.target.value)}
            error={!!passwordError}
            helperText={passwordError}
          />
          {errors && <Alert severity="error">{errors}</Alert>}
          {/* <FormControlLabel
            control={<Checkbox value="remember" color="primary" />}
            label="Remember me"
          /> */}
          <LoadingButton
            loading={isLoading}
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 3, mb: 2, p: 1 }}
          >
            Sign In
          </LoadingButton>
          <Divider>
            <Typography variant="caption">or</Typography>
          </Divider>
          <Button
            variant="outlined"
            sx={{ p: 1, mb: 1, mt: 1 }}
            fullWidth
            onClick={() => redirectUser()}
          >
            <Box sx={{ width: '20px', height: '20px', mr: 1 }}></Box>
            Sign in with Utah Id
          </Button>
          {/* <Button
            variant="outlined"
            sx={{ padding: '10px', marginBottom: '40px' }}
            fullWidth
          >
            <Box sx={{ width: '20px', height: '20px', marginRight: '10px' }}>
              <NextImage
                path="/images/github-mark.png"
                alt="log in with Google"
              />
            </Box>
            Sign in with Github
          </Button> */}
          <Grid container>
            <Grid item xs>
              <Link href="/password-reset" variant="body2">
                Forgot password?
              </Link>
            </Grid>
            <Grid item>
              <Link href="/register" variant="body2">
                {"Don't have an account? Sign Up"}
              </Link>
            </Grid>
          </Grid>
        </Box>
      </Box>
    </Container>
  )
}
