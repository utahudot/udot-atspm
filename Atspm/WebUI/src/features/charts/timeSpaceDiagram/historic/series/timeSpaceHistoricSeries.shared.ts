import { dateToTimestamp } from '@/utils/dateTime'

export const PASSIVE_DETECTION_SERIES_PROPS = {
  silent: true,
  tooltip: { show: false },
} as const

export function getSpeedInFeetPerSecond(speed: number): number {
  return (speed * 5280) / 3600
}

export function getArrivalTime(
  distanceToNextLocation: number,
  speed: number,
  currentDetectorOn: Date | string
): string {
  const start = new Date(currentDetectorOn)
  const speedInFeetPerSecond = getSpeedInFeetPerSecond(speed)
  const timeToTravelSeconds = distanceToNextLocation / speedInFeetPerSecond

  const arrivalMs = start.getTime() + timeToTravelSeconds * 1000

  return dateToTimestamp(new Date(arrivalMs))
}
