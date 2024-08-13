import { useSidebarStore } from '@/stores/sidebar'
import { navigateToPage } from '@/utils/routes'
import { Button, Menu, MenuItem, Typography } from '@mui/material'
import { ArrowDropDownIcon } from '@mui/x-date-pickers'
import { useState } from 'react'

const DropDownButton = ({
  title,
  menuItems,
  disabled,
}: {
  title: string
  icon: JSX.Element
  menuItems: { name: string; icon?: JSX.Element; link: string }[]
  disabled?: boolean
}) => {
  const [anchorEl, setAnchorEl] = useState<HTMLButtonElement | null>(null)
  const { closeSideBar } = useSidebarStore()
  const open = Boolean(anchorEl)

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    if (!disabled) {
      setAnchorEl(event.currentTarget)
    }
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleNavigation = (path: string) => {
    closeSideBar()
    navigateToPage(path)
    setAnchorEl(null)
  }

  return (
    <>
      <Button
        color={title === 'Admin' ? 'error' : 'inherit'}
        variant={title === 'Admin' ? 'outlined' : 'text'}
        aria-controls={open ? 'menu-list-grow' : undefined}
        aria-haspopup="true"
        aria-expanded={open ? 'true' : undefined}
        onClick={handleClick}
        endIcon={<ArrowDropDownIcon />}
        sx={{
          mx: '2px',
          color: 'black',
          textTransform: 'none',
          '& .MuiButton-endIcon': { ml: '0px' },
        }}
        disabled={disabled}
      >
        <Typography fontWeight={400} sx={{ textTransform: 'none' }}>
          {title}
        </Typography>
      </Button>
      <Menu
        id="menu-list-grow"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
      >
        {menuItems.map((item) => (
          <MenuItem key={item.name} onClick={() => handleNavigation(item.link)}>
            <Typography
              sx={{
                display: 'flex',
                alignItems: 'center',
                textDecoration: 'none',
              }}
            >
              {item.name}
            </Typography>
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}

export default DropDownButton
