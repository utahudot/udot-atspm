import Cookies from 'js-cookie'
import { useRouter } from 'next/router'
import { ReactNode, useEffect, useState } from 'react'

interface AuthorizedProps {
  requiredClaim: string
  children: ReactNode
}

const Authorization: React.FC<AuthorizedProps> = ({
  requiredClaim,
  children,
}) => {
  const router = useRouter()
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null)

  useEffect(() => {
    const loggedIn = Cookies.get('loggedIn')
    const claims = Cookies.get('claims')

    const hasClaimOrIsAdmin = (): boolean => {
      if (claims) {
        return claims.includes(requiredClaim) || claims === 'Admin'
      }
      return false
    }

    if (loggedIn) {
      if (hasClaimOrIsAdmin()) {
        setIsAuthorized(true)
      } else {
        router.push('/unauthorized')
        setIsAuthorized(false)
      }
    } else {
      router.push('/login')
      setIsAuthorized(false)
    }
  }, [requiredClaim, router])

  if (isAuthorized === null) {
    return null
  }

  return <>{isAuthorized ? children : null}</>
}

export default Authorization
