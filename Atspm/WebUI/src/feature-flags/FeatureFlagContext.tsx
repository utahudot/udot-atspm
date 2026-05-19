// src/FeatureFlagContext.tsx
import { createContext, useContext, useEffect, useState } from 'react'
import { Flags, loadFlags } from './featureFlags'

const FeatureFlagContext = createContext<Flags | null>(null)

export const FeatureFlagProvider = ({
  children,
}: {
  children: React.ReactNode | React.ReactNode[]
}) => {
  const [flags, setFlags] = useState<Flags | null>(null)

  useEffect(() => {
    loadFlags().then(setFlags)
  }, [])

  if (!flags) return <div>Loading…</div>

  return (
    <FeatureFlagContext.Provider value={flags}>
      {children}
    </FeatureFlagContext.Provider>
  )
}

export const useFlags = (): Flags => {
  const ctx = useContext(FeatureFlagContext)
  if (!ctx) throw new Error('useFlags must be inside FeatureFlagProvider')
  return ctx
}
