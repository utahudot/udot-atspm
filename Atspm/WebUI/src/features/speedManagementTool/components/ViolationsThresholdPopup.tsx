import useStore from '@/features/speedManagementTool/speedManagementStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Box, Button, InputAdornment, Popover, TextField } from '@mui/material'
import { useState } from 'react'

export default function ViolationsThresholdPopup() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? 'violations-threshold-popover' : undefined

  const handleViolationsThresholdChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      violationThreshold: parseInt(event.target.value, 10),
    })
  }

  return (
    <>
      <Button
        variant="outlined"
        endIcon={open ? <ExpandLessIcon /> : <ExpandMoreIcon />}
        onClick={handleOpen}
        sx={{
          textTransform: 'none',
          borderColor: 'lightgray',
          color: 'black',
        }}
      >
        Violations Threshold: {routeSpeedRequest.violationThreshold}%
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
        <Box padding={'10px'} marginTop={'10px'}>
          <TextField
            label="Threshold"
            variant="outlined"
            size="small"
            type="number"
            value={routeSpeedRequest.violationThreshold}
            onChange={handleViolationsThresholdChange}
            InputProps={{
              endAdornment: <InputAdornment position="end">%</InputAdornment>,
            }}
            fullWidth
          />
        </Box>
      </Popover>
    </>
  )
}
