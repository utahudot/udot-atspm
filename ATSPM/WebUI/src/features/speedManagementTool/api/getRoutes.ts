import {
  AnalysisPeriod,
  DataSource,
} from '@/features/speedManagementTool/enums'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { useGetRequest } from '@/hooks/useGetRequest'
import { speedAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

export interface RouteParams {
  dataSource: DataSource
  start: string
  end: string
  daysOfWeek: string[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  customStartTime?: Date
  customEndTime?: Date
}

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})
const axiosInstance = speedAxios

export const useRoutes = ({ params }: { params: RouteParams }) => {
  const {
    start,
    end,
    analysisPeriod,
    daysOfWeek,
    customStartTime,
    customEndTime,
  } = params

  // Extract hours and minutes from customStartTime and customEndTime
  const customStartHour = customStartTime?.getHours()
  const customStartMinute = customStartTime?.getMinutes()
  const customEndHour = customEndTime?.getHours()
  const customEndMinute = customEndTime?.getMinutes()

  let url = `RouteSpeed/GetRouteSpeeds`
  url += `?startDate=${start}`
  url += `&endDate=${end}`
  url += `&analysisPeriod=${
    analysisPeriod === AnalysisPeriod.MorningPeak
      ? AnalysisPeriod.CustomHour
      : analysisPeriod
  }`
  url += `&violationThreshold=${params.violationThreshold}`
  url += `&sourceId=${params.dataSource}`
  url += `&daysOfWeek=${daysOfWeek}`
  if (analysisPeriod === AnalysisPeriod.CustomHour) {
    url += `&customStartHour=${customStartHour}`
    url += `&customStartMinute=${customStartMinute}`
    url += `&customEndHour=${customEndHour}`
    url += `&customEndMinute=${customEndMinute}`
  }
  if (analysisPeriod === AnalysisPeriod.MorningPeak) {
    url += `&customStartHour=10`
    url += `&customStartMinute=0`
    url += `&customEndHour=12`
    url += `&customEndMinute=0`
  }

  return useGetRequest<RoutesResponse>({
    route: url,
    axiosInstance,
    headers,
  })
}
