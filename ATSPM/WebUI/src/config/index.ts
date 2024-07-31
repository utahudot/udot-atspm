// Check if window.__ENV is available, otherwise fallback to process.env
const env = typeof window !== 'undefined' ? window.__ENV : process.env

export const MOCKING_ENABLED = env.NEXT_PUBLIC_API_MOCKING === 'true'

export const CONFIG_URL = env.NEXT_PUBLIC_CONFIG_URL as string
export const REPORTS_URL = env.NEXT_PUBLIC_REPORTS_URL as string
export const IDENTITY_URL = env.NEXT_PUBLIC_IDENTITY_URL as string
export const DATA_URL = env.NEXT_PUBLIC_DATA_URL as string

export const MAP_DEFAULT_LATITUDE = parseFloat(
  env.NEXT_PUBLIC_MAP_DEFAULT_LATITUDE as string
)
export const MAP_DEFAULT_LONGITUDE = parseFloat(
  env.NEXT_PUBLIC_MAP_DEFAULT_LONGITUDE as string
)
