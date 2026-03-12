// features/data/components/ReportApiInsightsCard/utils.ts
import { UsageEntry } from '@/api/config'
import { IdentityUser } from '@/features/data/components/ReportApiAgencyCard'
import { addSpaces } from '@/utils/string'
import { GroupBy, Metric, SortBy } from './InsightsHeader'

export type InsightRow = { name: string; count: number; sortKey?: string }

function toDateKey(d: Date, groupBy: GroupBy) {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')

  if (groupBy === 'day') return `${y}-${m}-${day}`
  if (groupBy === 'month') return `${y}-${m}`

  const oneJan = new Date(d.getFullYear(), 0, 1)
  const week =
    Math.ceil(
      ((d.getTime() - oneJan.getTime()) / 86400000 + oneJan.getDay() + 1) / 7
    ) || 1

  return `${y}-W${String(week).padStart(2, '0')}`
}

export function normalizeUsers(users: any): IdentityUser[] {
  if (!users) return []
  if (Array.isArray(users)) return users as IdentityUser[]
  if (typeof users === 'object') return Object.values(users) as IdentityUser[]
  return []
}

function getUserLabel(u?: IdentityUser | null) {
  if (!u) return 'Unknown'
  return u.fullName?.trim() || u.email?.trim() || u.userName?.trim() || u.userId
}

function metricValue(r: UsageEntry, metric: Metric): number {
  if (metric === 'ReportsGenerated') return 1

  // UsageEntry can be inconsistent; keep it defensive.
  const anyR = r as any
  const v =
    anyR?.resultSizeBytes ??
    anyR?.ResultSizeBytes ??
    anyR?.responseSizeBytes ??
    anyR?.ResponseSizeBytes ??
    0

  const n = typeof v === 'number' ? v : Number(v)
  return Number.isFinite(n) ? n : 0
}

function applySort(rows: InsightRow[], sortBy: SortBy) {
  // Keep your existing behavior:
  // - Name: name desc
  // - Amount: count asc
  return [...rows].sort((a, b) =>
    sortBy === 'Name' ? b.name.localeCompare(a.name) : a.count - b.count
  )
}

export function buildByChartType(
  metricRows: UsageEntry[],
  opts: { maxBars: number; sortBy: SortBy; metric: Metric }
): InsightRow[] {
  const m = new Map<string, number>()

  for (const r of metricRows) {
    const keyRaw =
      opts.metric === 'ReportsGenerated'
        ? r.controller || 'Unknown'
        : getDataApiTypeFromRoute((r as any).route ?? (r as any).Route)

    const key = addSpaces(keyRaw || 'Unknown')

    m.set(key, (m.get(key) ?? 0) + metricValue(r, opts.metric))
  }

  const rows = [...m.entries()].map(([name, count]) => ({ name, count }))
  return applySort(rows, opts.sortBy).slice(0, opts.maxBars)
}

function pad2(n: number) {
  return String(n).padStart(2, '0')
}

export function toISODate(d: Date) {
  return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`
}

// Monday-start week (more intuitive than W05)
export function startOfWeekMonday(d: Date) {
  const x = new Date(d)
  x.setHours(0, 0, 0, 0)
  const day = x.getDay() // 0 Sun ... 6 Sat
  const diff = (day + 6) % 7 // Monday=0, Sunday=6
  x.setDate(x.getDate() - diff)
  return x
}

function formatMD(d: Date) {
  return `${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`
}

export function weekRangeLabel(weekStart: Date) {
  const endExclusive = new Date(weekStart)
  endExclusive.setDate(endExclusive.getDate() + 7)
  return `${formatMD(weekStart)} – ${formatMD(endExclusive)}`
}

function getDataApiTypeFromRoute(route?: string | null) {
  if (!route) return 'Unknown'

  // Drop querystring, trim slashes
  const path = route.split('?')[0].replace(/\/+$/, '')

  // Split and take the last segment
  const parts = path.split('/').filter(Boolean)
  if (parts.length === 0) return 'Unknown'

  return parts[parts.length - 1] || 'Unknown'
}

export function buildByTime(
  metricRows: UsageEntry[],
  opts: { groupBy: GroupBy; metric: Metric }
): InsightRow[] {
  const m = new Map<string, number>()

  for (const r of metricRows) {
    if (!r.timestamp) continue
    const d = new Date(r.timestamp)
    if (Number.isNaN(d.getTime())) continue

    if (opts.groupBy === 'week') {
      const s = startOfWeekMonday(d)
      const key = toISODate(s) // stable sort key
      m.set(key, (m.get(key) ?? 0) + metricValue(r, opts.metric))
      continue
    }

    const key = toDateKey(d, opts.groupBy) // day/month as before
    m.set(key, (m.get(key) ?? 0) + metricValue(r, opts.metric))
  }

  if (opts.groupBy === 'week') {
    return [...m.entries()]
      .map(([weekStartIso, count]) => {
        const s = new Date(`${weekStartIso}T00:00:00`)
        return { name: weekRangeLabel(s), count, sortKey: weekStartIso }
      })
      .sort((a, b) => (a.sortKey || '').localeCompare(b.sortKey || ''))
  }

  return [...m.entries()]
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => a.name.localeCompare(b.name))
}

export function buildByUser(
  reportRows: UsageEntry[],
  opts: {
    userById: Map<string, IdentityUser>
    maxBars: number
    sortBy: SortBy
    metric: Metric
  }
): InsightRow[] {
  const m = new Map<string, number>()
  for (const r of reportRows) {
    const uid = r.userId || ''
    const u = uid ? opts.userById.get(uid) : undefined
    const label = uid ? getUserLabel(u) : 'Unknown'
    m.set(label, (m.get(label) ?? 0) + metricValue(r, opts.metric))
  }

  const rows = [...m.entries()].map(([name, count]) => ({ name, count }))
  return applySort(rows, opts.sortBy).slice(0, opts.maxBars)
}

export function buildByAgency(
  reportRows: UsageEntry[],
  opts: {
    userById: Map<string, IdentityUser>
    maxBars: number
    sortBy: SortBy
    metric: Metric
  }
): InsightRow[] {
  const m = new Map<string, number>()
  for (const r of reportRows) {
    const uid = r.userId || ''
    const u = uid ? opts.userById.get(uid) : undefined
    const agency = u?.agency?.trim() || 'Unknown'
    m.set(agency, (m.get(agency) ?? 0) + metricValue(r, opts.metric))
  }

  const rows = [...m.entries()].map(([name, count]) => ({ name, count }))
  return applySort(rows, opts.sortBy).slice(0, opts.maxBars)
}
