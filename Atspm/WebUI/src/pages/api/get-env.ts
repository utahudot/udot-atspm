// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - get-env.ts
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
import { EnvVariables } from '@/utils/getEnv'
import { NextApiRequest, NextApiResponse } from 'next'
export default function handler(
  req: NextApiRequest,
  res: NextApiResponse<EnvVariables>
) {
  res.status(200).json({
    CONFIG_URL: process.env.CONFIG_URL,
    REPORTS_URL: process.env.REPORTS_URL,
    IDENTITY_URL: process.env.IDENTITY_URL,
    DATA_URL: process.env.DATA_URL,
    SPEED_URL: process.env.SPEED_URL,
    MAP_DEFAULT_LATITUDE: process.env.MAP_DEFAULT_LATITUDE,
    MAP_DEFAULT_LONGITUDE: process.env.MAP_DEFAULT_LONGITUDE,
    MAP_TILE_LAYER: process.env.MAP_TILE_LAYER,
    MAP_TILE_ATTRIBUTION: process.env.MAP_TILE_ATTRIBUTION,
    SPONSOR_IMAGE_URL: process.env.POWERED_BY_IMAGE_URL,
    MAP_DEFAULT_ZOOM: process.env.MAP_DEFAULT_ZOOM,
  })
}
