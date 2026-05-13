import type { NextApiRequest, NextApiResponse } from 'next'

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  const { z, x, y } = req.query
  const session =
    typeof req.query.session === 'string' ? req.query.session : undefined

  if (!session) return res.status(400).json({ error: 'Missing session' })

  const apiKey = process.env.GOOGLE_MAPS_API_KEY
  if (!apiKey)
    return res.status(500).json({ error: 'Missing GOOGLE_MAPS_API_KEY' })

  const url =
    `https://tile.googleapis.com/v1/2dtiles/${z}/${x}/${y}` +
    `?session=${encodeURIComponent(session)}` +
    `&key=${encodeURIComponent(apiKey)}`

  const r = await fetch(url)

  if (!r.ok) {
    const text = await r.text()
    return res.status(r.status).send(text)
  }

  // Forward content-type (png/jpg)
  const contentType = r.headers.get('content-type') ?? 'image/png'
  res.setHeader('Content-Type', contentType)

  // Reasonable caching for tiles (tune as you want)
  res.setHeader('Cache-Control', 'public, max-age=86400, s-maxage=86400')

  const buf = Buffer.from(await r.arrayBuffer())
  return res.status(200).send(buf)
}
