import { Area } from '@/features/areas/types'
import { Device } from '@/features/devices/types'
import { Jurisdiction } from '@/features/jurisdictions/types'
import { Approach, LocationType } from '@/features/locations/types'
import { Region } from '@/features/regions/types'

export type Location = {
  areas: number[]
  latitude: number
  longitude: number
  primaryName: string
  locationIdentifier: string
  secondaryName: string
  jurisdictionId: number
  locationTypeId: number
  regionId: number
  chartEnabled: boolean
  charts: number[]
  start: string
  pedsAre1to1: boolean
  id: string
  note: string
  approaches?: Approach[]
  jurisdiction?: Jurisdiction
  region?: Region
}

export type LocationExpanded = Omit<Location, 'areas' | 'charts'> & {
  areas: Area[]
  approaches: Approach[]
  jurisdiction: Jurisdiction
  region: Region
  locationType: LocationType
  devices: Device[]
  note: string
  locationTypeId: number
}
