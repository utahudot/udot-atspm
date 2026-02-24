import { Location } from '@/api/config'
import {
  PedatLocationData,
  useGetPedestrianAggregationLocationData,
} from '@/api/reports'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import RightSidebar from '@/components/RightSidebar'
import { ActiveTransportationOptions } from '@/features/activeTransportation/components/ActiveTransportationOptions'
import PedatChartsContainer from '@/features/activeTransportation/components/PedatChartsContainer'
import PedestrianActivityInfoPanel from '@/features/activeTransportation/components/PedestrianActivityInfoPanel'
import { dateToTimestamp } from '@/utils/dateTime'
import { roundTo } from '@/utils/numberFormat'
import { DropResult } from '@hello-pangea/dnd'
import { zodResolver } from '@hookform/resolvers/zod'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Alert, Box } from '@mui/material'
import { startOfDay, subYears } from 'date-fns'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

const activeTransportationSchema = z.object({
  locations: z.array(
    z.object({
      id: z.string(),
      name: z.string(),
      approaches: z.array(z.any()),
      locationIdentifier: z.string(),
    })
  ),
  timeUnit: z.number(),
  startDate: z.date(),
  endDate: z.date(),
  phase: z
    .union([z.number(), z.literal('All')])
    .nullable()
    .optional(),
})

type ActiveTransportationForm = z.infer<typeof activeTransportationSchema>

export type ATErrorState =
  | { type: 'NONE' }
  | { type: 'NO_LOCATIONS' }
  | { type: 'MISSING_PHASES'; locationIDs: Set<string> }
  | { type: '400' }
  | { type: 'UNKNOWN'; message: string }

