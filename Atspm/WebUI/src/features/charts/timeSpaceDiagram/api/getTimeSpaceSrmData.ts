import {
  TimeSpaceSrmOptions,
  TimeSpaceSrmPhaseOverlay,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { reportsRequest } from '@/lib/axios'
import { dateToTimestamp } from '@/utils/dateTime'
import { useMutation } from 'react-query'

export const getTimeSpaceSrmData = async (
  options: TimeSpaceSrmOptions
): Promise<TimeSpaceSrmPhaseOverlay[]> => {
  const payload = {
    ...options,
    routeId: Number(options.routeId),
    start: dateToTimestamp(options.start),
    end: dateToTimestamp(options.end),
  }

  return reportsRequest<TimeSpaceSrmPhaseOverlay[]>({
    url: '/TimeSpaceDiagram/getSrmData',
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    data: payload,
  })
}

export const useTimeSpaceSrmData = () => {
  return useMutation(getTimeSpaceSrmData)
}
