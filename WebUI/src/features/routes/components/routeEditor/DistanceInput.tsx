import { RouteLocation } from '@/features/routes/types'
import { useNotificationStore } from '@/stores/notifications'
import { fetchRouteDistance } from '@/utils/fetchRouteDistance'
import RefreshIcon from '@mui/icons-material/Refresh'
import {
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
  handleDistanceChange: (link: RouteLocation, distance: number) => void
}

const DistanceInput = ({
  hasErrors,
  link,
  nextLink,
  handleDistanceChange,
}: DistanceDisplayInputProps) => {
  const { addNotification } = useNotificationStore()
  const [rotate, setRotate] = useState(false)

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const distance = event.target.value
    handleDistanceChange(link, distance)
  }

  const handleRecalculateClick = () => {
    setRotate((prevState) => !prevState)
    if (nextLink) {
      fetchRouteDistance([link, nextLink]).then((response) => {
        if (response?.distance) {
          const distance = Math.round(response.distance)
          handleDistanceChange(link, distance)
        } else {
          addNotification({
            type: 'error',
            title: 'Error Calculating Distance',
          })
        }
      })
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
          />
          <Tooltip title="Recalculate Distance">
            <IconButton
              size="small"
              color="primary"
              aria-label="recalculate distance"
              onClick={handleRecalculateClick}
            >
              <RefreshIcon
                sx={{
                  transform: rotate ? 'rotate(360deg)' : 'rotate(0deg)',
                  transition: 'transform 0.3s',
                }}
              />
            </IconButton>
          </Tooltip>
        </>
      ) : (
        <Typography variant="caption">End of Route</Typography>
      )}
    </Box>
  )
}

export default DistanceInput
