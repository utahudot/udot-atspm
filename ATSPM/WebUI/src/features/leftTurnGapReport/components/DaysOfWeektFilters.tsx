import { Box, Checkbox, FormControlLabel } from '@mui/material'

import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'

interface SelectDaysProp {
  selectedDays: number[]
  setSelectedDays(daysOfWeek: number[]): void
}

export const DaysOfWeektFilters = ({
  selectedDays,
  setSelectedDays,
}: SelectDaysProp) => {
  const daysOfWeekList: string[] = [
    'Sunday',
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
  ]

  const handleDayChange = (dayIndex: number) => {
    if (selectedDays.includes(dayIndex)) {
      setSelectedDays(selectedDays.filter((day) => day !== dayIndex).sort())
    } else {
      setSelectedDays([...selectedDays, dayIndex].sort())
    }
  }

  return (
    <Box>
      {/* <Paper
        sx={{
          ...commonPaperStyle,
          maxWidth: '371px',
          '@media (max-width: 1200px)': {},
        }}
      > */}
      <Box>
        <StyledComponentHeader header={'Days to Include'} />
      </Box>
      <Box
        sx={{ display: 'flex', flexDirection: 'column', marginLeft: '25px' }}
      >
        {daysOfWeekList.map((movement, index) => {
          return (
            <FormControlLabel
              key={index}
              control={
                <Checkbox
                  checked={selectedDays.includes(index)}
                  onChange={() => handleDayChange(index)}
                />
              }
              label={movement}
            />
          )
        })}
      </Box>
      {/* </Paper> */}
    </Box>
  )
}
