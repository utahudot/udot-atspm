export interface DeviceConfiguration {
  id: number
  firmware: string
  notes?: string
  protocol: string
  port: number
  directory: string
  searchTerms: string
  connectionTimeout: number
  operationTimeout: number
  dataModel: string
  userName: string
  password: string
  productId: number
  product?: Product
  productName?: string
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
