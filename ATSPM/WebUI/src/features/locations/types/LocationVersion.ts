type Jurisdiction = {
  countyParish: string | null
  name: string
  mpo: string
  otherPartners: string | null
  id: number
}

type Region = {
  description: string
  id: number
}

export type LocationVersion = {
  latitude: number
  longitude: number
  note: string
  primaryName: string
  locationIdentifier: string
  secondaryName: string
  jurisdictionId: number
  chartEnabled: boolean
  versionAction: string
  start: string
  pedsAre1to1: boolean
  locationTypeId: number
  regionId: number
  id: number
  jurisdiction: Jurisdiction
  region: Region
}
