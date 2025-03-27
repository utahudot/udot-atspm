import { useSidebarStore } from '@/stores/sidebar'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import {
  Box,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  MenuItem,
  Paper,
  Popper,
  Typography,
  useTheme,
} from '@mui/material'
import { useRouter } from 'next/router'
import React from 'react'

type SubItem = {
  text: string
  url: string
}

type NavItemFlyoutProps = {
  icon?: React.ReactNode
  text: string
  subItems: SubItem[]
}

export default function NavItemFlyout({
  icon,
  text,
  subItems,
}: NavItemFlyoutProps) {
  const router = useRouter()
  const theme = useTheme()
  const { toggleSidebar } = useSidebarStore()

  const [open, setOpen] = React.useState(false)
  const anchorRef = React.useRef<HTMLDivElement | null>(null)
  const popperRef = React.useRef<HTMLDivElement | null>(null)

  const handleParentMouseEnter = () => {
    setOpen(true)
  }

  const handleParentMouseLeave = (e: React.MouseEvent) => {
    if (popperRef.current?.contains(e.relatedTarget as Node)) {
      return
    }
    setOpen(false)
  }

  const handlePopperMouseEnter = () => {
    setOpen(true)
  }

  const handlePopperMouseLeave = (e: React.MouseEvent) => {
    if (anchorRef.current?.contains(e.relatedTarget as Node)) {
      return
    }
    setOpen(false)
  }

  const handleMenuItemClick = (url: string) => {
    toggleSidebar()
    router.push(url)
    setOpen(false)
  }

  const isChildActive = subItems.some((item) => router.asPath === item.url)
  const baseColor = theme.palette.primary.main

  return (
    <>
      <Box
        ref={anchorRef}
        onMouseEnter={handleParentMouseEnter}
        onMouseLeave={handleParentMouseLeave}
      >
        <ListItemButton
          selected={isChildActive}
          sx={{
            py: 0.5,
            color: theme.palette.text.primary,
            '&.Mui-selected': {
              backgroundColor: `${baseColor}30`,
              borderRight: `4px solid ${baseColor}`,
            },
            '&:hover': {
              backgroundColor: `${baseColor}30`,
            },
          }}
        >
          {icon && (
            <ListItemIcon sx={{ minWidth: '40px' }}>{icon}</ListItemIcon>
          )}
          <ListItemText>
            <Typography fontWeight={400}>{text}</Typography>
          </ListItemText>
          <ChevronRightIcon fontSize="small" />
        </ListItemButton>
      </Box>
      <Popper
        open={open}
        anchorEl={anchorRef.current}
        placement="right-start"
        style={{ zIndex: theme.zIndex.modal }}
      >
        <Paper
          ref={popperRef}
          onMouseEnter={handlePopperMouseEnter}
          onMouseLeave={handlePopperMouseLeave}
          sx={{ py: 1 }}
        >
          {subItems.map(({ text: subText, url }) => {
            const selected = router.asPath === url
            return (
              <MenuItem
                key={url}
                onClick={() => handleMenuItemClick(url)}
                selected={selected}
                sx={{
                  color: theme.palette.text.primary,
                  '&.Mui-selected': {
                    backgroundColor: `${baseColor}30`,
                  },
                  '&:hover': {
                    backgroundColor: `${baseColor}30`,
                  },
                }}
              >
                {subText}
              </MenuItem>
            )
          })}
        </Paper>
      </Popper>
    </>
  )
}
