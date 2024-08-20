import GeneralOptions from '@/features/speedManagementTool/components/optionsPanel/GeneralOptions'
import { DataSource } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Button, Popover } from '@mui/material'
import { useState } from 'react'

export default function GeneralOptionsPopup() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { routeSpeedRequest } = useStore()

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'general-options-popover' : undefined

  const getDataSourceLabel = () => {
    switch (routeSpeedRequest.sourceId) {
      case DataSource.ATSPM:
        return 'ATSPM'
      case DataSource.PeMS:
        return 'PeMS'
      case DataSource.ClearGuide:
        return 'ClearGuide'
      default:
        return 'Select Data Source'
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
          minWidth: '130px',
        }}
      >
        {getDataSourceLabel()}
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
        <GeneralOptions />
      </Popover>
    </>
  )
}
