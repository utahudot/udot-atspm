import { useSidebarStore } from '@/stores/sidebar'

import {
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
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
  const { toggleSidebar } = useSidebarStore()
  const handleClick = () => {
    toggleSidebar()
  }

  const isSelected = router.pathname === url
  const baseColor = theme.palette.primary.main

  return (
    <ListItem
      key={text}
      sx={{
        padding: 0,
        width: '100%',
        '&:hover': { backgroundColor: baseColor + '30' },
      }}
    >
      <Link
        href={url}
        style={{ width: '100%', textDecoration: 'none', color: 'inherit' }}
      >
        <ListItemButton
          onClick={handleClick}
          selected={isSelected}
          sx={{
            py: 0.5,
            color: theme.palette.text.primary,
            '&.Mui-selected': {
              backgroundColor: `${baseColor}30`,
              borderRight: `4px solid ${baseColor}`,
            },
          }}
        >
          <ListItemIcon sx={{ minWidth: '40px' }}>{icon}</ListItemIcon>
          <ListItemText>
            <Typography fontWeight={400}>{text}</Typography>
          </ListItemText>
        </ListItemButton>
      </Link>
    </ListItem>
  )
}

export default NavItem
