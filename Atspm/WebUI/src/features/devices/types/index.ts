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

export interface DeviceConfiguration {
  id: number
  description?: string
  notes?: string
  protocol: string
  port: number
  path: string
  query: string[]
  connectionTimeout: number
  operationTimeout: number
  loggingOffset: number
  decoders: string[]
  userName: string
  password: string
  productId?: number
  product?: Product
  devices?: Device[]
  [key: string]: unknown
}

export interface Product {
  id: number | null
  manufacturer: string
  model: string
  notes?: string
  webPage?: string
}

export interface Device {
  id: number | null
  loggingEnabled: boolean
  ipaddress: string
  deviceStatus: string
  notes?: string
  locationId: string | null
  deviceConfigurationId: number | null
  deviceConfiguration?: DeviceConfiguration
  deviceType: string
}
