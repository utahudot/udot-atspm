import RoutesToggle from '@/features/speedManagementTool/components/RoutesToggle'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import DisplaySettingsOutlinedIcon from '@mui/icons-material/DisplaySettingsOutlined'
import {
  Box,
  Button,
  FormControlLabel,
  Paper,
  Popper,
  Switch,
  Tooltip,
} from '@mui/material'
import { useCallback, useEffect, useRef, useState } from 'react'

type RouteDisplayToggleProps = {
  includeNoDataSegments: boolean
  setIncludeNoDataSegments: (include: boolean) => void
}

const RouteDisplayToggle = ({
  includeNoDataSegments,
  setIncludeNoDataSegments,
}: RouteDisplayToggleProps) => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const { mapRef } = useSpeedManagementStore()
  const pointerInsideOptionsRef = useRef(false)
  const pointerDownInOptionsRef = useRef(false)
  const open = Boolean(anchorEl)

  const handleDisplaySettingsClick = (
    event: React.MouseEvent<HTMLButtonElement>
  ) => {
    setAnchorEl(anchorEl ? null : event.currentTarget)
  }

  const disableMapDragging = useCallback(() => {
    mapRef?.dragging?.disable()
  }, [mapRef])

  const enableMapDragging = useCallback(() => {
    mapRef?.dragging?.enable()
  }, [mapRef])

  useEffect(() => {
    const handlePointerUp = () => {
      pointerDownInOptionsRef.current = false
      if (!pointerInsideOptionsRef.current) {
        enableMapDragging()
      }
    }

    window.addEventListener('pointerup', handlePointerUp)
    window.addEventListener('mouseup', handlePointerUp)
    window.addEventListener('touchend', handlePointerUp)
    return () => {
      window.removeEventListener('pointerup', handlePointerUp)
      window.removeEventListener('mouseup', handlePointerUp)
      window.removeEventListener('touchend', handlePointerUp)
      enableMapDragging()
    }
  }, [enableMapDragging])

  useEffect(() => {
    if (!open) {
      pointerInsideOptionsRef.current = false
      pointerDownInOptionsRef.current = false
      enableMapDragging()
    }
  }, [enableMapDragging, open])

  const handleOptionsPointerEnter = () => {
    pointerInsideOptionsRef.current = true
    disableMapDragging()
  }

  const handleOptionsPointerLeave = () => {
    pointerInsideOptionsRef.current = false
    if (!pointerDownInOptionsRef.current) {
      enableMapDragging()
    }
  }

  const handleOptionsPointerDown = () => {
    pointerDownInOptionsRef.current = true
    disableMapDragging()
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
      <Tooltip title="Toggle Segment Display">
        <Button
          sx={{
            px: 1,
            minWidth: 0,
            textTransform: 'none',
          }}
          variant="contained"
          size="small"
          onClick={handleDisplaySettingsClick}
        >
          Display &nbsp; <DisplaySettingsOutlinedIcon />
        </Button>
      </Tooltip>
      <Popper
        open={open}
        anchorEl={anchorEl}
        placement="bottom-start"
        disablePortal
      >
        <Paper
          sx={{ width: '300px' }}
          onPointerEnter={handleOptionsPointerEnter}
          onPointerLeave={handleOptionsPointerLeave}
          onPointerDownCapture={handleOptionsPointerDown}
          onMouseDownCapture={disableMapDragging}
          onTouchStartCapture={disableMapDragging}
        >
          <RoutesToggle setAnchorEl={setAnchorEl} />
          <Box
            sx={{
              px: 2,
              py: 1,
              borderTop: '1px solid',
              borderColor: 'divider',
            }}
          >
            <FormControlLabel
              control={
                <Switch
                  checked={includeNoDataSegments}
                  onChange={(event) =>
                    setIncludeNoDataSegments(event.target.checked)
                  }
                  size="small"
                />
              }
              label="Include segments without data"
            />
          </Box>
        </Paper>
      </Popper>
    </Box>
  )
}

export default RouteDisplayToggle
