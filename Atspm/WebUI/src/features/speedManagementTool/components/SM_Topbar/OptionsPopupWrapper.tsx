import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Button, Popover } from '@mui/material'
import { ReactNode, useState } from 'react'

interface OptionsPopupWrapperProps {
  label: string
  getLabel: () => string
  children: ReactNode
  buttonStyles?: object
}

export default function OptionsPopupWrapper({
  label,
  getLabel,
  children,
  buttonStyles = {},
}: OptionsPopupWrapperProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)
  const id = open ? `${label}-options-popover` : undefined

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
          ...buttonStyles,
        }}
      >
        {getLabel()}
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
        {children}
      </Popover>
    </>
  )
}
