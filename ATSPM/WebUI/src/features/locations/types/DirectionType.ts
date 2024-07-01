export const southbound = 'Southbound'
export const northbound = 'Northbound'
export const westbound = 'Westbound'
export const eastbound = 'Eastbound'
export const northEast = 'NorthEast'
export const northWest = 'NorthWest'
export const southEast = 'SouthEast'
export const southWest = 'SouthWest'

export const directionList = [
  southbound,
  northbound,
  westbound,
  eastbound,
  northEast,
  northWest,
  southEast,
  southWest,
]

export type directionType =
  | 'Southbound'
  | 'Northbound'
  | 'Westbound'
  | 'Eastbound'

export interface DirectionType {
  description: string
  abbreviation: string
  displayOrder: number
  id: string
}
