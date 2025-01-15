import { getEnv } from '@/utils/getEnv'
import { useEffect, useState } from 'react'

export const useEnv = () => {
  const [env, setEnv] = useState(null)

  useEffect(() => {
    const fetchEnv = async () => {
      const environment = await getEnv()
      setEnv(environment)
    }
    fetchEnv()
  }, [])

  return env
}
