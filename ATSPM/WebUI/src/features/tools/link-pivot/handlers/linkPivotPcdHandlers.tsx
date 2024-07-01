import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import { toUTCDateStamp, toUTCDateWithTimeStamp } from '@/utils/dateTime'
import { addDays } from 'date-fns'
import { useState } from 'react'
import { LinkPivotComponentDto, LinkPivotPcdOptions } from '../types'
import { LinkPivotHandler } from './linkPivotHandlers'

export interface LinkPivotPcdHandler extends DateTimeProps, TimeOnlyProps {
  locationIdentifier: string
  downLocationIdentifier: string
  delta: number
  changeDelta(delta: number): void
  daysSelectList: string[]
}

interface LPPcdContainerPresenterProps {
  handleToolOptions: (options: Partial<LinkPivotPcdOptions>) => void
  pcdDto: LinkPivotComponentDto
  lpHandler: LinkPivotHandler
}

export const useLinkPivotPcdOptionsHandler = ({
  handleToolOptions,
  pcdDto,
  lpHandler,
}: LPPcdContainerPresenterProps) => {
  const [startDate, setStartDate] = useState(new Date(lpHandler.startDateTime))
  const [endDate, setEndDate] = useState(new Date(lpHandler.startDateTime))
  const [startTime, setStartTime] = useState(new Date(lpHandler.startTime))
  const [endTime, setEndTime] = useState(new Date(lpHandler.endTime))
  const [delta, setDelta] = useState(pcdDto.delta)

  const createTimeSelectValues = () => {
    const { startDateTime, endDateTime, startTime } = lpHandler
    const differenceMs = endDateTime.getTime() - startDateTime.getTime()
    let differenceDays = Math.abs(differenceMs / (1000 * 60 * 60 * 24))
    differenceDays = Math.floor(differenceDays)

    const daysSelectList: string[] = []

    for (let i = 0; i <= differenceDays; i++) {
      const date = addDays(startDateTime, i)
      const formattedDate = toUTCDateStamp(date)
      const formattedTime = toUTCDateWithTimeStamp(startTime)

      daysSelectList.push(`${formattedDate}T${formattedTime}`)
    }

    return daysSelectList
  }

  const componentHandler: LinkPivotPcdHandler = {
    daysSelectList: createTimeSelectValues(),
    startDateTime: startDate,
    endDateTime: endDate,
    startTime,
    endTime,
    locationIdentifier: pcdDto.locationIdentifier,
    downLocationIdentifier: pcdDto.downstreamLocationIdentifier,
    delta,
    changeDelta: (delta: number) => {
      handleToolOptions({ delta })
      setDelta(delta)
    },
    changeStartDate: (date: Date) => {
      const formattedDate = toUTCDateStamp(date)
      handleToolOptions({ startDate: formattedDate })
      setStartDate(date)
    },
    changeEndDate: (date: Date) => {
      const formattedDate = toUTCDateStamp(date)
      handleToolOptions({ endDate: formattedDate })
      setEndDate(date)
    },
    changeStartTime: (date: Date) => {
      const formattedTime = toUTCDateWithTimeStamp(date)
      handleToolOptions({ startTime: formattedTime })
      setStartTime(date)
    },
    changeEndTime: (date: Date) => {
      const formattedTime = toUTCDateWithTimeStamp(date)
      handleToolOptions({ endTime: formattedTime })
      setEndTime(date)
    },
  }

  return componentHandler
}
