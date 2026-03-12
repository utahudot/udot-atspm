export function formatBytes(bytes: number) {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 B'
  const k = 1024
  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.min(
    units.length - 1,
    Math.floor(Math.log(bytes) / Math.log(k))
  )
  const v = bytes / Math.pow(k, i)
  const digits = v >= 100 ? 0 : v >= 10 ? 1 : 2
  return `${v.toFixed(digits)} ${units[i]}`
}

export function formatMs(ms: number) {
  if (!Number.isFinite(ms)) return ''
  if (ms < 1000) return `${ms} ms`
  const s = ms / 1000
  if (s < 60) return `${s.toFixed(2)} s`
  const m = Math.floor(s / 60)
  const r = s - m * 60
  return `${m}m ${r.toFixed(1)}s`
}
