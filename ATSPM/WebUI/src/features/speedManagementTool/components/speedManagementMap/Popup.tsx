import { Box, Typography } from '@mui/material'
import { Popup } from 'react-leaflet'

const SpeedManagementPopup = ({ route }) => {
  return (
    <Popup offset={[0, -30]}>
      <Box sx={{ fontWeight: 'bold' }}>
        <Typography variant="h6">{route.properties.name}</Typography>
        <br />
        Speed Limit: {route.properties.speedLimit}
        <br />
        Average Speed: {route.properties.avg}
        {route.properties.percentilespd_85 && (
          <>
            <br />
            85th Percentile: {route.properties.percentilespd_85}
          </>
        )}
      </Box>
    </Popup>
  )
}

export default SpeedManagementPopup
