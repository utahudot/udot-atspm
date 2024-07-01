import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import SelectLocationMap from '@/features/locations/components/selectLocationMap'
import { Location } from '@/features/locations/types'
import { Button } from '@mui/material'
import { memo } from 'react'

interface SelectLocationProps {
  location: Location | null
  setLocation: (location: Location) => void
  route?: number[][]
  center?: [number, number]
  zoom?: number
  mapHeight?: number | string
  addLocationBtn?: boolean
}

export function SelectLocation({
  location,
  setLocation,
  route,
  center,
  zoom,
  addLocationBtn,
  mapHeight,
}: SelectLocationProps) {
  const { data } = useLatestVersionOfAllLocations()

  const locations = data?.value || []

  const handleChange = (_: React.SyntheticEvent, value: Location | null) => {
    if (value) {
      setLocation(value)
    }
  }

  return (
    <>
      <LocationInput
        location={location}
        locations={locations}
        handleChange={handleChange}
      />
      {addLocationBtn && <Button sx={{ ml: 0 }}>Add Location</Button>}
      <br />
      <SelectLocationMap
        location={location}
        setLocation={setLocation}
        locations={locations}
        center={center}
        zoom={zoom}
        route={route}
        mapHeight={mapHeight}
      />
    </>
  )
}

export default memo(SelectLocation)
