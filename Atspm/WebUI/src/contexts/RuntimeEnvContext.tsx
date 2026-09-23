import { createContext, useContext, type ReactNode } from 'react'
import type { EnvVariables } from '@/utils/getEnv'

const RuntimeEnvContext = createContext<EnvVariables | null>(null)

export function RuntimeEnvProvider({
  children,
  env,
}: {
  children: ReactNode
  env: EnvVariables
}) {
  return (
    <RuntimeEnvContext.Provider value={env}>
      {children}
    </RuntimeEnvContext.Provider>
  )
}

export function useRuntimeEnv(): EnvVariables {
  const env = useContext(RuntimeEnvContext)

  if (!env) {
    throw new Error('useRuntimeEnv must be used inside RuntimeEnvProvider')
  }

  return env
}
