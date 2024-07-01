import NextImage from '@/components/NextImage'
import { IDENTITY_URL } from '@/config'
import { useLogin } from '@/features/identity/api/getLogin'
import IdentityDto from '@/features/identity/types/identityDto'
import { LoadingButton } from '@mui/lab'
import { Button, Divider } from '@mui/material'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import Link from '@mui/material/Link'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { addDays } from 'date-fns'
import Cookies from 'js-cookie'
import * as React from 'react'
import { useEffect, useState } from 'react'

export default function Signin() {
  const [data, setData] = useState<IdentityDto>()
  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')
  const {
    refetch,
    data: queryData,
    status,
    isLoading,
  } = useLogin({ email, password })

  useEffect(() => {
    if (queryData) {
      console.log(queryData)
      setData(queryData as IdentityDto)
    }
  }, [data, queryData])

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    refetch()
  }

  if (status === 'success' && data !== undefined) {
    const oneDay = addDays(new Date(), 1)
    Cookies.set('token', data.token, {
      expires: oneDay,
      // httpOnly: true,
      secure: true,
      sameSite: 'strict',
    })
    Cookies.set('claims', data.claims.join(','), { expires: oneDay })
    Cookies.set('loggedIn', 'True', { expires: oneDay })
    window.location.href = '/locations'
  }

  const redirectUser = () => {
    const externalLoginUrl = `${IDENTITY_URL}Account/external-login`

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
          <NextImage path="/images/new-atspm-logo.png" alt="ATSPM" />
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
          />
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
              <Link href="/forgotpassword" variant="body2">
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
