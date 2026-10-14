import { useSidebarStore } from '@/stores/sidebar'
import CloseIcon from '@mui/icons-material/Close'
import { Box, Drawer, IconButton, Typography, useTheme } from '@mui/material'
import { PropsWithChildren } from 'react'

export default function RightSidebar({
  children,
  width = 420,
  title,
  subtitle,
  dismissOnBackdrop = false,
}: PropsWithChildren<{
  width?: number
  title: string
  subtitle?: string
  dismissOnBackdrop?: boolean
}>) {
  const theme = useTheme()
  const { isRightSidebarOpen, closeRightSidebar } = useSidebarStore()

  return (
    <Drawer
      anchor="right"
      variant={dismissOnBackdrop ? 'temporary' : 'persistent'}
      open={isRightSidebarOpen}
      onClose={closeRightSidebar}
      PaperProps={{
        sx: {
          height: `calc(100%)`,
          width,
          border: 'none',
          boxShadow: 3,
          backgroundColor: theme.palette.background.paper,
          display: 'flex',
          flexDirection: 'column',
        },
      }}
      ModalProps={{ keepMounted: true }}
    >
      <Box
        sx={{
          p: 2,
          pb: subtitle ? 1.5 : 0,
          display: 'flex',
          alignItems: 'flex-start',
          gap: 1,
          justifyContent: 'space-between',
          borderBottom: subtitle ? 1 : 0,
          borderColor: 'divider',
        }}
      >
        <Box>
          <Typography variant="subtitle2" sx={{ mb: subtitle ? 0.25 : 0 }}>
            {title}
          </Typography>
          {subtitle && (
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ display: 'block', lineHeight: 1.4 }}
            >
              {subtitle}
            </Typography>
          )}
        </Box>

        <IconButton
          size="small"
          onClick={closeRightSidebar}
          aria-label="Collapse sidebar"
        >
          <CloseIcon fontSize="small" />
        </IconButton>
      </Box>
      {children}
    </Drawer>
  )
}
