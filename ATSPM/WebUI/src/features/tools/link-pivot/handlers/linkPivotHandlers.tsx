import {
  RouteHandler,
  useRouteHandler,
} from '@/components/handlers/routeHandler'
import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import { startOfDay } from 'date-fns'
import { useState } from 'react'
import { LinkPivotAdjustmentOptions } from '../types'

interface LPContainerPresenterProps {
  handleToolOptions: (options: Partial<LinkPivotAdjustmentOptions>) => void
}

export interface LinkPivotHandler
  extends DateTimeProps,
    TimeOnlyProps,
    RouteHandler {
  selectedDays: number[]
  cycleLength: number
  startingPoint: StreamType
  bias: number
  biasDirection: StreamType
  routeId: string
  changeRouteId(routeId: string): void
  updateDaysOfWeek(daysOfWeek: number[]): void
  changeCycleLength(cycleLength: number): void
  changeStartingPoint(startingPoint: StreamType): void
  changeBias(bias: number): void
  changeBiasDirection(biasDirection: StreamType): void
}

export type StreamType = 'Upstream' | 'Downstream'

export const useLinkPivotOptionsHandler = ({
  handleToolOptions,
}: LPContainerPresenterProps) => {
  const [startDate, setStartDate] = useState(startOfDay(new Date()))
  const [endDate, setEndDate] = useState(startOfDay(new Date()))
  const [startTime, setStartTime] = useState(startOfDay(new Date()))
  const [endTime, setEndTime] = useState(startOfDay(new Date()))
  const [routeId, setRouteId] = useState<string>('')
  const [selectedDays, setSelectedDays] = useState<number[]>([1, 2, 3, 4, 5])
  const [cycleLength, setCycleLength] = useState(90)
  const [direction, setDirection] = useState<StreamType>('Downstream')
  const [bias, setBias] = useState(0)
  const [biasDirection, setBiasDirection] = useState<StreamType>('Downstream')

  const routeHandler = useRouteHandler()

  const componentHandler: LinkPivotHandler = {
    ...routeHandler,
    startDateTime: startDate,
    endDateTime: endDate,
    startTime,
    endTime,
    routeId,
    selectedDays,
    cycleLength,
    startingPoint: direction,
    bias,
    biasDirection,
    changeStartDate: (date: Date) => {
      const year = date.getUTCFullYear()
      const month = String(date.getUTCMonth() + 1).padStart(2, '0')
      const day = String(date.getUTCDate()).padStart(2, '0')

      const formattedDate = `${year}-${month}-${day}`
      handleToolOptions({ startDate: formattedDate })
      setStartDate(date)
    },
    changeEndDate: (date: Date) => {
      const year = date.getUTCFullYear()
      const month = String(date.getUTCMonth() + 1).padStart(2, '0')
      const day = String(date.getUTCDate()).padStart(2, '0')

      const formattedDate = `${year}-${month}-${day}`
      handleToolOptions({ endDate: formattedDate })
      setEndDate(date)
    },
    changeStartTime: (date: Date) => {
      const options: Intl.DateTimeFormatOptions = {
        hour12: false,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      }

      const formattedTime = date.toLocaleString('en-US', options)
      handleToolOptions({ startTime: formattedTime })
      setStartTime(date)
    },
    changeEndTime: (date: Date) => {
      const options: Intl.DateTimeFormatOptions = {
        hour12: false,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      }

      const formattedTime = date.toLocaleString('en-US', options)
      handleToolOptions({ endTime: formattedTime })
      setEndTime(date)
    },
    changeRouteId: (newRouteId: string) => {
      handleToolOptions({ routeId: newRouteId })
      setRouteId(newRouteId)
    },
    updateDaysOfWeek: (daysOfWeek: number[]) => {
      handleToolOptions({ daysOfWeek })
      setSelectedDays(daysOfWeek)
    },
    changeCycleLength: (cycleLength: number) => {
      handleToolOptions({ cycleLength })
      setCycleLength(cycleLength)
    },
    changeStartingPoint: (direction: StreamType) => {
      handleToolOptions({ direction })
      setDirection(direction)
    },
    changeBias: (bias: number) => {
      handleToolOptions({ bias })
      setBias(bias)
    },
    changeBiasDirection: (biasDirection: StreamType) => {
      handleToolOptions({ biasDirection })
      setBiasDirection(biasDirection)
    },
  }

  return componentHandler
}
