import { Box } from '@mui/material'
import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { useEffect } from 'react'

const SsoLogin = () => {
  const router = useRouter()

  useEffect(() => {
    const query = router.query
    const { token, claims, error: callError } = query
    console.log(claims)
    if (token) {
      Cookies.set('token', token as string, {
        secure: true,
        sameSite: 'strict',
      })
      Cookies.set('claims', claims?.length ? (claims as string) : '')
      Cookies.set('loggedIn', 'True')

      // Redirect to another page after setting cookies
      window.location.href = '/locations' // Replace '/another-page' with your desired path
    } else if (callError) {
      window.location.href = `/login?error=${callError}`
    }
  }, [router])

  return <Box></Box>
}

export default SsoLogin
