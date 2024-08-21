import DateRangeOptions from '@/features/speedManagementTool/components/optionsPanel/DateRangeOptions'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Button, Popover } from '@mui/material'
import { format } from 'date-fns'
import { useState } from 'react'

export default function DateRangeOptionsPopup() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { routeSpeedRequest } = useStore()

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'date-range-options-popover' : undefined

  const getDateRangeLabel = () => {
    const { startDate, endDate } = routeSpeedRequest
    // Assuming startDate and endDate are in the format 'YYYY-MM-DD'
    const formattedStartDate = format(new Date(startDate), 'MMM d, yyyy')
    const formattedEndDate = format(new Date(endDate), 'MMM d, yyyy')

    return `${formattedStartDate} - ${formattedEndDate}`
  }

  return (
    <>
      <Button
        variant="outlined"
        endIcon={open ? <ExpandLessIcon /> : <ExpandMoreIcon />}
        onClick={handleOpen}
        sx={{
          textTransform: 'none',
          color: 'black',
          borderColor: 'lightgray',
          width: '250px',
        }}
      >
        {getDateRangeLabel()}
      </Button>
      <Popover
        id={id}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'left',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'left',
        }}
      >
        <DateRangeOptions />
      </Popover>
    </>
  )
}
