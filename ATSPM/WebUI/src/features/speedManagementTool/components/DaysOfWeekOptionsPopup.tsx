import { DaysOfWeekOptions } from '@/features/speedManagementTool/components/optionsPanel/DayOfTheWeekOptions'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Button, Popover } from '@mui/material'
import { useState } from 'react'

export default function DaysOfWeekOptionsPopup() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { routeSpeedRequest } = useStore()

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'days-of-week-options-popover' : undefined

  const getDaysOfWeekLabel = () => {
    const days = routeSpeedRequest.daysOfWeek
    const dayLabels = days.map(
      (day) => ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'][day]
    )

    if (days.length === 7) {
      return 'Whole Week'
    }

    if (
      days.length === 5 &&
      days.includes(1) &&
      days.includes(5) &&
      !days.includes(0) &&
      !days.includes(6)
    ) {
      return 'Work Week'
    }

    return `${dayLabels.join(', ')}`
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
          fontSize: '14px',
        }}
      >
        {getDaysOfWeekLabel()}
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
        <DaysOfWeekOptions />
      </Popover>
    </>
  )
}
