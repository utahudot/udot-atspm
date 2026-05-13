import type { NextApiRequest, NextApiResponse } from 'next'

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  const lat = typeof req.query.lat === 'string' ? req.query.lat : null
  const lng = typeof req.query.lng === 'string' ? req.query.lng : null
  if (!lat || !lng) return res.status(400).json({ available: false })

  const key = process.env.GOOGLE_MAPS_API_KEY
  if (!key) return res.status(500).json({ available: false })

  const url =
    `https://maps.googleapis.com/maps/api/streetview/metadata` +
    `?location=${encodeURIComponent(`${lat},${lng}`)}` +
    `&radius=50` +
    `&key=${encodeURIComponent(key)}`

  const r = await fetch(url)
  if (!r.ok) return res.status(200).json({ available: false })

  const data = (await r.json()) as { status?: string }
  return res.status(200).json({ available: data.status === 'OK' })
}
