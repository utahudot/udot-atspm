import { IconButton, Menu } from '@mui/material'
import { PropsWithChildren, useState } from 'react'

const DropDownMenu = ({
  icon,
  children,
}: PropsWithChildren & { icon: React.ReactNode }) => {
  const [anchorElement, setAnchorElement] = useState<null | HTMLElement>(null)

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorElement(event.currentTarget)
  }

  const handleMenuClose = () => {
    setAnchorElement(null)
  }

  return (
    <>
      <IconButton
        aria-haspopup="true"
        aria-label="info menu"
        aria-expanded={Boolean(anchorElement)}
        onClick={handleClick}
      >
        {icon}
      </IconButton>
      {anchorElement && (
        <Menu
          anchorEl={anchorElement}
          open={Boolean(anchorElement)}
          onClose={handleMenuClose}
        >
          {children}
        </Menu>
      )}
    </>
  )
}

export default DropDownMenu
