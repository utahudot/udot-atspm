import SeparateWindow from '@/features/speedManagementTool/components/NewWindow' // Assuming you save SeparateWindow in the same folder
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ArrowBack, Close, OpenInNew } from '@mui/icons-material'
import {
  Box,
  Dialog,
  DialogContent,
  IconButton,
  Typography,
} from '@mui/material'
import { createPortal } from 'react-dom'

type SM_PopupProps = {
  route: SpeedManagementRoute
  onClose: () => void
  open: boolean
  isSeparateScreen?: boolean
  onPopBack?: () => void
  onOpenSeparateScreen?: () => void
}

const SM_Popup = ({
  route,
  onClose,
  open,
  isSeparateScreen = false,
  onPopBack,
  onOpenSeparateScreen,
}: SM_PopupProps) => {
  if (isSeparateScreen) {
    return (
      <SeparateWindow onClose={onPopBack}>
        <Dialog
          open={open}
          onClose={onClose}
          PaperProps={{
            sx: {
              height: '100%',
              width: '95%',
              margin: 'auto',
              position: 'relative',
            },
          }}
        >
          {/* Action Bar */}
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'flex-end',
              alignItems: 'center',
              padding: 1,
              borderBottom: '1px solid #ccc',
            }}
          >
            <IconButton onClick={onPopBack}>
              <ArrowBack />
            </IconButton>
            <IconButton onClick={onClose}>
              <Close />
            </IconButton>
          </Box>
          <DialogContent dividers>
            {/* Title Inside Content */}
            <Box sx={{ marginBottom: 2 }}>
              <Typography variant="h6">{route.properties.name}</Typography>
            </Box>
            <Box sx={{ fontWeight: 'bold' }}>
              <Typography>
                Speed Limit: {route.properties.speedLimit}
              </Typography>
              <Typography>Average Speed: {route.properties.avg}</Typography>
              {route.properties.percentilespd_85 && (
                <Typography>
                  85th Percentile: {route.properties.percentilespd_85}
                </Typography>
              )}
            </Box>
          </DialogContent>
        </Dialog>
      </SeparateWindow>
    )
  }

  return createPortal(
    <Dialog
      open={open}
      onClose={onClose}
      PaperProps={{
        sx: {
          height: 'auto',
          width: '95%',
          margin: 'auto',
          position: 'relative',
        },
      }}
    >
      {/* Action Bar */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          alignItems: 'center',
          padding: 1,
          borderBottom: '1px solid #ccc',
        }}
      >
        <IconButton onClick={onOpenSeparateScreen}>
          <OpenInNew />
        </IconButton>
        <IconButton onClick={onClose}>
          <Close />
        </IconButton>
      </Box>
      <DialogContent dividers>
        {/* Title Inside Content */}
        <Box sx={{ marginBottom: 2 }}>
          <Typography variant="h6">{route.properties.name}</Typography>
        </Box>
        <Box sx={{ fontWeight: 'bold' }}>
          <Typography>Speed Limit: {route.properties.speedLimit}</Typography>
          <Typography>Average Speed: {route.properties.avg}</Typography>
          {route.properties.percentilespd_85 && (
            <Typography>
              85th Percentile: {route.properties.percentilespd_85}
            </Typography>
          )}
        </Box>
      </DialogContent>
    </Dialog>,
    document.body
  )
}

export default SM_Popup
