// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getEnv.ts
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
import axios from 'axios'
export interface EnvVariables {
  CONFIG_URL: string | undefined
  REPORTS_URL: string | undefined
  IDENTITY_URL: string | undefined
  DATA_URL: string | undefined
  SPEED_URL: string | undefined
  MAP_DEFAULT_LATITUDE: string | undefined
  MAP_DEFAULT_LONGITUDE: string | undefined
  MAP_DEFAULT_ZOOM:string | undefined
  MAP_TILE_LAYER: string | undefined
  MAP_TILE_ATTRIBUTION: string | undefined
  SPONSOR_IMAGE_URL: string | undefined
  MAP_DEFAULT_ZOOM: string | undefined
}
let cachedEnv: EnvVariables | null = null
export const getEnv = async (): Promise<EnvVariables | null> => {
  if (cachedEnv) {
    return cachedEnv
  }
  try {
    const response = await axios.get('/api/get-env')
    cachedEnv = response.data
    return cachedEnv
  } catch (error) {
    console.error('Failed to load environment variables:', error)
    throw error
  }
}
