import { useSidebarStore } from '@/stores/sidebar'
import CloseIcon from '@mui/icons-material/Close'
import { Box, Drawer, IconButton, Typography, useTheme } from '@mui/material'
import { PropsWithChildren } from 'react'

export default function RightSidebar({
  children,
  width = 420,
  title,
}: PropsWithChildren<{ width?: number; title: string }>) {
  const theme = useTheme()
  const { isRightSidebarOpen, closeRightSidebar } = useSidebarStore()

  return (
    <Drawer
      anchor="right"
      variant="persistent"
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
          pb: 0,
          display: 'flex',
          gap: 1,
          justifyContent: 'space-between',
        }}
      >
        <Typography variant="subtitle2" gutterBottom>
          {title}
        </Typography>

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
