export enum ServiceType {
  FeatureServer = 'FeatureServer',
  MapServer = 'MapServer',
  WMS = 'WMS',
  WFS = 'WFS',
}

export interface MapLayer {
  [key: string]: unknown
  id?: number
  name: string
  mapLayerUrl?: string | null
  showByDefault: boolean
  serviceType: ServiceType | string
  refreshIntervalSeconds?: number | null
  resourceId?: string | null
  style?: string | null
  created?: string | null
  createdBy?: string | null
  modified?: string | null
  modifiedBy?: string | null
}

export type PersistedMapLayer = MapLayer & { id: number }
