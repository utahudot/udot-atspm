import AnalysisPeriodOptions from '@/features/speedManagementTool/components/optionsPanel/AnalysisPeriodOptions'
import { AnalysisPeriod } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Button, Popover } from '@mui/material'
import { useState } from 'react'

export default function AnalysisPeriodOptionsPopup() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { routeSpeedRequest } = useStore()

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'analysis-period-options-popover' : undefined

  const getAnalysisPeriodLabel = () => {
    switch (routeSpeedRequest.analysisPeriod) {
      case AnalysisPeriod.AllDay:
        return 'Whole Day'
      case AnalysisPeriod.PeekPeriod:
        return 'Peak Periods'
      case AnalysisPeriod.CustomHour:
        return `Custom: ${routeSpeedRequest.customStartTime?.format(
          'HH:mm'
        )} - ${routeSpeedRequest.customEndTime?.format('HH:mm')}`
      default:
        return 'Analysis Period'
    }
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
        }}
      >
        {getAnalysisPeriodLabel()}
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
        <AnalysisPeriodOptions />
      </Popover>
    </>
  )
}
