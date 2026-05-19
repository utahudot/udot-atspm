import { Feature } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/hooks/useMapClickHandler'
import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
  useTheme,
} from '@mui/material'

interface RouteSelectionDialogProps {
  open: boolean
  routeOptions: Feature[]
  onSelect: (feature: Feature | null) => void
  onClose: () => void
}

export const RouteSelectionDialog = ({
  open,
  routeOptions,
  onSelect,
  onClose,
}: RouteSelectionDialogProps) => {
  const theme = useTheme()

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth={'md'}>
      <DialogTitle>
        <Typography variant="h6" gutterBottom>
          Choose a Route to Utilize
        </Typography>
        <Typography variant="body2" color="textSecondary">
          You clicked between multiple routes. Select one or draw manually.
        </Typography>
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="row"
          flexWrap="wrap"
          sx={{
            columnGap: 2,
            rowGap: 2,
          }}
        >
          {routeOptions.map((feature) => {
            const isPositive = feature.properties.ROUTE_DIRECTION === 'P'
            return (
              <Card
                key={feature.properties.ROUTE_ID}
                sx={{
                  width: 200,
                  boxShadow: 1,
                  display: 'flex',
                  flexDirection: 'column',
                  '&:hover': { boxShadow: 6 },
                }}
              >
                <CardActionArea
                  onClick={() => onSelect(feature)}
                  sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}
                >
                  <CardContent
                    sx={{
                      flex: 1,
                      display: 'flex',
                      flexDirection: 'column',
                      justifyContent: 'flex-start',
                    }}
                  >
                    <Typography variant="h6" noWrap>
                      {feature.properties.ROUTE_ID}
                    </Typography>
                    <Typography variant="caption" color="textSecondary">
                      Direction:{' '}
                      <Box
                        component="span"
                        sx={{
                          color: isPositive
                            ? theme.palette.success.main
                            : theme.palette.error.main,
                        }}
                      >
                        {isPositive ? 'Positive' : 'Negative'}
                      </Box>
                    </Typography>
                    <Typography variant="caption" color="textSecondary">
                      Begin Mile: {feature.properties.BEG_MILEAGE.toFixed(2)}
                    </Typography>
                    <Typography variant="caption" color="textSecondary">
                      End Mile: {feature.properties.END_MILEAGE.toFixed(2)}
                    </Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            )
          })}

          <Card
            sx={{
              width: 200,
              boxShadow: 1,
              display: 'flex',
              flexDirection: 'column',
              '&:hover': { boxShadow: 6 },
            }}
          >
            <CardActionArea
              onClick={() => onSelect(null)}
              sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}
            >
              <CardContent
                sx={{
                  flex: 1,
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'flex-start',
                  bgcolor: theme.palette.grey[100],
                }}
              >
                <Typography variant="h6">Manual Segment</Typography>
                <Typography
                  variant="caption"
                  color={theme.palette.text.secondary}
                  sx={{ mt: 1 }}
                >
                  Draw freely without snapping
                </Typography>
              </CardContent>
            </CardActionArea>
          </Card>
        </Stack>
      </DialogContent>
    </Dialog>
  )
}
