import LoginOutlinedIcon from '@mui/icons-material/LoginOutlined'
import PersonOutlineOutlinedIcon from '@mui/icons-material/PersonOutlineOutlined'

import Link from 'next/link'

import { useUserInfo } from '@/features/identity/api/getUserInfo'
import Login from '@/features/identity/components/signin'
import { useSidebarStore } from '@/stores/sidebar'
import {
  Avatar,
  Box,
  Dialog,
  IconButton,
  Menu,
  MenuItem,
  Typography,
} from '@mui/material'
import Cookies from 'js-cookie'
import React, { useEffect, useState } from 'react'

function getColorFromName(firstName: string, lastName: string): string {
  const colors = [
    '#1e824c',
    '#007a7c',
    '#00552a',
    '#2574a9',
    '#406098',
    '#1460aa',
    '#0a3055',
    '#000060',
    '#8859b6',
    '#a74165',
    '#8a2be2',
    '#8d6708',
    '#d43900',
    '#802200',
    '#dc2a2a',
    '#aa0000',
    '#5c0819',
  ]

  // Extract the first two letters of the first and last name
  const firstTwoFirstName = firstName.slice(0, 2).toUpperCase()
  const firstTwoLastName = lastName.slice(0, 2).toUpperCase()

  // Combine the letters
  const combinedLetters = firstTwoFirstName + firstTwoLastName

  // Hash the combined letters
  function hashString(str: string): number {
    let hash = 0
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i)
      hash = (hash << 5) - hash + char
      hash |= 0 // Convert to 32bit integer
    }
    return hash
  }

  const hash = hashString(combinedLetters)

  // Use the hash to select a color from the array
  const colorIndex = Math.abs(hash) % colors.length
  return colors[colorIndex]
}

export default function UserMenu() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const { data: userData, refetch } = useUserInfo({})
  const { closeSideBar } = useSidebarStore()

  useEffect(() => {
    if (isLoggedIn) {
      refetch()
    }
  }, [isLoggedIn, refetch])

  const [anchorElement, setAnchorElement] = useState<null | HTMLElement>(null)
  const [openLogin, setOpenLogin] = useState(false)

  const handleLoginOpen = () => {
    setOpenLogin(true)
  }

  const handleSignOut = () => {
    const cookies = Cookies.get()
    Object.entries(cookies).forEach((value) => Cookies.remove(value[0]))
    window.location.href = '/'
  }

  const handleLoginClose = () => {
    setOpenLogin(false)
  }

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorElement(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorElement(null)
    closeSideBar()
  }

  useEffect(() => {
    setIsLoggedIn(!!Cookies.get('loggedIn'))
  }, [])

  return (
    <>
      <IconButton
        id="userMenu"
        aria-controls="userMenu"
        aria-haspopup="true"
        aria-label="User Menu"
        onClick={handleClick}
      >
        <Avatar
          variant="rounded"
          sx={{
            bgcolor:
              isLoggedIn && userData
                ? getColorFromName(userData?.firstName, userData?.lastName)
                : '',
          }}
        >
          {isLoggedIn ? (
            <>
              {userData?.firstName.charAt(0).toUpperCase()}
              {userData?.lastName.charAt(0).toUpperCase()}
            </>
          ) : (
            <PersonOutlineOutlinedIcon />
          )}
        </Avatar>
      </IconButton>
      {anchorElement && (
        <Menu
          id="userMenu"
          anchorEl={anchorElement}
          open={Boolean(anchorElement)}
          onClose={handleClose}
          MenuListProps={{
            'aria-labelledby': 'userMenu',
            role: 'menu',
          }}
          slotProps={{
            paper: {
              sx: {
                minWidth: 150,
              },
            },
          }}
        >
          {isLoggedIn && (
            <MenuItem component={Link} href={'/user/profile'}>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  flexGrow: 1,
                  justifyContent: 'space-between',
                }}
              >
                <PersonOutlineOutlinedIcon fontSize="small" />
                <Typography>Profile</Typography>
              </Box>
            </MenuItem>
          )}
          {/* <ListItemButton
            onClick={() => (isLoggedIn ? handleSignOut() : handleLoginOpen())}
            role="menuitem"
          >
            <ListItemIcon>
              <LoginOutlinedIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary={isLoggedIn ? 'Log out' : 'Log in'} />
          </ListItemButton> */}
          <MenuItem
            onClick={() => (isLoggedIn ? handleSignOut() : handleLoginOpen())}
          >
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                flexGrow: 1,
                justifyContent: 'space-between',
              }}
            >
              <LoginOutlinedIcon fontSize="small" />
              <Typography>{isLoggedIn ? 'Log out' : 'Log in'}</Typography>
            </Box>
          </MenuItem>
        </Menu>
      )}
      {openLogin && (
        <Dialog onClose={handleLoginClose} open={openLogin}>
          <Login />
        </Dialog>
      )}
    </>
  )
}
