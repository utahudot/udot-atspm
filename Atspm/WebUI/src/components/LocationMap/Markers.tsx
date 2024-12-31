// Markers.tsx

import { Location } from '@/features/locations/types'
import { generatePin } from '@/features/locations/utils'
import { Box } from '@mui/material'
import { Marker, Popup } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'

type MarkersProps = {
  locations: Location[] | undefined
  setLocation: (location: Location) => void
}

const Markers = ({ locations, setLocation }: MarkersProps) => {
  if (!locations) return null

  const handleSelectLocation = (location: Location) => {
    setLocation(location)
  }

  return (
    <MarkerClusterGroup chunkedLoading>
      {locations.map((marker) => {
        const icon = generatePin(marker.locationTypeId)

        return (
          <Marker
            key={marker.id}
            position={[marker.latitude, marker.longitude]}
            icon={icon}
            eventHandlers={{ click: () => handleSelectLocation(marker) }}
          >
            <Popup offset={[0, -30]}>
              <Box sx={{ fontWeight: 'bold' }}>
                Location #{marker.locationIdentifier}
              </Box>
              <Box>
                {marker.primaryName} & {marker.secondaryName}
              </Box>
            </Popup>
          </Marker>
        )
      })}
    </MarkerClusterGroup>
  )
}

export default Markers
