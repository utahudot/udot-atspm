import { Feature } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/hooks/useMapClickHandler'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp'
import LockIcon from '@mui/icons-material/Lock'
import LockOpenIcon from '@mui/icons-material/LockOpen'
import { Box, Button, Collapse, Paper, Typography } from '@mui/material'
import { useState } from 'react'

interface RouteModeIndicatorProps {
  lockedRoute: Feature | null
  onClear: () => void
  polylineCoordinates: [number, number][]
  inEditMode: boolean
}

const RouteModeIndicator = ({
  lockedRoute,
  onClear,
  polylineCoordinates,
  inEditMode,
}: RouteModeIndicatorProps) => {
  const [showModeDetails, setShowModeDetails] = useState(false)

  return (
    <Box>
      <Box
        sx={{
          position: 'absolute',
          top: 10,
          right: 140,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'flex-end',
          zIndex: 1000,
          maxWidth: 320,
        }}
      >
        {lockedRoute === null ? (
          <Box
            sx={{
              bgcolor: 'info.main',
              color: 'info.contrastText',
              px: 2,
              py: 1,
              borderRadius: 1,
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              boxShadow: 1,
              cursor: 'default',
              width: '100%',
              maxHeight: '31px',
            }}
          >
            <LockOpenIcon fontSize="small" />
            <Typography
              variant="body2"
              noWrap
              sx={{ flexGrow: 1, textAlign: 'left' }}
            >
              Freeform
            </Typography>
          </Box>
        ) : (
          <>
            <Button
              variant="contained"
              color="error"
              startIcon={<LockIcon fontSize="small" />}
              endIcon={
                showModeDetails ? (
                  <KeyboardArrowUpIcon fontSize="small" />
                ) : (
                  <KeyboardArrowDownIcon fontSize="small" />
                )
              }
              onClick={() => setShowModeDetails((prev) => !prev)}
              sx={{ justifyContent: 'space-between', width: '100%' }}
            >
              <Typography
                variant="body2"
                noWrap
                sx={{ flexGrow: 1, textAlign: 'left', textTransform: 'none' }}
              >
                Utilizing route: {lockedRoute?.properties?.ROUTE_ALIAS_COMMON}
              </Typography>
            </Button>
            <Collapse in={showModeDetails} unmountOnExit sx={{ zIndex: 1000 }}>
              <Paper
                sx={{ mt: 1, p: 2, maxWidth: '250px', zIndex: 1000 }}
                elevation={3}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Beginning Mileage:
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {lockedRoute?.properties?.BEG_MILEAGE.toFixed(2)}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Ending Mileage:
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {lockedRoute?.properties?.END_MILEAGE.toFixed(2)}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Direction:
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {lockedRoute?.properties?.ROUTE_DIRECTION}
                  </Typography>
                </Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Description:
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {lockedRoute?.properties?.ROUTE_DESC}
                </Typography>
              </Paper>
            </Collapse>
          </>
        )}
      </Box>
      <Button
        sx={{
          position: 'absolute',
          top: 10,
          right: 15,
          zIndex: 1000,
          textTransform: 'none',
        }}
        variant="contained"
        color="error"
        size="small"
        onClick={onClear}
        disabled={polylineCoordinates.length === 0}
      >
        {inEditMode ? 'Reset Segment' : 'Clear Points'}
      </Button>
    </Box>
  )
}

export default RouteModeIndicator
