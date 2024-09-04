import RoutesToggle from '@/features/speedManagementTool/components/RoutesToggle'
import ViolationRangeSlider from '@/features/speedManagementTool/components/RoutesToggle/ViolationRangeSlider'
import DisplaySettingsOutlinedIcon from '@mui/icons-material/DisplaySettingsOutlined'
import { Box, Button, Paper, Popper, Tooltip } from '@mui/material'
import { useState } from 'react'

const RouteDisplayToggle = () => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const open = Boolean(anchorEl)

  const handleDisplaySettingsClick = (
    event: React.MouseEvent<HTMLButtonElement>
  ) => {
    setAnchorEl(anchorEl ? null : event.currentTarget)
  }

  return (
    <Box
      sx={{
        position: 'absolute',
        right: '10px',
        top: '50px',
        zIndex: 1000,
      }}
    >
      <Tooltip title="Toggle Route Display">
        <Button
          sx={{
            px: 1,
            minWidth: 0,
          }}
          variant="contained"
          onClick={handleDisplaySettingsClick}
        >
          <DisplaySettingsOutlinedIcon />
        </Button>
      </Tooltip>
      <Popper
        open={open}
        anchorEl={anchorEl}
        placement="bottom-start"
        disablePortal
      >
        <Paper sx={{ width: '300px' }}>
          <RoutesToggle />
          <ViolationRangeSlider />
        </Paper>
      </Popper>
    </Box>
  )
}

export default RouteDisplayToggle
