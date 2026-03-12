export function roundTo(
  value: number | null | undefined,
  decimals: number
): number | null {
  if (value == null) return null
  const factor = 10 ** decimals
  return Math.round(value * factor) / factor
}

export function formatNumber(
  value: number | string | null | undefined,
  decimals = 0
): string {
  if (value == null) return ''

  const numeric = typeof value === 'number' ? value : Number(String(value))

  if (!Number.isFinite(numeric)) return ''

  if (decimals === 0) {
    return String(Math.round(numeric))
  }

  return numeric.toFixed(decimals)
}
