import { RouteLocation } from '@/features/routes/types'
import { useNotificationStore } from '@/stores/notifications'
import { fetchRouteDistance } from '@/utils/fetchRouteDistance'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
  CircularProgress,
  InputAdornment,
  OutlinedInput,
  Tooltip,
  Typography,
} from '@mui/material'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import { useState } from 'react'

interface DistanceDisplayInputProps {
  hasErrors: boolean
  link: RouteLocation
  nextLink: RouteLocation
  handleDistanceChange: (locationIdentifier: string, distance: number) => void
}

const DistanceInput = ({
  hasErrors,
  link,
  nextLink,
  handleDistanceChange,
}: DistanceDisplayInputProps) => {
  const { addNotification } = useNotificationStore()
  const [rotate, setRotate] = useState(false)
  const [loading, setLoading] = useState(false)

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const numeric = Number(event.target.value)
    handleDistanceChange(link.locationIdentifier, numeric)
  }

  const handleRecalculateClick = async () => {
    if (!nextLink || loading) return

    setRotate((prev) => !prev)
    setLoading(true)

    try {
      const response = await fetchRouteDistance([link, nextLink])
      if (response) {
        const distance = Math.round(response.distance)
        handleDistanceChange(link.locationIdentifier, distance)
      } else {
        addNotification({
          type: 'error',
          title: 'Error calculating distance',
        })
      }
    } catch {
      addNotification({
        type: 'error',
        title: 'Error calculating distance',
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box display="flex" alignItems="center" width="130px">
      {nextLink ? (
        <>
          <OutlinedInput
            error={hasErrors && link.nextLocationDistance === null}
            size="small"
            placeholder="?"
            value={link.nextLocationDistance?.distance || ''}
            onChange={handleInputChange}
            inputProps={{ 'aria-label': 'distance' }}
            endAdornment={<InputAdornment position="end">ft</InputAdornment>}
            disabled={loading}
          />
          <Tooltip title={loading ? 'Calculating…' : 'Recalculate Distance'}>
            <span>
              <IconButton
                size="small"
                color="primary"
                aria-label="recalculate distance"
                onClick={handleRecalculateClick}
                disabled={loading}
              >
                {loading ? (
                  <CircularProgress size={20} />
                ) : (
                  <RefreshIcon
                    sx={{
                      transform: rotate ? 'rotate(360deg)' : 'rotate(0deg)',
                      transition: 'transform 0.3s',
                    }}
                  />
                )}
              </IconButton>
            </span>
          </Tooltip>
        </>
      ) : (
        <Typography variant="caption">End of Route</Typography>
      )}
    </Box>
  )
}

export default DistanceInput
