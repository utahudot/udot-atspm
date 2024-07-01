import { getDateFromDateStamp } from '@/utils/dateTime'
import { Box, MenuItem, TextField } from '@mui/material'
import { useState } from 'react'
import { LinkPivotPcdHandler } from '../handlers/linkPivotPcdHandlers'

interface props {
  handler: LinkPivotPcdHandler
}

export const LinkPivotPcdOptionsComponent = ({ handler }: props) => {
  const [daySelected, setDaySelected] = useState(handler.daysSelectList[0])

  const updateDateTimeOptions = (val: string) => {
    const date: Date = getDateFromDateStamp(val)
    handler.changeStartDate(date)
    handler.changeEndDate(date)
    setDaySelected(val)
  }

  const updateDaySelected = (val: string) => {
    updateDateTimeOptions(val)
    setDaySelected(val)
  }

  return (
    <Box display="flex">
      <Box>
        <TextField
          select
          label="Date"
          value={
            handler.daysSelectList.find((day) => daySelected === day) || ''
          }
          onChange={(event) => updateDaySelected(event.target.value)}
          size="small"
          sx={{ width: '100%' }}
        >
          {handler.daysSelectList.map((day, index) => (
            <MenuItem key={index} value={day}>
              {day}
            </MenuItem>
          ))}
        </TextField>
      </Box>
      <Box>
        <TextField
          label="Delta"
          type="number"
          defaultValue={handler.delta}
          style={{ width: '50%' }}
          size="small"
          onChange={(event) => {
            handler.changeDelta(parseInt(event.target.value))
          }}
        />
      </Box>
    </Box>
  )
}
