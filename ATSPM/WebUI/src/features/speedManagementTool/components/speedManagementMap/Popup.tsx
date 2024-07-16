import { Box, Typography } from '@mui/material'
import { Popup } from 'react-leaflet'

const SpeedManagementPopup = ({ route }) => {
  return (
    <Popup offset={[0, -30]}>
      <Box sx={{ fontWeight: 'bold' }}>
        <Typography variant="h6">{route.properties.route_name}</Typography>
        <br />
        Data Source: {route.properties.dataSource}
        <br />
        Speed Limit: {route.properties.speedLimit}
        <br />
        Average Speed: {route.properties.avg}
      </Box>
    </Popup>
  )
}

export default SpeedManagementPopup
