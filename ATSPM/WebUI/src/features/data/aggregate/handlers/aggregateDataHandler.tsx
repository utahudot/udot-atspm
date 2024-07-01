import {
  LocationHandler,
  useLocationHandler,
} from '@/components/handlers/locationHandler'
import {
  RouteHandler,
  useRouteHandler,
} from '@/components/handlers/routeHandler'
import { useGetLocation } from '@/features/locations/api'
import { LocationExpanded } from '@/features/locations/types'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import { dateToTimestamp } from '@/utils/dateTime'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'
import { usePostAggregateData } from '../../api/getAggregateData'
import {
  AggregateApiData,
  AggregateFilterDirection,
  AggregateFilterMovement,
  AggregateTimeOptions,
} from '../types/aggregateApiData'
import { AggregateData } from '../types/aggregateData'
import {
  AggregationType,
  MetricTypeOptionsList,
  YAxisOptions,
  binSizeMarks,
  chartTypeOptions,
  xAxisOptions,
} from '../types/aggregateOptionsData'
import {
  ExpandLocationHandler,
  useExpandLocationHandler,
} from './expandLocationHandler'

export interface AggregateOptionsHandler
  extends DateTimeProps,
    TimeOnlyProps,
    RouteHandler,
    LocationHandler,
    ExpandLocationHandler {
  aggregatedData: AggregateData[]
  selectedDays: number[]
  selectedDirections: number[]
  selectedMovements: number[]
  metricType: string
  binSize: number
  averageOrSum: number
  xAxisType: number
  yAxisType: number
  detectionType: string
  visualChartType: string
  changeSelectedDays(days: number[]): void
  changeSelectedDirections(directions: number[]): void
  changeSelectedMovements(movements: number[]): void
  changeMetricType(metricType: string): void
  changeBinSize(binSize: number): void
  changeAverageOrSum(value: number): void
  changeXAxisType(value: number): void
  changeYAxisType(value: number): void
  changeDetectionType(value: string): void
  changeVisualChartType(value: 'line' | 'bar' | 'pie'): void
  handleRunAnalysis(): void
}

