export type DiscrepancyKind =
  | 'NOT_FOUND_APP'
  | 'NOT_FOUND_DET'
  | 'FOUND_PHASE'
  | 'FOUND_DET'

export const dk = (kind: DiscrepancyKind, raw: string | number) =>
  `${kind}:${raw}`

export const parseKey = (key: string) => {
  const i = key.indexOf(':')
  if (i === -1) return null
  return { kind: key.slice(0, i) as DiscrepancyKind, raw: key.slice(i + 1) }
}

export const parseApproachIdFromKey = (key: string) => {
  const p = parseKey(key)
  if (!p || p.kind !== 'NOT_FOUND_APP') return null
  const id = Number(p.raw)
  return Number.isFinite(id) ? id : null
}
