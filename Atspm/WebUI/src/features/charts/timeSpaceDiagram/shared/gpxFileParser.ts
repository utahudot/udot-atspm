export type GpxPoint = {
  time: string
  distance: number
}

export const parseGpxFile = async (file: File): Promise<GpxPoint[]> => {
  const text = await file.text()
  const xml = new DOMParser().parseFromString(text, 'application/xml')

  const trkpts = Array.from(xml.getElementsByTagName('trkpt'))

  let lastLat: number | null = null
  let lastLon: number | null = null
  let totalDistance = 0

  const startTime = trkpts[0]?.getElementsByTagName('time')[0]?.textContent

  if (!startTime) return []

  const startMs = new Date(startTime).getTime()

  const points: GpxPoint[] = []
  for (const pt of trkpts) {
    const lat = Number(pt.getAttribute('lat'))
    const lon = Number(pt.getAttribute('lon'))
    const timeStr = pt.getElementsByTagName('time')[0]?.textContent
    if (!timeStr) continue

    const filtered = timeStr?.replace(/Z$/, '')
    // const t = new Date(timeStr).getTime()

    if (lastLat != null && lastLon != null) {
      totalDistance += haversine(lastLat, lastLon, lat, lon)
    }

    points.push({
      time: filtered,
      distance: totalDistance,
    })

    lastLat = lat
    lastLon = lon
  }
  return points.slice(0, 500)
}

const haversine = (lat1: number, lon1: number, lat2: number, lon2: number) => {
  const R = 6371000
  const toRad = (d: number) => (d * Math.PI) / 180

  const dLat = toRad(lat2 - lat1)
  const dLon = toRad(lon2 - lon1)

  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2

  return 2 * R * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}
