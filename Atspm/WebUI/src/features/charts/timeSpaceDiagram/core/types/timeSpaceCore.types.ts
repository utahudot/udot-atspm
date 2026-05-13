export interface TimeSpaceCoreRow {
  start: string
  end: string
  locationIdentifier: string
  locationDescription: string
  approachDescription: string
  phaseType: 'Primary' | 'Opposing'
  distanceToNextLocation: number
  calculatedDistanceToNext: number
  calculatedDistanceToPrevious: number
  isIgnoredLocation: boolean
  speed: number
  cycleLength: number | null
  order?: number
}

export interface TimeSpacePhaseLayout<T extends TimeSpaceCoreRow> {
  primaryDirection: string
  opposingDirection: string
  primaryPhaseData: T[]
  opposingPhaseData: T[]
  rawDistanceData: number[]
  distanceScale: number
  locationCenterDistanceData: number[]
  primaryDistanceData: number[]
  opposingDistanceData: number[]
  minDisplayDistance: number
  maxDisplayDistance: number
  chartHeight: number
}
