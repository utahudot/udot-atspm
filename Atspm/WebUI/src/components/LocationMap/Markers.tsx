import { SearchLocation as Location } from '@/api/config'
import { generatePin } from '@/features/locations/utils'
import { Box } from '@mui/material'
import { memo, useEffect, useState } from 'react'
import { Marker, Popup } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'

type MarkersProps = {
  locations: Location[] | undefined
  setLocation: (location: Location) => void
}

const Markers = ({ locations, setLocation }: MarkersProps) => {
  const [icons, setIcons] = useState<Record<string, L.DivIcon>>({})

  useEffect(() => {
    if (locations) {
      const fetchIcons = async () => {
        const iconPromises = locations.map(async (location) => ({
          id: location.id,
          icon: await generatePin(location.locationTypeId),
        }))
        const iconResults = await Promise.all(iconPromises)
        const iconMap = iconResults.reduce<Record<string, L.DivIcon>>(
          (acc, { id, icon }) => {
            acc[id] = icon
            return acc
          },
          {}
        )
        setIcons(iconMap)
      }

      fetchIcons()
    }
  }, [locations])

  if (!locations) return null

  const handleSelectLocation = (location: Location) => {
    setLocation(location)
  }

  return (
    <MarkerClusterGroup chunkedLoading disableClusteringAtZoom={16}>
      {locations.map((marker) => {
        const icon = icons[marker.id]

        if (!icon) return null

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

export default memo(Markers)
