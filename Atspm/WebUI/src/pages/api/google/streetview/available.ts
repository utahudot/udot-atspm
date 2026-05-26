// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - available.ts
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
