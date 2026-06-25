// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - session.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
