import type { NextApiRequest, NextApiResponse } from 'next'

type SessionResponse = {
  session: string
  expiry?: string
}

export default async function handler(
  req: NextApiRequest,
  res: NextApiResponse
) {
  if (req.method !== 'POST') return res.status(405).end()

  const apiKey = process.env.GOOGLE_MAPS_API_KEY
  if (!apiKey)
    return res.status(500).json({ error: 'Missing GOOGLE_MAPS_API_KEY' })

  const mapType = process.env.GOOGLE_MAPS_MAPTYPE ?? 'roadmap'
  const language = process.env.GOOGLE_MAPS_LANGUAGE ?? 'en-US'
  const region = process.env.GOOGLE_MAPS_REGION ?? 'US'
  const r = await fetch(
    `https://tile.googleapis.com/v1/createSession?key=${encodeURIComponent(apiKey)}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ mapType, language, region }),
    }
  )

  if (!r.ok) {
    const text = await r.text()
    return res.status(r.status).send(text)
  }

  const data = (await r.json()) as { session: string; expiry?: string }
  const payload: SessionResponse = {
    session: data.session,
    expiry: data.expiry,
  }

  // You can cache the session response briefly; session itself is valid for a while.
  res.setHeader('Cache-Control', 'private, max-age=300')
  return res.status(200).json(payload)
}
