import { useGetAdminPagesList } from '@/features/identity/pagesCheck'
import { Button, Menu, MenuItem, Typography, useTheme } from '@mui/material'
import NextLink from 'next/link'
import React, { useState } from 'react'

const AdminMenu = () => {
  const theme = useTheme()

  const [anchorElement, setAnchorElement] = useState<null | HTMLElement>(null)
  const [open, setOpen] = useState(false)
  const pagesToLinks = useGetAdminPagesList()

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setOpen(!open)
    setAnchorElement(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorElement(null)
    setOpen(!open)
  }

  return (
    <>
      <Button
        id="adminMenu"
        aria-controls="adminMenu"
        aria-haspopup="true"
        aria-expanded={Boolean(anchorElement)}
        onClick={handleClick}
      >
        <Typography variant="button" sx={{ color: theme.palette.error.main }}>
          Admin
        </Typography>
      </Button>
      {anchorElement && (
        <Menu
          id="adminMenu"
          anchorEl={anchorElement}
          open={Boolean(anchorElement)}
          onClose={handleClose}
          MenuListProps={{
            'aria-labelledby': 'admin-button',
          }}
        >
          {Array.from(pagesToLinks.keys()).map((key) => {
            return (
              <MenuItem
                key={key}
                href={pagesToLinks.get(key) as string}
                component={NextLink}
                onClick={handleClose}
              >
                {key}
              </MenuItem>
            )
          })}
        </Menu>
      )}
    </>
  )
}

export default AdminMenu
