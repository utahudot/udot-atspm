import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { IDENTITY_URL } from '@/config'
import { useLogin } from '@/features/identity/api/getLogin'
import IdentityDto from '@/features/identity/types/identityDto'
import {
  Alert,
  Box,
  Button,
  Container,
  Divider,
  Grid,
  Link,
  TextField,
  Typography,
} from '@mui/material'
import { addMinutes } from 'date-fns'
import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { useEffect, useState } from 'react'

function Login() {
  const [data, setData] = useState<IdentityDto>()
  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')
  const [error, setError] = useState<string>()
  const router = useRouter()

  const { refetch, data: queryData } = useLogin({ email, password })

  useEffect(() => {
    if (queryData) {
      console.log(queryData)
      setData(queryData as IdentityDto)
    }
  }, [data, queryData])

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    refetch()
  }

  useEffect(() => {
    const query = router.query
    const { error } = query
    if (error !== undefined || error !== null) {
      setError(error as string)
    }
  }, [router.query])

  if (data?.code === 200) {
    const inOneMinute = addMinutes(new Date(), 1)
    Cookies.set('token', data.token, {
      // httpOnly: true,
      secure: true,
      sameSite: 'strict',
    })
    Cookies.set('claims', data.claims.join(','))
    Cookies.set('loggedIn', 'True', { expires: inOneMinute })
    window.location.href = '/locations'
  }

  const redirectUser = () => {
    const externalLoginUrl = `${IDENTITY_URL}Account/external-login`
    window.open(externalLoginUrl, '_self')
  }

  return (
    <ResponsivePageLayout title={'login'} hideTitle>
      <Container component="main" maxWidth="xs">
        <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
          Login
        </Typography>
        <Box
          sx={{
            marginTop: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
          }}
        >
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
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  required
                  fullWidth
                  name="password"
                  label="Password"
                  type="password"
                  id="password"
                  autoComplete="new-password"
                  onChange={(e) => setPassword(e.target.value)}
                />
              </Grid>
            </Grid>
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
            >
              Sign In
            </Button>
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
            <Grid container justifyContent="space-between">
              <Grid item>
                <Link
                  style={{ color: 'cornflowerblue' }}
                  href="/forgotpassword"
                >
                  Forgot Password
                </Link>
              </Grid>
              <Grid item>
                <Link style={{ color: 'cornflowerblue' }} href="/register">
                  Dont have a Account? Sign Up
                </Link>
              </Grid>
            </Grid>
            {error?.length && (
              <Alert severity="error" sx={{ marginLeft: 1 }}>
                {error}
              </Alert>
            )}
          </Box>
        </Box>
      </Container>
    </ResponsivePageLayout>
  )
}

export default Login
