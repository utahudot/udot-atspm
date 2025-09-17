import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import WatchDogDatePopup from '@/features/locations/components/editLocation/WatchDogDatePopup'
import {
  useCreateWatchdogIgnoreEvents,
  useDeleteWatchdogIgnoreEvents,
  useEditWatchdogIgnoreEvents,
  useGetWatchdogIgnoreEvents,
} from '@/features/watchdog/api/watchdogIgnoreEvents'
import { useNotificationStore } from '@/stores/notifications'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box, Button, Divider, Paper, Typography } from '@mui/material'
import { addDays, format } from 'date-fns'
import { useEffect, useState } from 'react'

interface WatchdogOption {
  id: number
  locationId: number
  locationIdentifier: string
  start: string
  end: string
  issueType: WatchDogIssueTypes
}

enum WatchDogIssueTypes {
  RecordCount = 'RecordCount',
  ForceOffThreshold = 'ForceOffThreshold',
  MaxOutThreshold = 'MaxOutThreshold',
  LowDetectorHits = 'LowDetectorHits',
  StuckPed = 'StuckPed',
  UnconfiguredApproach = 'UnconfiguredApproach',
  UnconfiguredDetector = 'UnconfiguredDetector',
}

const watchdogOptions = [
  {
    label: 'Record Count',
    description:
      'Report phases with record counts below a configured threshold over the previous day.',
    issueType: WatchDogIssueTypes.RecordCount,
  },
  {
    label: 'Force Off Thresholds',
    description:
      'Report phases where the percentage of force offs exceeds a configured threshold within a specified number of activations during certain early morning hours.',
    issueType: WatchDogIssueTypes.ForceOffThreshold,
  },
  {
    label: 'Max Out Thresholds',
    description:
      'Report signals where the percentage of max outs exceeds a configured threshold within a specified number of activations during certain early morning hours.',
    issueType: WatchDogIssueTypes.MaxOutThreshold,
  },
  {
    label: 'Low Detector Counts',
    description:
      'Report phases with vehicle detector counts below a configured threshold during the previous day’s peak time period.',
    issueType: WatchDogIssueTypes.LowDetectorHits,
  },
  {
    label: 'Stuck Ped',
    description:
      'Report phases with pedestrian activations exceeding a configured threshold during certain early morning hours.',
    issueType: WatchDogIssueTypes.StuckPed,
  },
  {
    label: 'Unconfigured Approach',
    description:
      'Identifies and processes incoming event data from controllers that lack a corresponding configuration for the specified approach.',
    issueType: WatchDogIssueTypes.UnconfiguredApproach,
  },
  {
    label: 'Unconfigured Detector',
    description:
      'Identifies and processes incoming event data from controllers that lack a corresponding configuration for the specified detector.',
    issueType: WatchDogIssueTypes.UnconfiguredDetector,
  },
]

