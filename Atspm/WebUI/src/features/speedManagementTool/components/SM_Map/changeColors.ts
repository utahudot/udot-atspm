export const getChangeBackgroundColor = (value: unknown): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) return 'inherit'
  if (value > 0) return 'rgba(173, 216, 230, 0.3)'
  if (value < 0) return 'rgba(255, 143, 10, 0.3)'
  return 'inherit'
}
