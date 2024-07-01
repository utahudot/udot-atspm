export interface LTGRDataCheck {
  locationIdentifier: string
  start: string
  end: string
  daysOfWeek: number[]
  startHour: number
  startMinute: number
  endHour: number
  endMinute: number
}

export interface LTGRGapDuration {
  locationIdentifier: string
  approachId: number
  start: string
  end: string
  daysOfWeek: number[]
  startHour: number
  startMinute: number
  endHour: number
  endMinute: number
  acceptableGaps: number
}

export interface peakHours {
  start: Date
  end: Date
  locationIdentifier: string
  approachId: number
  daysOfWeek: number[]
}
