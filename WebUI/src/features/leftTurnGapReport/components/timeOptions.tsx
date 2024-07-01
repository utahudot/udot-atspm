import {
  StyledComponentHeader,
  commonPaperStyle,
} from '@/components/HeaderStyling/StyledComponentHeader'
import {
  Box,
  FormControlLabel,
  Paper,
  Radio,
  RadioGroup,
  TextField,
} from '@mui/material'
import { ChangeEvent, useState } from 'react'

const timeOptionRaidoButtons = {
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'space-around',
  height: '250px',
  paddingLeft: '15px',
}

interface TimeOptionsProps {
  timeOptions: string
  setTimeOptions: React.Dispatch<React.SetStateAction<string>>
  startHour: number
  setStartHour: React.Dispatch<React.SetStateAction<number>>
  endHour: number
  setEndHour: React.Dispatch<React.SetStateAction<number>>
  startMinute: number
  setStartMinute: React.Dispatch<React.SetStateAction<number>>
  endMinute: number
  setEndMinute: React.Dispatch<React.SetStateAction<number>>
  setGetAMPMPeakHour: React.Dispatch<React.SetStateAction<boolean>>
  setGet24HourPeriod: React.Dispatch<React.SetStateAction<boolean>>
  setGetAMPMPeakPeriod: React.Dispatch<React.SetStateAction<boolean>>
}

// ... (previous imports and code)

export const TimeOptions = ({
  timeOptions,
  setTimeOptions,
  startHour,
  setStartHour,
  endHour,
  setEndHour,
  startMinute,
  setStartMinute,
  endMinute,
  setEndMinute,
  setGetAMPMPeakHour,
  setGet24HourPeriod,
  setGetAMPMPeakPeriod,
}: TimeOptionsProps) => {
  const [startTimeString, setStartTimeString] = useState('00:00')
  const [endTimeString, setEndTimeString] = useState('23:59')

  const handleTimeOptionChange = (event: ChangeEvent<HTMLInputElement>) => {
    setTimeOptions(event.target.value)
    if (event.target.value === 'customTimeRadiobutton') {
      setGetAMPMPeakHour(false)
      setGet24HourPeriod(false)
      setGetAMPMPeakPeriod(false)
    } else if (event.target.value === 'PeakHourRadiobutton') {
      setGetAMPMPeakHour(true)
      setGet24HourPeriod(false)
      setGetAMPMPeakPeriod(false)
    } else if (event.target.value === 'PeakPeriodRadiobutton') {
      setGetAMPMPeakHour(false)
      setGet24HourPeriod(false)
      setGetAMPMPeakPeriod(true)
    } else if (event.target.value === 'FullDayRadiobutton') {
      setGetAMPMPeakHour(false)
      setGet24HourPeriod(true)
      setGetAMPMPeakPeriod(false)
      setStartHour(0)
      setEndHour(23)
      setStartMinute(0)
      setEndMinute(59)
    }
  }

  const handleCustomTimeChange = (event: ChangeEvent<HTMLInputElement>) => {
    const timeString = event.target.value
    const [hours, minutes] = timeString.split(':')

    if (event.target.name === 'startTime') {
      setStartTimeString(timeString)
      setStartHour(parseInt(hours, 10))
      setStartMinute(parseInt(minutes, 10))
    } else {
      setEndTimeString(timeString)
      setEndHour(parseInt(hours, 10))
      setEndMinute(parseInt(minutes, 10))
    }
  }

  return (
    <Paper
      sx={{
        ...commonPaperStyle,
        minWidth: '154px',

        '@media (max-width: 1200px)': {
          marginTop: 1,
        },
        '@media (max-width: 800px)': {
          marginBottom: 1,
          marginTop: 1,
        },
      }}
    >
      <Box>
        <StyledComponentHeader header={'Time Options'} />
      </Box>
      <RadioGroup
        sx={{ ...timeOptionRaidoButtons }}
        value={timeOptions}
        onChange={handleTimeOptionChange}
      >
        <FormControlLabel
          value="PeakHourRadiobutton"
          control={<Radio />}
          label="Peak Hour (AM & PM)"
        />
        <FormControlLabel
          value="PeakPeriodRadiobutton"
          control={<Radio />}
          label="Peak Periods (6AM-9AM & 3PM-6PM)"
        />
        <FormControlLabel
          value="FullDayRadiobutton"
          control={<Radio />}
          label="24-Hour Period"
        />
        <FormControlLabel
          value="customTimeRadiobutton"
          control={<Radio />}
          label="Custom"
        />
      </RadioGroup>
      {timeOptions === 'customTimeRadiobutton' && (
        <Box>
          <TextField
            type="time"
            label="Start Time"
            name="startTime"
            value={startTimeString}
            onChange={handleCustomTimeChange}
            InputLabelProps={{ shrink: true }}
          />
          <TextField
            type="time"
            label="End Time"
            name="endTime"
            value={endTimeString}
            onChange={handleCustomTimeChange}
            InputLabelProps={{ shrink: true }}
          />
        </Box>
      )}
    </Paper>
  )
}