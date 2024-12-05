import { setSecureCookie } from '@/features/identity/utils'
import { Box } from '@mui/material'
import { useRouter } from 'next/router'
import { useEffect } from 'react'

const SsoLogin = () => {
  const router = useRouter()

  useEffect(() => {
    const query = router.query
    const { token, claims, error: callError } = query
    if (token) {
      setSecureCookie('token', token as string)
      setSecureCookie('claims', claims?.length ? (claims as string) : '')
      setSecureCookie('loggedIn', 'True')

      // Redirect to another page after setting cookies
      window.location.href = '/' // Replace '/another-page' with your desired path
    } else if (callError) {
      window.location.href = `/login?error=${callError}`
    }
  }, [router])

  return <Box></Box>
}

export default SsoLogin
