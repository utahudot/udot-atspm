import { useSidebarStore } from '@/stores/sidebar'

import {
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import Link from 'next/link'
import { useRouter } from 'next/router'
import React from 'react'

interface MenuItemProps {
  icon: React.ReactNode
  text: string
  url: string
}

const NavItem = ({ icon, text, url }: MenuItemProps) => {
  const router = useRouter()
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const { closeSideBar } = useSidebarStore()
  const handleClick = () => {
    if (isMobile) {
      closeSideBar()
    }
  }

  const isSelected = router.pathname === url
  const baseColor = theme.palette.primary.main

  return (
    <ListItem
      key={text}
      sx={{
        padding: '0px',
        width: '100%',
        '&:hover': { backgroundColor: baseColor + '30' },
      }}
    >
      <Link href={url} passHref legacyBehavior>
        <ListItemButton
          onClick={handleClick}
          selected={isSelected}
          LinkComponent={'a'}
          sx={{
            color: theme.palette.text.primary,
            '&.Mui-selected': {
              backgroundColor: `${baseColor}30`,
              borderRight: `2px solid ${baseColor}`,
            },
          }}
        >
          <ListItemIcon>{icon}</ListItemIcon>
          <ListItemText>
            <Typography fontWeight={400}>{text}</Typography>
          </ListItemText>
        </ListItemButton>
      </Link>
    </ListItem>
  )
}

export default NavItem
