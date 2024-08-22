import ChartsContainer from '@/features/speedManagementTool/components/detailsPanel/ChartsContainer'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import ArrowBackIosNewOutlinedIcon from '@mui/icons-material/ArrowBackIosNewOutlined'
import { Box, Button, Dialog, DialogContent, Typography } from '@mui/material'
import dynamic from 'next/dynamic'

const StaticMap = dynamic(
  () => import('@/features/speedManagementTool/components/StaticMap'),
  { ssr: false }
)

type SM_PopupProps = {
  route: SpeedManagementRoute
  open: boolean
  onClose: () => void
}

const SM_Popup = ({ route, open, onClose }: SM_PopupProps) => {
  const {
    name,
    speedLimit,
    avg,
    percentilespd_85,
    averageSpeedAboveSpeedLimit,
    enddate,
    estimatedViolations,
    flow,
    percentilespd_15,
    percentilespd_95,
    startdate,
  } = route.properties

  // if screen width is less than 600px, set the width to 100%

  const formattedDateRange =
    startdate && enddate
      ? `for ${new Date(startdate).toLocaleDateString()} - ${new Date(
          enddate
        ).toLocaleDateString()}`
      : null

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullScreen
      maxWidth="md"
      PaperProps={{
        sx: {
          height: '100%',
          width: '100%',
          margin: 'auto',
          maxWidth: 'lg',
        },
      }}
    >
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'right',
          alignItems: 'center',
          padding: 1,
          borderBottom: '1px solid #ccc',
        }}
      >
        <Box width={'100%'}>
          <Button onClick={onClose} startIcon={<ArrowBackIosNewOutlinedIcon />}>
            Back to Map
          </Button>
        </Box>
      </Box>
      <DialogContent>
        <Typography variant="h2" sx={{ mb: 2 }}>
          {name}
        </Typography>
        {formattedDateRange && (
          <Typography variant="subtitle1" sx={{ mb: 2 }}>
            {formattedDateRange}
          </Typography>
        )}
        <Box width={'100%'} mb={2}>
          <StaticMap routes={[route]} />
        </Box>
        <Box>
          <ChartsContainer selectedRouteId={route.properties.route_id} />
        </Box>
      </DialogContent>
    </Dialog>
  )
}

export default SM_Popup
