import {
  type MeasureOptionPreset,
  useGetMeasureTypeMeasureOptionPresetsFromKey,
} from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useChartDefaults } from '@/features/charts/api'
import ChartMessages from '@/features/charts/components/chartMessages/ChartMessages'
import { useTimeOfDayReport } from '@/features/timeOfDay/api/getTimeOfDay'
import TimeOfDayOptions from '@/features/timeOfDay/components/TimeOfDayOptions'
import TimeOfDayResults from '@/features/timeOfDay/components/TimeOfDayResults'
import type { TimeOfDayMeasureDefaults } from '@/features/timeOfDay/measureDefaults'
import {
  buildTimeOfDaySchedulePresets,
  buildTimeOfDayTuningOptionsFromDefaults,
  timeOfDayMeasureTypeId,
} from '@/features/timeOfDay/measureDefaults'
import {
  areDateArraysEqual,
  areLaneCountsEqual,
  areStringArraysEqual,
  createSearchLocationsFromIdentifiers,
  dataSourceParser,
  laneCountsParser,
  normalizeDates,
  normalizeDirections,
  normalizeLaneCounts,
  normalizeLocationIdentifiers,
  resolveSearchLocationsByIdentifier,
  ymdDateParser,
} from '@/features/timeOfDay/queryParams'
import type {
  TimeOfDayFormState,
  TimeOfDayOptions as TimeOfDayRequestOptions,
  TimeOfDayTuningOptions,
} from '@/features/timeOfDay/types'
import {
  defaultPrimaryDirections,
  timeOfDayDefaultTuningOptions,
  timeOfDayTuningOptionKeys,
} from '@/features/timeOfDay/types'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Box, Stack } from '@mui/material'
import { AxiosError } from 'axios'
import { format, parseISO, startOfYesterday, subDays } from 'date-fns'
import {
  parseAsArrayOf,
  parseAsFloat,
  parseAsInteger,
  parseAsString,
  useQueryStates,
} from 'nuqs'
import { useEffect, useMemo, useState } from 'react'

type PageError =
  | { type: 'NONE' }
  | { type: 'VALIDATION'; message: string }
  | { type: 'API'; message: string }

const getDefaultFormState = (
  tuningOptions: TimeOfDayTuningOptions = timeOfDayDefaultTuningOptions
): TimeOfDayFormState => ({
  selectedLocations: [],
  selectedDates: [subDays(startOfYesterday(), 1), startOfYesterday()],
  dataSource: 'IndianaEvents',
  allDayPrimaryDirections: defaultPrimaryDirections,
  amPrimaryDirections: defaultPrimaryDirections,
  pmPrimaryDirections: defaultPrimaryDirections,
  ...tuningOptions,
  directionLaneCounts: {},
})

const getTuningOptions = (
  options: TimeOfDayFormState
): TimeOfDayTuningOptions =>
  timeOfDayTuningOptionKeys.reduce(
    (tuningOptions, key) => ({
      ...tuningOptions,
      [key]: options[key],
    }),
    {} as TimeOfDayTuningOptions
  )

const areTuningOptionsEqual = (
  current: TimeOfDayFormState,
  next: TimeOfDayTuningOptions
) => timeOfDayTuningOptionKeys.every((key) => current[key] === next[key])

const getErrorMessage = (error: unknown) => {
  if (error instanceof AxiosError) {
    const responseData = error.response?.data

    if (typeof responseData === 'string') return responseData
    if (responseData?.detail && typeof responseData.detail === 'string') {
      return responseData.detail
    }
    if (responseData?.title && typeof responseData.title === 'string') {
      return responseData.title
    }
  }

  if (error instanceof Error) return error.message

  return 'Unable to generate Time Of Day analysis.'
}

const getSchedulePresetRecords = (data: unknown): MeasureOptionPreset[] => {
  if (Array.isArray(data)) return data as MeasureOptionPreset[]

  if (data && typeof data === 'object' && 'value' in data) {
    const value = (data as { value?: unknown }).value
    if (Array.isArray(value)) return value as MeasureOptionPreset[]
  }

  return []
}

