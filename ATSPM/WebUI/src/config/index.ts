export const MOCKING_ENABLED = process.env.NEXT_PUBLIC_API_MOCKING === 'true'

export const CONFIG_URL = process.env.NEXT_PUBLIC_CONFIG_URL as string
export const REPORTS_URL = process.env.NEXT_PUBLIC_REPORTS_URL as string
export const IDENTITY_URL = process.env.NEXT_PUBLIC_IDENTITY_URL as string
export const DATA_URL = process.env.NEXT_PUBLIC_DATA_URL as string

export const MAP_DEFAULT_LATITUDE = parseFloat(
  process.env.NEXT_PUBLIC_MAP_DEFAULT_LATITUDE as string
)
export const MAP_DEFAULT_LONGITUDE = parseFloat(
  process.env.NEXT_PUBLIC_MAP_DEFAULT_LONGITUDE as string
)
