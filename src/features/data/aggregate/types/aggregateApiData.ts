import { ExpandLocationForAggregation } from '../handlers/expandLocationHandler'

export interface AggregateApiData {
  locationIdentifiers: string[]
  start: string
  end: string
  aggregationType: number
  dataType: number
  timeOptions: AggregateTimeOptions
  selectedAggregationType: number
  selectedXAxisType: number
  selectedSeries: number
  locations: ExpandLocationForAggregation[]
  filterDirections: AggregateFilterDirection[]
  filterMovements: AggregateFilterMovement[]
}

export interface AggregateTimeOptions {
  start: string
  end: string
  timeOfDayStartHour: number
  timeOfDayStartMinute: number
  timeOfDayEndHour: number
  timeOfDayEndMinute: number
  daysOfWeek: number[]
  timeOption: number
  selectedBinSize: number
}

export interface AggregateFilterDirection {
  directionTypeId: number
  description: string
  include: boolean
}

export interface AggregateFilterMovement {
  movementTypeId: number
  description: string
  include: boolean
}
