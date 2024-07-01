import LoginOutlinedIcon from '@mui/icons-material/LoginOutlined'
import PersonOutlineOutlinedIcon from '@mui/icons-material/PersonOutlineOutlined'

import Link from 'next/link'

import { useUserInfo } from '@/features/identity/api/getUserInfo'
import Login from '@/features/identity/components/signin'
import {
  Avatar,
  Dialog,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
} from '@mui/material'
import Cookies from 'js-cookie'
import React, { useEffect, useState } from 'react'

interface ItemProps {
  index: number
  item: {
    name: string
    icon: JSX.Element
    link: string
  }
  // handleClick: (
  //   event: (event: MouseEvent<HTMLButtonElement, MouseEvent>) => void
  // )
}

function getColorFromName(firstName, lastName) {
  // Extract the first two letters of the first and last name
  const firstTwoFirstName = firstName.slice(0, 2).toUpperCase()
  const firstTwoLastName = lastName.slice(0, 2).toUpperCase()

  // Combine the letters
  const combinedLetters = firstTwoFirstName + firstTwoLastName

  // Hash the combined letters
  function hashString(str) {
    let hash = 0
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i)
      hash = (hash << 5) - hash + char
      hash |= 0 // Convert to 32bit integer
    }
    return hash
  }

  const hash = hashString(combinedLetters)

  // Generate a random color from the hash integer
  function intToRGB(i) {
    const c = (i & 0x00ffffff).toString(16).toUpperCase()

    return '00000'.substring(0, 6 - c.length) + c
  }

  const color = `#${intToRGB(hash)}`

  return color
}

const ListSubMenuItem = ({ index, item }: ItemProps) => {
  return (
    <ListItemButton
      key={index}
      // onClick={handleClick}
      component={Link}
      href={item.link}
    >
      <ListItemIcon>{item.icon}</ListItemIcon>
      <ListItemText primary={item.name} />
    </ListItemButton>
  )
}

const userItems = [
  {
    name: 'Profile',
    icon: <PersonOutlineOutlinedIcon fontSize="small" />,
    link: '/user/profile',
  },
]

export default function UserMenu() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const { data: userData, refetch } = useUserInfo({})

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
    window.location.href = '/locations'
  }

  const handleLoginClose = () => {
    setOpenLogin(false)
  }

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorElement(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorElement(null)
  }

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Enter') {
      isLoggedIn ? handleSignOut() : handleLoginOpen()
    }
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
        >

            {isLoggedIn &&
              userItems.map((item, index) => (
                <ListSubMenuItem
                  key={index}
                  index={index}
                  item={item}
                  // handleClick={handleClose}
                />
              ))}
            <ListItemButton
              onClick={() => (isLoggedIn ? handleSignOut() : handleLoginOpen())}
              role="menuitem"
            >
              <ListItemIcon>
                <LoginOutlinedIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText primary={isLoggedIn ? 'Log out' : 'Log in'} />
            </ListItemButton>

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
