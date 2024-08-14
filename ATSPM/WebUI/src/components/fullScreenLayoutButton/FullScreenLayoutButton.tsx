import FullscreenIcon from '@mui/icons-material/Fullscreen'
import FullscreenExitIcon from '@mui/icons-material/FullscreenExit'
import { Button } from '@mui/material'
import React, { useCallback, useEffect, useState } from 'react'

type FullScreenToggleButtonProps = {
  targetRef: React.RefObject<HTMLElement>
}

const FullScreenToggleButton: React.FC<FullScreenToggleButtonProps> = ({
  targetRef,
}) => {
  const [isFullscreen, setIsFullscreen] = useState(false)

  const checkFullscreen = useCallback(() => {
    setIsFullscreen(!!document.fullscreenElement)
  }, [])

  const handleFullscreenToggle = (e: MouseEvent) => {
    e.stopPropagation()
    if (!isFullscreen) {
      if (targetRef.current?.requestFullscreen) {
        targetRef.current.requestFullscreen()
      }
    } else if (document.exitFullscreen) {
      document.exitFullscreen()
    }
  }

  useEffect(() => {
    document.addEventListener('fullscreenchange', checkFullscreen)
    return () => {
      document.removeEventListener('fullscreenchange', checkFullscreen)
    }
  }, [checkFullscreen])

  return (
    <Button
      onClick={handleFullscreenToggle}
      sx={{
        backgroundColor: 'primary.main',
        color: 'white',
        px: 1,
        minWidth: 0,
      }}
      variant="contained"
    >
      {isFullscreen ? (
        <FullscreenExitIcon fontSize="small" />
      ) : (
        <FullscreenIcon fontSize="small" />
      )}
    </Button>
  )
}

export default FullScreenToggleButton
