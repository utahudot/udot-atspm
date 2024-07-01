import { useVerifyUserHandler } from '@/features/identity/components/handlers/verifyUserHandler'
import { VerifyUserOptionsComponent } from '@/features/identity/components/verifyUser/verifyUserOptionsComponent'
import { isUserLoggedIn } from '@/utils/user'
import { Alert, Box, Container, Snackbar, Typography } from '@mui/material'
import { useEffect, useState } from 'react'

const VerifyUser = () => {
  const handler = useVerifyUserHandler()
  const [allowVerify, setAllowVerify] = useState(false)

  useEffect(() => {
    if (!isUserLoggedIn()) {
      window.location.href = '/unauthorized'
    } else {
      setAllowVerify(true)
    }
  }, [])

  if (!allowVerify) {
    return
  }

  return (
    <Box>
      <Container component="main" maxWidth="xs">
        <Typography variant="h1" fontWeight="bold" sx={{ textAlign: 'center' }}>
          Please verify be entering your password
        </Typography>
        <VerifyUserOptionsComponent handler={handler} />
      </Container>
      <Snackbar
        open={handler.responseError}
        autoHideDuration={6000}
        onClose={() => handler.handleResponseError(false)}
      >
        <Alert
          elevation={6}
          variant="filled"
          onClose={() => handler.handleResponseError(false)}
          severity="error"
        >
          {handler?.data?.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}
export default VerifyUser
