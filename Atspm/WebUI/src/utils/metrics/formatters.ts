import { formatNumber } from '@/utils/numberFormat'

export function formatSpeed(value: number | null | undefined, decimals = 2) {
  if (value == null) return ''
  return `${formatNumber(value, decimals)} mph`
}

export function formatPercent(
  value: string | number | null | undefined,
  decimals = 2
) {
  if (value == null) return ''
  const numericValue = typeof value === 'string' ? Number(value) : value
  return `${formatNumber(numericValue * 100, decimals)}%`
}

export function formatCount(
  value: string | number | null | undefined,
  decimals = 0
) {
  if (value == null) return ''
  return `${formatNumber(value, decimals)}`
}
