import { ApiKeyMetadata } from '@/features/identity/api/apiKeys'
import Cookies from 'js-cookie'

const assignableApiKeyClaims = new Set([
  'ApiKey:View',
  'ApiKey:Revoke',
  'Data:View',
  'Data:Edit',
  'Device:View',
  'Device:Edit',
  'Device:Delete',
  'GeneralConfiguration:View',
  'GeneralConfiguration:Edit',
  'GeneralConfiguration:Delete',
  'LocationConfiguration:View',
  'LocationConfiguration:Edit',
  'LocationConfiguration:Delete',
  'Report:View',
  'Role:View',
  'Role:Edit',
  'Role:Delete',
  'Usage:View',
  'Usage:Edit',
  'Usage:Delete',
  'User:View',
  'User:Edit',
  'User:Delete',
  'Watchdog:View',
  'SpeedConfigurations:Edit',
  'SpeedConfigurations:Delete',
])

const forbiddenApiKeyClaims = new Set(['Admin', 'ApiKey:Create'])

export const getCookieClaims = () =>
  Cookies.get('claims')
    ?.split(',')
    .map((claim) => claim.trim())
    .filter(Boolean) ?? []

export const getAssignableApiKeyClaims = (claims: string[]) => {
  return Array.from(new Set(claims))
    .filter((claim) => !forbiddenApiKeyClaims.has(claim))
    .filter((claim) => assignableApiKeyClaims.has(claim))
    .sort()
}

export const formatDate = (value?: string | null) => {
  if (!value) return 'None'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return date.toLocaleString()
}

export const formatOwner = (key: ApiKeyMetadata) => {
  if (key.ownerName && key.ownerEmail) {
    return `${key.ownerName} (${key.ownerEmail})`
  }

  return key.ownerName || key.ownerEmail || key.ownerId
}

export const getErrorMessage = (error: unknown) => {
  if (
    error &&
    typeof error === 'object' &&
    'response' in error &&
    error.response &&
    typeof error.response === 'object' &&
    'data' in error.response
  ) {
    const data = error.response.data as { detail?: string; title?: string }
    return data.detail || data.title
  }

  return undefined
}
