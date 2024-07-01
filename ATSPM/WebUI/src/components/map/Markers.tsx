import { Location } from '@/features/locations/types'
import { Box } from '@mui/material'
import { Icon } from 'leaflet'
import { Marker, Popup } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'

const icon = new Icon({
  iconUrl: '/images/intersection-pin.svg',
  iconSize: [25, 40],
  iconAnchor: [13, 40],
  shadowSize: [40, 40],
  shadowAnchor: [40, 62],
})

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
      {locations.map((marker) => (
        <Marker
          key={marker.id}
          position={[marker.latitude, marker.longitude]}
          icon={icon}
          eventHandlers={{
            click: () => handleSelectLocation(marker),
          }}
        >
          <Popup offset={[0, -30]}>
            <Box sx={{ fontWeight: 'bold' }}>
              Location #{marker.locationIdentifier}
            </Box>
            <Box>
              {marker.primaryName} & {marker.secondaryName}
            </Box>
            {/* <Box display="flex" justifyContent="flex-end" marginTop={2}>
              <Button
                variant="contained"
                size="small"
                onClick={() => handleSelectLocation(marker)}
              >
                Select
              </Button>
            </Box> */}
          </Popup>
        </Marker>
      ))}
    </MarkerClusterGroup>
  )
}

export default Markers