const ActiveTransportation = () => {
  const { mutateAsync: fetchPedestrianData, isLoading } =
    useGetPedestrianAggregationLocationData()

  const form = useForm<ActiveTransportationForm>({
    resolver: zodResolver(activeTransportationSchema),
    defaultValues: {
      locations: [],
      timeUnit: 0,
      startDate: startOfDay(subYears(new Date(), 1)),
      endDate: startOfDay(new Date()),
      phase: null,
    },
  })

  const { watch, setValue } = form
  const [errorState, setErrorState] = useState<ATErrorState>({ type: 'NONE' })
  const [data, setData] = useState<PedatLocationData[] | null>(null)

  const locations = watch('locations')
  const timeUnit = watch('timeUnit')
  const startDate = watch('startDate')
  const endDate = watch('endDate')
  const phase = watch('phase')

  useEffect(() => {
    setData(null)
  }, [locations, timeUnit, startDate, endDate, phase])

  function renderErrorAlert() {
    if (errorState.type === 'NO_LOCATIONS') {
      return (
        <Alert severity="error">Please select one or more locations.</Alert>
      )
    }
    if (errorState.type === 'MISSING_PHASES') {
      return (
        <Alert severity="error">
          Please select phases for the highlighted locations.
        </Alert>
      )
    }
    if (errorState.type === '400') {
      return (
        <Alert severity="error">
          400: The requested resource was not found.
        </Alert>
      )
    }
    if (errorState.type === 'UNKNOWN') {
      return (
        <Alert severity="error">
          Something went wrong: {errorState.message}
        </Alert>
      )
    }
    return null
  }

  const setLocations = (newLocations: Location[]) => {
    const hadNoLocations = errorState.type === 'NO_LOCATIONS'

    setValue('locations', newLocations)

    if (hadNoLocations && newLocations.length > 0) {
      setErrorState({ type: 'NONE' })
    }
  }

  const handleLocationDelete = (location: Location) => {
    const newLocations = locations.filter((loc) => loc.id !== location.id)
    setValue('locations', newLocations)
  }

  const handleReorderLocations = (dropResult: DropResult) => {
    if (!dropResult.destination) return
    const newLocations = Array.from(locations)
    const [reorderedItem] = newLocations.splice(dropResult.source.index, 1)
    newLocations.splice(dropResult.destination.index, 0, reorderedItem)
    setValue('locations', newLocations)
  }

  const handleUpdateLocation = (updatedLocation: Location) => {
    const updatedLocations = locations.map((loc) =>
      loc.id === updatedLocation.id ? updatedLocation : loc
    )
    setValue('locations', updatedLocations)

    if (errorState.type === 'MISSING_PHASES') {
      const newIDs = new Set(errorState.locationIDs)
      if (newIDs.size === 0) {
        setErrorState({ type: 'NONE' })
      } else {
        setErrorState({ type: 'MISSING_PHASES', locationIDs: newIDs })
      }
    }
  }

  const handleGenerateReport = async () => {
    const formData = form.getValues()
    if (formData.locations.length === 0) {
      setErrorState({ type: 'NO_LOCATIONS' })
      return
    }
    setErrorState({ type: 'NONE' })

    const charts = await fetchPedestrianData({
      data: {
        locationIdentifiers: formData.locations.map(
          (l) => l.locationIdentifier
        ),
        startDate: dateToTimestamp(formData.startDate),
        endDate: dateToTimestamp(formData.endDate),
        timeUnit: formData.timeUnit,
        phase: formData.phase === 'All' ? null : formData.phase,
      },
    })

    charts.forEach((location) => {
      location.averageDailyVolume = roundTo(location.averageDailyVolume, 0)

      location.averageVolumeByHourOfDay.forEach((item) => {
        item.volume = roundTo(item.volume, 0)
      })

      location.averageVolumeByDayOfWeek.forEach((item) => {
        item.volume = roundTo(item.volume, 0)
      })

      location.averageVolumeByMonthOfYear.forEach((item) => {
        item.volume = roundTo(item.volume, 0)
      })

      location.rawData?.forEach((item) => {
        item.pedestrianCount = roundTo(item.pedestrianCount, 0)
      })

      location.statisticData.count = roundTo(location.statisticData?.count, 0)
      location.statisticData.max = roundTo(location.statisticData?.max, 0)
      location.statisticData.min = roundTo(location.statisticData?.min, 0)
      location.statisticData.totalVolume = roundTo(
        location.statisticData?.totalVolume,
        0
      )
      location.statisticData.mean = roundTo(location.statisticData?.mean, 0)
      location.statisticData.std = roundTo(location.statisticData?.std, 0)
      location.statisticData.fiftiethPercentile = roundTo(
        location.statisticData?.fiftiethPercentile,
        0
      )
      location.statisticData.seventyFifthPercentile = roundTo(
        location.statisticData?.seventyFifthPercentile,
        0
      )
      location.statisticData.twentyFifthPercentile = roundTo(
        location.statisticData?.twentyFifthPercentile,
        0
      )
    })
    setData(charts)
  }

  return (
    <ResponsivePageLayout
      title="Pedestrian Activity"
      subtitle="Utah State University"
      hasRightSidebar
    >
      <RightSidebar title="Pedestrian Activity">
        <PedestrianActivityInfoPanel />
      </RightSidebar>

      <ActiveTransportationOptions
        errorState={errorState}
        locations={locations}
        timeUnit={timeUnit}
        startDate={startDate}
        endDate={endDate}
        phase={phase}
        setLocations={setLocations}
        setTimeUnit={(unit) => setValue('timeUnit', unit)}
        setStartDate={(date) => setValue('startDate', date)}
        setEndDate={(date) => setValue('endDate', date)}
        setPhase={(phase) => setValue('phase', phase)}
        onLocationDelete={handleLocationDelete}
        onReorderLocations={handleReorderLocations}
        onUpdateLocation={handleUpdateLocation}
      />

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 2 }}>
        <LoadingButton
          loading={isLoading}
          loadingPosition="start"
          startIcon={<PlayArrowIcon />}
          variant="contained"
          sx={{ padding: '10px', mb: 2 }}
          onClick={handleGenerateReport}
        >
          Generate Report
        </LoadingButton>
        {renderErrorAlert()}
      </Box>

      {data && (
        <PedatChartsContainer data={data} phase={phase} timeUnit={timeUnit} />
      )}
    </ResponsivePageLayout>
  )
}

export default ActiveTransportation
