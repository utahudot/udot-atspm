// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - index.ts
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
export const MOCKING_ENABLED = process.env.NEXT_PUBLIC_API_MOCKING === 'true'

export const CONFIG_URL = process.env.NEXT_PUBLIC_CONFIG_URL as string
export const REPORTS_URL = process.env.NEXT_PUBLIC_REPORTS_URL as string
export const IDENTITY_URL = process.env.NEXT_PUBLIC_IDENTITY_URL as string
export const DATA_URL = process.env.NEXT_PUBLIC_DATA_URL as string

export const MAP_DEFAULT_LATITUDE = parseFloat(
  process.env.NEXT_PUBLIC_MAP_DEFAULT_LATITUDE as string
)
export const MAP_DEFAULT_LONGITUDE = parseFloat(
  process.env.NEXT_PUBLIC_MAP_DEFAULT_LONGITUDE as string
)