export const useAggregateOptionsHandler = (): AggregateOptionsHandler => {
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const [startTime, setStartTime] = useState(
    new Date(new Date().setHours(0, 0, 0, 0))
  )
  const [endTime, setEndTime] = useState(
    new Date(new Date().setHours(0, 0, 0, 0))
  )
  const [selectedDays, setSelectedDays] = useState<number[]>([1, 2, 3, 4, 5])
  const [selectedDirections, setSelectedDirections] = useState<number[]>([
    0, 1, 2, 3, 4, 5, 6, 7,
  ])
  const [selectedMovements, setSelectedMovements] = useState<number[]>([
    0, 1, 2, 3, 4, 5, 6, 7,
  ])
  const [metricType, setMetricType] = useState<string>('')
  const [xAxisType, setXAxisType] = useState<number>(xAxisOptions[0].id)
  const [yAxisType, setYAxisType] = useState<number>(YAxisOptions[0].id)
  const [detectionType, setDetectionType] = useState<string>('')
  const [visualChartType, setVisualChartType] = useState<
    'line' | 'bar' | 'pie'
  >(chartTypeOptions[0].id)
  const [binSize, setBinSize] = useState<number>(binSizeMarks[0].value)
  const [locationId, setLocationId] = useState<string>('')
  const [locationIds, setLocationIds] = useState<string[]>([])
  const [averageOrSum, setAverageOrSum] = useState<number>(0)
  const [selectedLocations, setSelectedLocations] = useState<
    LocationExpanded[]
  >([])
  const [aggregatedData, setAggregatedData] = useState<AggregateData[]>([])
  const [routeExpandedLocations, setRouteExpandedLocations] = useState<
    LocationExpanded[]
  >([])
  const {
    data: locationExpandedData,
    refetch: refetchLocationExpanded,
    status,
  } = useGetLocation(locationId)
  const postMutation = usePostAggregateData()
  const routeHandler = useRouteHandler()
  const {
    data: routeWithExpandedLocations,
    refetch: refectRouteExpanded,
    status: routeStatus,
  } = useGetRouteWithExpandedLocations({
    routeId: routeHandler.routeId,
    includeLocationDetail: true,
  })
  const locationHandler = useLocationHandler()
  const expandedLocationsHandler = useExpandLocationHandler({
    locations: selectedLocations,
    setSelectedLocations,
    changeLocation: locationHandler.changeLocation,
  })

  const getAggregateTypeEnumValue = (enumString: string): number => {
    if (Object.values(AggregationType).includes(enumString)) {
      // Type assertion to tell TypeScript that enumString is a valid member of AggregationType
      return AggregationType[enumString as keyof typeof AggregationType]
    }
    return 0
  }

  const getDataTypeValue = (aggregateVal: string, dataVal: string): number => {
    let dataTypeIndex = 0
    MetricTypeOptionsList.forEach(
      (metricType) =>
        metricType.id === aggregateVal &&
        metricType.options.forEach((option, index) => {
          if (option.id === dataVal) {
            dataTypeIndex = index
          }
        })
    )
    return dataTypeIndex
  }

  const createTimeOptions = (): AggregateTimeOptions => {
    return {
      start: dateToTimestamp(startDateTime),
      end: dateToTimestamp(endDateTime),
      timeOfDayStartHour: startTime.getHours(),
      timeOfDayStartMinute: startTime.getMinutes(),
      timeOfDayEndHour: endTime.getHours(),
      timeOfDayEndMinute: endTime.getMinutes(),
      daysOfWeek: selectedDays,
      timeOption: 0,
      selectedBinSize: binSize,
    }
  }

  const createFilterDirections = (): AggregateFilterDirection[] => {
    return selectedDirections.map((directionId) => {
      return {
        directionTypeId: directionId,
        description: 'string',
        include: true,
      }
    })
  }

  const createFilterMovements = (): AggregateFilterMovement[] => {
    return selectedMovements.map((movementId) => {
      return {
        movementTypeId: movementId,
        description: 'string',
        include: true,
      }
    })
  }

  const createAggregateObject = (): AggregateApiData => {
    const locationIdentifiers = selectedLocations.map(
      (location) => location.locationIdentifier
    )
    const metric = metricType.split('-')
    const aggregationType = getAggregateTypeEnumValue(metric[0])
    const dataType = getDataTypeValue(metric[0], metric[1])

    const timeOptions = createTimeOptions()
    const filterDirections = createFilterDirections()
    const filterMovements = createFilterMovements()

    return {
      locationIdentifiers,
      start: dateToTimestamp(startDateTime),
      end: dateToTimestamp(endDateTime),
      aggregationType,
      dataType,
      timeOptions,
      selectedAggregationType: averageOrSum,
      selectedXAxisType: xAxisType,
      selectedSeries: yAxisType,
      locations: expandedLocationsHandler.updatedLocations,
      filterDirections,
      filterMovements,
    }
  }

  const handleSubmit = async () => {
    const aggregateObject: AggregateApiData = createAggregateObject()
    try {
      const result: AggregateData[] = (await postMutation.mutateAsync(
        aggregateObject
      )) as unknown as AggregateData[]
      setAggregatedData(result)
    } catch (e) {
      console.log('error')
    }
  }

  useEffect(() => {
    if (routeHandler.routeId) {
      console.log('Route is present')
      refectRouteExpanded()
    }
  }, [refectRouteExpanded, routeHandler.routeId])

  useEffect(() => {
    if (selectedLocations && selectedLocations.length !== locationIds.length) {
      setLocationIds(selectedLocations.map((location) => location.id))
    }
  }, [locationIds.length, selectedLocations])

  useEffect(() => {
    if (routeExpandedLocations && locationIds) {
      const filteredExpandedLocations = routeExpandedLocations.filter(
        (location) => !locationIds.includes(location.id)
      )
      setSelectedLocations((prev) => [...prev, ...filteredExpandedLocations])
      setRouteExpandedLocations([])
    }
  }, [locationIds, routeExpandedLocations])

  useEffect(() => {
    if (routeStatus === 'success' && routeWithExpandedLocations) {
      console.log('IN here with route expanded location')
      const locationsExpandedData = routeWithExpandedLocations.routeLocations
      setRouteExpandedLocations(locationsExpandedData)
    }
  }, [routeStatus, routeWithExpandedLocations])

  useEffect(() => {
    if (locationHandler.location) {
      setLocationId(locationHandler.location.id)
    } else if (locationHandler.location === null) {
      setLocationId('')
    }
  }, [locationHandler.location])

  useEffect(() => {
    if (locationId !== '') {
      refetchLocationExpanded()
    }
  }, [locationId, refetchLocationExpanded])

  useEffect(() => {
    if (status === 'success' && locationExpandedData) {
      setSelectedLocations((prevArr) => {
        return prevArr.some(
          (location) =>
            location.locationIdentifier ===
            locationExpandedData.value[0].locationIdentifier
        )
          ? prevArr
          : [...prevArr, ...locationExpandedData.value]
      })
    }
  }, [locationExpandedData, status])

  const component: AggregateOptionsHandler = {
    ...expandedLocationsHandler,
    ...locationHandler,
    ...routeHandler,
    aggregatedData: aggregatedData as AggregateData[],
    startDateTime,
    endDateTime,
    startTime,
    endTime,
    selectedDays,
    selectedDirections,
    selectedMovements,
    metricType,
    binSize,
    averageOrSum,
    xAxisType,
    yAxisType,
    detectionType,
    visualChartType,
    handleRunAnalysis: () => {
      handleSubmit()
      console.log('Run Analysis!')
    },
    changeSelectedDays(days) {
      setSelectedDays(days)
    },
    changeSelectedDirections(directions) {
      setSelectedDirections(directions)
    },
    changeSelectedMovements(movements) {
      setSelectedMovements(movements)
    },
    changeStartDate: (date) => {
      setStartDateTime(date)
    },
    changeEndDate: (date) => {
      setEndDateTime(date)
    },
    changeStartTime: (date) => {
      setStartTime(date)
    },
    changeEndTime: (date) => {
      setEndTime(date)
    },
    changeMetricType(metricType) {
      setMetricType(metricType)
      console.log(metricType)
    },
    changeBinSize(binSize) {
      setBinSize(binSize)
    },
    changeXAxisType(value) {
      setXAxisType(value)
    },
    changeYAxisType(value) {
      setYAxisType(value)
    },
    changeDetectionType(value) {
      setDetectionType(value)
    },
    changeVisualChartType(value) {
      setVisualChartType(value)
    },
    changeAverageOrSum(value) {
      setAverageOrSum(value)
    },
  }

  return component
}
