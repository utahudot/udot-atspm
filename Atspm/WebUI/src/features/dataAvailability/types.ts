export interface CalendarDayLocationAvailability {
  locationIdentifier: string
  hasData: boolean
}

export interface CalendarDayAvailability {
  date: Date
  availableLocationCount: number
  totalLocationCount: number
  locations: CalendarDayLocationAvailability[]
}
