import {
  type MeasureOptionPreset,
  useGetMeasureTypeMeasureOptionPresetsFromKey,
} from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import ChartMessages from '@/features/charts/components/chartMessages/ChartMessages'
import { useTimeOfDayReport } from '@/features/timeOfDay/api/getTimeOfDay'
import TimeOfDayOptions from '@/features/timeOfDay/components/TimeOfDayOptions'
import TimeOfDayResults from '@/features/timeOfDay/components/TimeOfDayResults'
import {
  getChangedTimeOfDayFormFields,
  mergeUntouchedTimeOfDayFormState,
  type TimeOfDayFormField,
} from '@/features/timeOfDay/formState'
import {
  buildTimeOfDaySchedulePresets,
  timeOfDayMeasureTypeId,
} from '@/features/timeOfDay/measureDefaults'
import {
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
  timeOfDayBinSizeMinutes,
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
import { useEffect, useMemo, useRef, useState } from 'react'

type PageError =
  | { type: 'NONE' }
  | { type: 'VALIDATION'; message: string }
  | { type: 'API'; message: string }

const getDefaultFormState = (): TimeOfDayFormState => ({
  selectedLocations: [],
  selectedDates: [subDays(startOfYesterday(), 1), startOfYesterday()],
  dataSource: 'IndianaEvents',
  allDayPrimaryDirections: defaultPrimaryDirections,
  amPrimaryDirections: defaultPrimaryDirections,
  pmPrimaryDirections: defaultPrimaryDirections,
  ...timeOfDayDefaultTuningOptions,
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
  const { data: schedulePresetData } =
    useGetMeasureTypeMeasureOptionPresetsFromKey(timeOfDayMeasureTypeId)
  const schedulePresets = useMemo(
    () =>
      buildTimeOfDaySchedulePresets(
        getSchedulePresetRecords(schedulePresetData)
      ),
    [schedulePresetData]
  )
  const defaultFormState = useMemo(() => getDefaultFormState(), [])
  const [qs, setQs] = useQueryStates(
    {
      locations: parseAsArrayOf(parseAsString, ',').withDefault([]),
      dates: parseAsArrayOf(ymdDateParser, ','),
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
      laneCapacity: parseAsInteger,
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
  const editedFormFieldsRef = useRef(new Set<TimeOfDayFormField>())
  // Only URL edits should reset locally edited fields.
  const queryStateKey = JSON.stringify(qs)
  const previousQueryStateKeyRef = useRef(queryStateKey)
  const [pageError, setPageError] = useState<PageError>({ type: 'NONE' })
  const {
    data: result,
    mutateAsync: generateTimeOfDay,
    isLoading,
  } = useTimeOfDayReport()

  const qsLocationsKey = qs.locations.join(',')

  useEffect(() => {
    const selectedDates = normalizeDates(
      qs.dates ?? defaultFormState.selectedDates,
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
      qs.laneCapacity !== null &&
      Number.isFinite(qs.laneCapacity) &&
      qs.laneCapacity > 0
        ? qs.laneCapacity
        : defaultFormState.laneCapacityVehiclesPerHour
    const directionLaneCounts = normalizeLaneCounts(qs.laneCounts)
    const tuningOptions = {
      amEntryPctOfPeak:
        qs.amEntryPctOfPeak ?? timeOfDayDefaultTuningOptions.amEntryPctOfPeak,
      amExitPctOfPeak:
        qs.amExitPctOfPeak ?? timeOfDayDefaultTuningOptions.amExitPctOfPeak,
      pmEntryPctOfPeak:
        qs.pmEntryPctOfPeak ?? timeOfDayDefaultTuningOptions.pmEntryPctOfPeak,
      pmExitPctOfPeak:
        qs.pmExitPctOfPeak ?? timeOfDayDefaultTuningOptions.pmExitPctOfPeak,
      freeEntryPctOfDailyPeak:
        qs.freeEntryPctOfDailyPeak ??
        timeOfDayDefaultTuningOptions.freeEntryPctOfDailyPeak,
      freeEntryPctOfDynamicRange:
        qs.freeEntryPctOfDynamicRange ??
        timeOfDayDefaultTuningOptions.freeEntryPctOfDynamicRange,
      entrySustainedBins:
        qs.entrySustainedBins ??
        timeOfDayDefaultTuningOptions.entrySustainedBins,
      freeSustainedBins:
        qs.freeSustainedBins ?? timeOfDayDefaultTuningOptions.freeSustainedBins,
      freeFallbackTime:
        qs.freeFallbackTime ?? timeOfDayDefaultTuningOptions.freeFallbackTime,
      maxAmEndTime:
        qs.maxAmEndTime ?? timeOfDayDefaultTuningOptions.maxAmEndTime,
      maxPmEndTime:
        qs.maxPmEndTime ?? timeOfDayDefaultTuningOptions.maxPmEndTime,
      laneCapacityVehiclesPerHour,
      approachVolumeAssumedLanes:
        qs.approachVolumeAssumedLanes ??
        timeOfDayDefaultTuningOptions.approachVolumeAssumedLanes,
      splitReviewThresholdPercent:
        qs.splitReviewThresholdPercent ??
        timeOfDayDefaultTuningOptions.splitReviewThresholdPercent,
      shoulderReviewThresholdPercent:
        qs.shoulderReviewThresholdPercent ??
        timeOfDayDefaultTuningOptions.shoulderReviewThresholdPercent,
    }

    const resolvedFormState: Partial<TimeOfDayFormState> = {
      selectedDates,
      dataSource: qs.dataSource,
      allDayPrimaryDirections,
      amPrimaryDirections,
      pmPrimaryDirections,
      ...tuningOptions,
      directionLaneCounts,
    }
    if (previousQueryStateKeyRef.current !== queryStateKey) {
      editedFormFieldsRef.current.clear()
      previousQueryStateKeyRef.current = queryStateKey
    }
    const editedFields = new Set(editedFormFieldsRef.current)

    setFormState((currentFormState) =>
      mergeUntouchedTimeOfDayFormState(
        currentFormState,
        resolvedFormState,
        editedFields
      )
    )
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
    queryStateKey,
    defaultFormState,
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

  const handleFormStateChange = (nextFormState: TimeOfDayFormState) => {
    setFormState((currentFormState) => {
      getChangedTimeOfDayFormFields(currentFormState, nextFormState).forEach(
        (field) => editedFormFieldsRef.current.add(field)
      )
      return nextFormState
    })
  }

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
      binSizeMinutes: timeOfDayBinSizeMinutes,
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
          onChange={handleFormStateChange}
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
