import { ColorModeContext } from '@/theme'
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined'
import LightModeOutlinedIcon from '@mui/icons-material/LightModeOutlined'

import { IconButton, useTheme } from '@mui/material'
import { useContext } from 'react'

const ColorModeToggle = () => {
  const theme = useTheme()
  const colorMode = useContext(ColorModeContext)

  return (
    <IconButton onClick={colorMode.toggleColorMode} aria-label="Change Theme">
      {theme.palette.mode === 'dark' ? (
        <DarkModeOutlinedIcon />
      ) : (
        <LightModeOutlinedIcon />
      )}
    </IconButton>
  )
}

export default ColorModeToggle
