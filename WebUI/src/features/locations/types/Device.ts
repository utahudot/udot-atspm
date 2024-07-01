export type DeviceConfiguration = {
  id: number
  firmware: string
  notes: string
  protocol: string
  port: number
  directory: string
  searchTerms: string
  username: string
  password: string
  productId: number
  product?: Product
}

export type Product = {
  id: number
  manufacturer: string
  model: string
  deviceType: string
  webPage: string
  notes: string
}

export enum DeviceType {
  Unknown,
  SignalController,
  RampController,
  AICamera,
  FIRCamera,
  LidarSensor,
  WavetronixSpeed,
}
