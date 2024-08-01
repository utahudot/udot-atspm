import Sidebar from '@/components/sidebar/Sidebar'
import Toast from '@/components/toast'
import { Box, useTheme } from '@mui/material'
import React from 'react'
import Topbar from './topbar'

interface LayoutProps {
  children: React.ReactNode
}

export default function Layout({ children }: LayoutProps) {
  const theme = useTheme()

  return (
    <Box className="app" sx={{ display: 'flex' }}>
      <Box
        component="main"
        className="content"
        sx={{
          backgroundColor: theme.palette.background.default,
        }}
      >
        <Topbar />
        <Box
          sx={{
            minHeight: `calc(100vh - 73px)`,
            width: '100%',
            p: 3,
            transition: 'width 0.3s ease-out',
            [theme.breakpoints.down('sm')]: {
              padding: theme.spacing(1),
            },
            [theme.breakpoints.down('xs')]: {
              padding: theme.spacing(0),
            },
          }}
        >
          {children}
        </Box>
        <Sidebar />
        <Toast />
      </Box>
    </Box>
  )
}
