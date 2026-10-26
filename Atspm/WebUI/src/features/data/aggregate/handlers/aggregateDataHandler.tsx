import {
  LocationHandler,
  useLocationHandler,
} from '@/components/handlers/locationHandler'
import {
  RouteHandler,
  useRouteHandler,
} from '@/components/handlers/routeHandler'
import { getChartErrorMessage } from '@/features/charts/components/chartsContainer/getChartErrorMessage'
import { useGetLocation } from '@/features/locations/api'
import { LocationExpanded } from '@/features/locations/types'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import { DateTimeProps, TimeOnlyProps } from '@/types/TimeProps'
import { dateToTimestamp } from '@/utils/dateTime'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'
import { usePostAggregateData } from '../../api/getAggregateData'
import { AggregateApiData } from '../types/aggregateApiData'
import { AggregateData } from '../types/aggregateData'
import {
  AggregationType,
  MetricTypeOptionsList,
  YAxisOptions,
  binSizeMarks,
  chartTypeOptions,
  directionFilterOptions,
  getValidAxes,
  getXAxisOptions,
  getYAxisOptions,
  movementFilterOptions,
  xAxisOptions,
} from '../types/aggregateOptionsData'
import {
  createFilterDirections,
  createFilterMovements,
  createTimeOptions,
  getTimeValidationMessage,
  usesWholeDays,
} from './aggregateRequest'
import { toAggregateCharts } from './aggregateResponse'
import {
  ExpandLocationHandler,
  useExpandLocationHandler,
} from './expandLocationHandler'

// The options a result was requested with, so its chart still matches it when
// the options change while it loads.
export interface AggregateChartSettings {
  metricType: string
  averageOrSum: number
  xAxisType: number
  yAxisType: number
  visualChartType: string
  startDateTime: Date
  endDateTime: Date
}

export interface AggregateResult {
  charts: AggregateData[]
  settings: AggregateChartSettings
}

export interface AggregateOptionsHandler
  extends DateTimeProps,
    TimeOnlyProps,
    RouteHandler,
    LocationHandler,
    ExpandLocationHandler {
  result: AggregateResult | null
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
  availableXAxisOptions: typeof xAxisOptions
  availableYAxisOptions: typeof YAxisOptions
  validationMessage: string | null
  // Start Time, End Time and Days don't apply to this Bin Size.
  wholeDaysOnly: boolean
  isLoading: boolean
  errorMessage: string | null
  noDataFound: boolean
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
  // Checkbox positions, all checked to start.
  const [selectedDirections, setSelectedDirections] = useState<number[]>(
    directionFilterOptions.map((_, index) => index)
  )
  const [selectedMovements, setSelectedMovements] = useState<number[]>(
    movementFilterOptions.map((_, index) => index)
  )
  const [metricType, setMetricType] = useState<string>('')
  const [xAxisType, setXAxisType] = useState<number>(xAxisOptions[0].id)
  const [yAxisType, setYAxisType] = useState<number>(YAxisOptions[0].id)
  const [detectionType, setDetectionType] = useState<string>('')
  const [visualChartType, setVisualChartType] = useState<
    'line' | 'bar' | 'pie'
  >(chartTypeOptions[0].id)
  const [binSize, setBinSize] = useState<number>(binSizeMarks[0].value)
  const [locationId, setLocationId] = useState<string>('')
  const [averageOrSum, setAverageOrSum] = useState<number>(0)
  const [selectedLocations, setSelectedLocations] = useState<
    LocationExpanded[]
  >([])
  const [result, setResult] = useState<AggregateResult | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [noDataFound, setNoDataFound] = useState(false)
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

  const locationIds = selectedLocations.map((location) => location.id)

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

  const timeInputs = {
    startDateTime,
    endDateTime,
    startTime,
    endTime,
    selectedDays,
    binSize,
  }

  const getValidationMessage = (): string | null => {
    if (!expandedLocationsHandler.updatedLocations.length)
      return 'Select at least one location.'
    if (!metricType) return 'Select a Metric Type.'
    return getTimeValidationMessage(timeInputs)
  }

  const createAggregateObject = (): AggregateApiData => {
    const locationIdentifiers = selectedLocations.map(
      (location) => location.locationIdentifier
    )
    const metric = metricType.split('-')
    const aggregationType = getAggregateTypeEnumValue(metric[0])
    const dataType = getDataTypeValue(metric[0], metric[1])

    const timeOptions = createTimeOptions(timeInputs)
    const filterDirections = createFilterDirections(selectedDirections)
    const filterMovements = createFilterMovements(selectedMovements)

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
    setErrorMessage(null)
    setNoDataFound(false)
    setResult(null)
    try {
      const aggregateObject: AggregateApiData = createAggregateObject()
      const settings: AggregateChartSettings = {
        metricType,
        averageOrSum,
        xAxisType,
        yAxisType,
        visualChartType,
        startDateTime,
        endDateTime,
      }
      const charts = toAggregateCharts(
        await postMutation.mutateAsync(aggregateObject)
      ).filter((chart) =>
        chart.series.some((series) => series.dataPoints.length > 0)
      )
      setNoDataFound(charts.length === 0)
      setResult(charts.length ? { charts, settings } : null)
    } catch (error) {
      setErrorMessage(getChartErrorMessage(error))
    }
  }

  useEffect(() => {
    if (routeHandler.routeId) {
      setSelectedLocations([])
      setRouteExpandedLocations([])
      refectRouteExpanded()
    }
  }, [refectRouteExpanded, routeHandler.routeId])

  useEffect(() => {
    if (routeExpandedLocations && routeExpandedLocations.length > 0) {
      const filteredExpandedLocations = routeExpandedLocations.filter(
        (location) => !locationIds.includes(location.id)
      )
      if (filteredExpandedLocations.length > 0) {
        setSelectedLocations((prev) => [...prev, ...filteredExpandedLocations])
      }
      setRouteExpandedLocations([])
    }
  }, [locationIds, routeExpandedLocations])

  useEffect(() => {
    if (routeStatus === 'success' && routeWithExpandedLocations) {
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

  const changeAxes = (
    nextMetricType: string,
    nextXAxisType: number,
    nextBinSize: number
  ) => {
    const axes = getValidAxes(
      nextMetricType,
      nextXAxisType,
      yAxisType,
      nextBinSize
    )
    setXAxisType(axes.xAxisType)
    setYAxisType(axes.yAxisType)
  }

  const component: AggregateOptionsHandler = {
    ...expandedLocationsHandler,
    ...locationHandler,
    ...routeHandler,
    result,
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
    availableXAxisOptions: getXAxisOptions(metricType, binSize),
    availableYAxisOptions: getYAxisOptions(metricType, xAxisType),
    validationMessage: getValidationMessage(),
    wholeDaysOnly: usesWholeDays(binSize),
    isLoading: postMutation.isLoading,
    errorMessage,
    noDataFound,
    handleRunAnalysis: () => {
      handleSubmit()
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
      changeAxes(metricType, xAxisType, binSize)
    },
    changeBinSize(binSize) {
      setBinSize(binSize)
      changeAxes(metricType, xAxisType, binSize)
    },
    changeXAxisType(value) {
      changeAxes(metricType, value, binSize)
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
