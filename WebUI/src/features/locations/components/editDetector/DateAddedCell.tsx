import { TableCell } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { useState } from 'react'

interface DateAddedCellProps {
  value: string
  onUpdate: (newValue: Date) => void
}

function DateAddedCell({ value, onUpdate }: DateAddedCellProps) {
  const [selectedDate, setSelectedDate] = useState(new Date(value))

  const handleChange = (newValue: Date | null) => {
    if (!newValue) return
    setSelectedDate(newValue)
    onUpdate(newValue)
  }

  return (
    <TableCell sx={{ minWidth: '170px' }}>
      <DatePicker
        slotProps={{
          textField: { inputProps: { 'aria-label': 'date-added' } },
        }}
        value={new Date(selectedDate)}
        onChange={handleChange}
        sx={{
          '& fieldset': { border: 'none' },
          '& .MuiInputBase-input': {
            padding: 0,
            cursor: 'pointer',
          },
          '& .MuiOutlinedInput-root': {
            border: 'none',
            paddingRight: '0 !important',
          },
        }}
      />
    </TableCell>
  )
}

export default DateAddedCell