const WatchdogEditor = () => {
  const { addNotification } = useNotificationStore()
  const { location } = useLocationStore()
  const [ignoreEvents, setIgnoreEvents] = useState<WatchdogOption[]>([])
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [selectedOption, setSelectedOption] = useState<null | WatchdogOption>(
    null
  )
  const [startDate, setStartDate] = useState<Date | null>(new Date())
  const [endDate, setEndDate] = useState<Date | null>(addDays(new Date(), 1))
  const [selectedIgnoreEventId, setSelectedIgnoreEventId] = useState<
    number | null
  >(null)

  const {
    data: watchdogIgnoreEvents,
    isLoading,
    refetch,
  } = useGetWatchdogIgnoreEvents()
  const { mutateAsync: addWatchdogIgnoreEvents } =
    useCreateWatchdogIgnoreEvents()
  const { mutateAsync: editWatchdogIgnoreEvents } =
    useEditWatchdogIgnoreEvents()
  const { mutateAsync: deleteWatchdogIgnoreEvents } =
    useDeleteWatchdogIgnoreEvents()

  useEffect(() => {
    if (watchdogIgnoreEvents?.value) {
      const filteredEvents = watchdogIgnoreEvents.value.filter(
        (event) => event.locationIdentifier === location?.locationIdentifier
      )
      setIgnoreEvents(filteredEvents)
    }
  }, [watchdogIgnoreEvents, location?.locationIdentifier])

  const handlePillClick = (
    option: WatchdogOption,
    event: React.MouseEvent<HTMLElement>
  ) => {
    setAnchorEl(event.currentTarget)
    setSelectedOption(option)
    const ignoreEvent = ignoreEvents.find(
      (e) => e.issueType === option.issueType
    )
    if (ignoreEvent) {
      setStartDate(new Date(ignoreEvent.start))
      setEndDate(new Date(ignoreEvent.end))
      setSelectedIgnoreEventId(ignoreEvent.id)
    } else {
      setStartDate(new Date())
      setEndDate(addDays(new Date(), 1))
      setSelectedIgnoreEventId(null)
    }
  }

  const handleIgnoreEvent = async () => {
    if (startDate && endDate && selectedOption) {
      const existingEvent = ignoreEvents.find(
        (e) => e.issueType === selectedOption.issueType
      )

      if (existingEvent) {
        await editWatchdogIgnoreEvents({
          data: {
            locationId: location?.id,
            locationIdentifier: location?.locationIdentifier,
            issueType: selectedOption.issueType.toString(),
            start: toUTCDateStamp(startDate),
            end: toUTCDateStamp(endDate),
          },
          id: existingEvent.id,
        })
        addNotification({
          title: 'Watchdog Ignore Event Updated',
          type: 'success',
        })
      } else {
        await addWatchdogIgnoreEvents({
          locationId: location?.id,
          locationIdentifier: location?.locationIdentifier,
          issueType: selectedOption.issueType.toString(),
          start: toUTCDateStamp(startDate),
          end: toUTCDateStamp(endDate),
        })
        addNotification({
          title: 'Watchdog Ignore Event Added',
          type: 'success',
        })
      }

      await refetch()
      setAnchorEl(null)
    }
  }

  const handleRemoveIgnore = async () => {
    if (selectedIgnoreEventId) {
      await deleteWatchdogIgnoreEvents(selectedIgnoreEventId)
      addNotification({
        title: 'Watchdog Ignore Event Removed',
        type: 'success',
      })
      await refetch()
      setAnchorEl(null)
    }
  }

  const handlePopoverClose = () => {
    setAnchorEl(null)
  }

  if (isLoading) {
    return <Box height={'700px'}>Loading...</Box>
  }

  return (
    <Paper sx={{ padding: 2, mt: 2 }}>
      <Typography variant="h4" fontWeight="bold" component="p" sx={{ mb: 2 }}>
        Watchdog Options
      </Typography>
      {watchdogOptions.map((option, index) => {
        const isIgnored = ignoreEvents.some(
          (event) => event.issueType === option.issueType
        )
        const ignoreEvent = ignoreEvents.find(
          (e) => e.issueType === option.issueType
        )
        const pillLabel = isIgnored
          ? `Inactive from ${format(
              new Date(ignoreEvent.start),
              'MM/dd/yyyy'
            )} to ${format(new Date(ignoreEvent.end), 'MM/dd/yyyy')}`
          : 'active'

        return (
          <Box key={index}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                my: 1,
              }}
            >
              <Box sx={{ flex: 1 }}>
                <Typography variant="subtitle1" fontWeight="bold">
                  {option.label}
                </Typography>
                <Typography
                  variant="body2"
                  sx={{ color: 'text.secondary', maxWidth: '750px' }}
                >
                  {option.description}
                </Typography>
              </Box>
              <Button
                onClick={(e) => handlePillClick(option, e)}
                color={isIgnored ? 'warning' : 'success'}
                variant="contained"
                sx={{
                  px: 2,
                  textTransform: 'none',
                }}
              >
                {pillLabel}
              </Button>
            </Box>
            <Divider />
          </Box>
        )
      })}
      <WatchDogDatePopup
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        handlePopoverClose={handlePopoverClose}
        startDate={startDate}
        setStartDate={setStartDate}
        endDate={endDate}
        setEndDate={setEndDate}
        handleIgnoreEvent={handleIgnoreEvent}
        handleRemoveIgnore={handleRemoveIgnore}
      />
    </Paper>
  )
}

export default WatchdogEditor
