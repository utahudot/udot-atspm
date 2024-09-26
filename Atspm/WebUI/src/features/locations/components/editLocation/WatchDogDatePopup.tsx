import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import { Box, Button, Paper, Popover } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'

interface WatchDogDatePopupProps {
  anchorEl: null | HTMLElement
  open: boolean
  handlePopoverClose: () => void
  startDate: Date | null
  setStartDate: (date: Date | null) => void
  endDate: Date | null
  setEndDate: (date: Date | null) => void
  handleIgnoreEvent: () => void
  handleRemoveIgnore: () => void
}

const WatchDogDatePopup = ({
  anchorEl,
  open,
  handlePopoverClose,
  startDate,
  setStartDate,
  endDate,
  setEndDate,
  handleIgnoreEvent,
  handleRemoveIgnore,
}: WatchDogDatePopupProps) => {
  const id = open ? 'watchdog-popover' : undefined

  return (
    <Popover
      id={id}
      open={open}
      anchorEl={anchorEl}
      onClose={handlePopoverClose}
      anchorOrigin={{
        vertical: 'bottom',
        horizontal: 'right',
      }}
      transformOrigin={{
        vertical: 'top',
        horizontal: 'right',
      }}
    >
      <Paper>
        <StyledComponentHeader header="Select the range of dates to ignore" />
        <Box sx={{ p: 2 }}>
          <DatePicker
            label="Start Date"
            value={startDate}
            onChange={(newValue) => setStartDate(newValue)}
            sx={{ mr: 2 }}
          />
          <DatePicker
            label="End Date"
            value={endDate}
            onChange={(newValue) => setEndDate(newValue)}
          />
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
            <Button variant="outlined" onClick={handleRemoveIgnore}>
              Remove Ignore
            </Button>
            <Button variant="contained" onClick={handleIgnoreEvent}>
              Ignore Event
            </Button>
          </Box>
        </Box>
      </Paper>
    </Popover>
  )
}

export default WatchDogDatePopup