export default function TimeOfDayPage() {
  const { data: chartDefaultsData } = useChartDefaults()
  const { data: schedulePresetData } =
    useGetMeasureTypeMeasureOptionPresetsFromKey(timeOfDayMeasureTypeId)
  const schedulePresets = useMemo(
    () =>
      buildTimeOfDaySchedulePresets(
        getSchedulePresetRecords(schedulePresetData)
      ),
    [schedulePresetData]
  )
  const measureDefaultOptions = useMemo(() => {
    const timeOfDayMeasureType = chartDefaultsData?.value.find(
      (measureType) => measureType.id === timeOfDayMeasureTypeId
    )

    return buildTimeOfDayTuningOptionsFromDefaults(
      timeOfDayMeasureType?.measureOptions as unknown as
        | TimeOfDayMeasureDefaults
        | undefined
    )
  }, [chartDefaultsData])
  const defaultFormState = useMemo(
    () => getDefaultFormState(measureDefaultOptions),
    [measureDefaultOptions]
  )
  const [qs, setQs] = useQueryStates(
    {
      locations: parseAsArrayOf(parseAsString, ',').withDefault([]),
      dates: parseAsArrayOf(ymdDateParser, ',').withDefault(
        defaultFormState.selectedDates
      ),
      dataSource: dataSourceParser.withDefault(defaultFormState.dataSource),
      primaryDirections: parseAsArrayOf(parseAsString, ',').withDefault(
        defaultFormState.allDayPrimaryDirections
      ),
      amPrimaryDirections: parseAsArrayOf(parseAsString, ',').withDefault(
        defaultFormState.amPrimaryDirections
      ),
      pmPrimaryDirections: parseAsArrayOf(parseAsString, ',').withDefault(
        defaultFormState.pmPrimaryDirections
      ),
      laneCapacity: parseAsInteger.withDefault(
        defaultFormState.laneCapacityVehiclesPerHour
      ),
      amEntryPctOfPeak: parseAsFloat,
      amExitPctOfPeak: parseAsFloat,
      pmEntryPctOfPeak: parseAsFloat,
      pmExitPctOfPeak: parseAsFloat,
      freeEntryPctOfDailyPeak: parseAsFloat,
      freeEntryPctOfDynamicRange: parseAsFloat,
      entrySustainedBins: parseAsInteger,
      freeSustainedBins: parseAsInteger,
      freeFallbackTime: parseAsString,
      maxAmEndTime: parseAsString,
      maxPmEndTime: parseAsString,
      approachVolumeAssumedLanes: parseAsFloat,
      splitReviewThresholdPercent: parseAsFloat,
      shoulderReviewThresholdPercent: parseAsFloat,
      laneCounts: laneCountsParser.withDefault(
        defaultFormState.directionLaneCounts
      ),
    },
    { history: 'replace' }
  )
  const [formState, setFormState] =
    useState<TimeOfDayFormState>(defaultFormState)
  const [pageError, setPageError] = useState<PageError>({ type: 'NONE' })
  const {
    data: result,
    mutateAsync: generateTimeOfDay,
    isLoading,
  } = useTimeOfDayReport()

  const qsLocationsKey = qs.locations.join(',')

  useEffect(() => {
    const selectedDates = normalizeDates(
      qs.dates,
      defaultFormState.selectedDates
    )
    const allDayPrimaryDirections = normalizeDirections(
      qs.primaryDirections,
      defaultFormState.allDayPrimaryDirections
    )
    const amPrimaryDirections = normalizeDirections(
      qs.amPrimaryDirections,
      allDayPrimaryDirections
    )
    const pmPrimaryDirections = normalizeDirections(
      qs.pmPrimaryDirections,
      allDayPrimaryDirections
    )
    const laneCapacityVehiclesPerHour =
      Number.isFinite(qs.laneCapacity) && qs.laneCapacity > 0
        ? qs.laneCapacity
        : defaultFormState.laneCapacityVehiclesPerHour
    const directionLaneCounts = normalizeLaneCounts(qs.laneCounts)
    const tuningOptions = {
      amEntryPctOfPeak:
        qs.amEntryPctOfPeak ?? measureDefaultOptions.amEntryPctOfPeak,
      amExitPctOfPeak:
        qs.amExitPctOfPeak ?? measureDefaultOptions.amExitPctOfPeak,
      pmEntryPctOfPeak:
        qs.pmEntryPctOfPeak ?? measureDefaultOptions.pmEntryPctOfPeak,
      pmExitPctOfPeak:
        qs.pmExitPctOfPeak ?? measureDefaultOptions.pmExitPctOfPeak,
      freeEntryPctOfDailyPeak:
        qs.freeEntryPctOfDailyPeak ??
        measureDefaultOptions.freeEntryPctOfDailyPeak,
      freeEntryPctOfDynamicRange:
        qs.freeEntryPctOfDynamicRange ??
        measureDefaultOptions.freeEntryPctOfDynamicRange,
      entrySustainedBins:
        qs.entrySustainedBins ?? measureDefaultOptions.entrySustainedBins,
      freeSustainedBins:
        qs.freeSustainedBins ?? measureDefaultOptions.freeSustainedBins,
      freeFallbackTime:
        qs.freeFallbackTime ?? measureDefaultOptions.freeFallbackTime,
      maxAmEndTime: qs.maxAmEndTime ?? measureDefaultOptions.maxAmEndTime,
      maxPmEndTime: qs.maxPmEndTime ?? measureDefaultOptions.maxPmEndTime,
      laneCapacityVehiclesPerHour,
      approachVolumeAssumedLanes:
        qs.approachVolumeAssumedLanes ??
        measureDefaultOptions.approachVolumeAssumedLanes,
      splitReviewThresholdPercent:
        qs.splitReviewThresholdPercent ??
        measureDefaultOptions.splitReviewThresholdPercent,
      shoulderReviewThresholdPercent:
        qs.shoulderReviewThresholdPercent ??
        measureDefaultOptions.shoulderReviewThresholdPercent,
    }

    setFormState((currentFormState) => {
      if (
        areDateArraysEqual(currentFormState.selectedDates, selectedDates) &&
        currentFormState.dataSource === qs.dataSource &&
        areStringArraysEqual(
          currentFormState.allDayPrimaryDirections,
          allDayPrimaryDirections
        ) &&
        areStringArraysEqual(
          currentFormState.amPrimaryDirections,
          amPrimaryDirections
        ) &&
        areStringArraysEqual(
          currentFormState.pmPrimaryDirections,
          pmPrimaryDirections
        ) &&
        currentFormState.laneCapacityVehiclesPerHour ===
          laneCapacityVehiclesPerHour &&
        areTuningOptionsEqual(currentFormState, tuningOptions) &&
        areLaneCountsEqual(
          currentFormState.directionLaneCounts,
          directionLaneCounts
        )
      ) {
        return currentFormState
      }

      return {
        ...currentFormState,
        selectedDates,
        dataSource: qs.dataSource,
        allDayPrimaryDirections,
        amPrimaryDirections,
        pmPrimaryDirections,
        ...tuningOptions,
        directionLaneCounts,
      }
    })
  }, [
    qs.dates,
    qs.dataSource,
    qs.primaryDirections,
    qs.amPrimaryDirections,
    qs.pmPrimaryDirections,
    qs.laneCapacity,
    qs.amEntryPctOfPeak,
    qs.amExitPctOfPeak,
    qs.pmEntryPctOfPeak,
    qs.pmExitPctOfPeak,
    qs.freeEntryPctOfDailyPeak,
    qs.freeEntryPctOfDynamicRange,
    qs.entrySustainedBins,
    qs.freeSustainedBins,
    qs.freeFallbackTime,
    qs.maxAmEndTime,
    qs.maxPmEndTime,
    qs.approachVolumeAssumedLanes,
    qs.splitReviewThresholdPercent,
    qs.shoulderReviewThresholdPercent,
    qs.laneCounts,
    defaultFormState,
    measureDefaultOptions,
  ])

  useEffect(() => {
    const identifiers = normalizeLocationIdentifiers(qs.locations)
    let cancelled = false

    if (identifiers.length === 0) {
      setFormState((currentFormState) =>
        currentFormState.selectedLocations.length
          ? { ...currentFormState, selectedLocations: [] }
          : currentFormState
      )
      return
    }

    const fallbackLocations = createSearchLocationsFromIdentifiers(identifiers)

    setFormState((currentFormState) => {
      const currentLocationIdentifiers = normalizeLocationIdentifiers(
        currentFormState.selectedLocations
          .map((location) => location.locationIdentifier)
          .filter((identifier): identifier is string => Boolean(identifier))
      )

      if (areStringArraysEqual(currentLocationIdentifiers, identifiers)) {
        return currentFormState
      }

      return {
        ...currentFormState,
        selectedLocations: fallbackLocations,
      }
    })

    void (async () => {
      try {
        const locations = await resolveSearchLocationsByIdentifier(identifiers)

        if (cancelled) return

        setFormState((currentFormState) => {
          const currentLocationIdentifiers = normalizeLocationIdentifiers(
            currentFormState.selectedLocations
              .map((location) => location.locationIdentifier)
              .filter((identifier): identifier is string => Boolean(identifier))
          )

          if (!areStringArraysEqual(currentLocationIdentifiers, identifiers)) {
            return currentFormState
          }

          return {
            ...currentFormState,
            selectedLocations: locations,
          }
        })
      } catch {
        return
      }
    })()

    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [qsLocationsKey])

  const buildRequestOptions = (): TimeOfDayRequestOptions | null => {
    const locationIdentifiers = formState.selectedLocations
      .map((location) => location.locationIdentifier)
      .filter((identifier): identifier is string => Boolean(identifier))

    if (locationIdentifiers.length === 0) {
      setPageError({
        type: 'VALIDATION',
        message: 'Please select one or more locations.',
      })
      return null
    }

    if (formState.selectedDates.length === 0) {
      setPageError({
        type: 'VALIDATION',
        message: 'Please select one or more dates.',
      })
      return null
    }

    const selectedDates = Array.from(
      new Set(formState.selectedDates.map((date) => format(date, 'yyyy-MM-dd')))
    ).sort()

    return {
      locationIdentifiers,
      selectedDates,
      dataSource: formState.dataSource,
      allDayPrimaryDirections: formState.allDayPrimaryDirections,
      amPrimaryDirections: formState.amPrimaryDirections,
      pmPrimaryDirections: formState.pmPrimaryDirections,
      binSizeMinutes: 15,
      ...getTuningOptions(formState),
      directionLaneCounts: formState.directionLaneCounts,
    }
  }

  const handleGenerateAnalysis = async () => {
    const requestOptions = buildRequestOptions()
    if (!requestOptions) return

    setPageError({ type: 'NONE' })

    try {
      await setQs({
        locations: requestOptions.locationIdentifiers,
        dates: requestOptions.selectedDates.map((date) => parseISO(date)),
        dataSource: requestOptions.dataSource,
        primaryDirections: requestOptions.allDayPrimaryDirections,
        amPrimaryDirections: requestOptions.amPrimaryDirections,
        pmPrimaryDirections: requestOptions.pmPrimaryDirections,
        laneCapacity:
          requestOptions.laneCapacityVehiclesPerHour ??
          defaultFormState.laneCapacityVehiclesPerHour,
        amEntryPctOfPeak: requestOptions.amEntryPctOfPeak,
        amExitPctOfPeak: requestOptions.amExitPctOfPeak,
        pmEntryPctOfPeak: requestOptions.pmEntryPctOfPeak,
        pmExitPctOfPeak: requestOptions.pmExitPctOfPeak,
        freeEntryPctOfDailyPeak: requestOptions.freeEntryPctOfDailyPeak,
        freeEntryPctOfDynamicRange: requestOptions.freeEntryPctOfDynamicRange,
        entrySustainedBins: requestOptions.entrySustainedBins,
        freeSustainedBins: requestOptions.freeSustainedBins,
        freeFallbackTime: requestOptions.freeFallbackTime,
        maxAmEndTime: requestOptions.maxAmEndTime,
        maxPmEndTime: requestOptions.maxPmEndTime,
        approachVolumeAssumedLanes: requestOptions.approachVolumeAssumedLanes,
        splitReviewThresholdPercent: requestOptions.splitReviewThresholdPercent,
        shoulderReviewThresholdPercent:
          requestOptions.shoulderReviewThresholdPercent,
        laneCounts: requestOptions.directionLaneCounts ?? {},
      })
      await generateTimeOfDay(requestOptions)
    } catch (error) {
      setPageError({ type: 'API', message: getErrorMessage(error) })
    }
  }

  return (
    <ResponsivePageLayout title="Time-of-Day Plan Analysis" useFullWidth>
      <Stack spacing={2}>
        <TimeOfDayOptions
          options={formState}
          onChange={setFormState}
          schedulePresets={schedulePresets}
        />
        <Box
          sx={{
            display: 'flex',
            alignItems: 'flex-start',
            flexWrap: 'wrap',
            gap: 1.5,
          }}
        >
          <LoadingButton
            loading={isLoading}
            loadingPosition="start"
            startIcon={<PlayArrowIcon />}
            variant="contained"
            sx={{ padding: '10px' }}
            onClick={handleGenerateAnalysis}
          >
            Generate Analysis
          </LoadingButton>
          {pageError.type !== 'NONE' && (
            <ChartMessages
              messages={[pageError.message]}
              ariaLabel="Analysis errors"
            />
          )}
          {result?.warnings && result.warnings.length > 0 && (
            <ChartMessages
              messages={result.warnings}
              severity="warning"
              ariaLabel="Analysis warnings"
            />
          )}
        </Box>
      </Stack>
      {result && <TimeOfDayResults result={result} />}
    </ResponsivePageLayout>
  )
}
