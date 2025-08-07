import { Location } from '@/api/config'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import SelectLocationMap from '@/features/locations/components/selectLocationMap'
import { Button } from '@mui/material'
import { memo, useCallback, useMemo, useState } from 'react'

interface SelectLocationProps {
  location: Location | null
  setLocation: (location: Location) => void
  chartsDisabled?: boolean
  route?: number[][]
  center?: [number, number]
  mapHeight?: number | string
  addLocationBtn?: boolean
}

export interface Filters {
  areaId?: number | null
  regionId?: number | null
  locationTypeId?: number | null
  jurisdictionId?: number | null
  measureTypeId?: number | null
}

export function SelectLocation({
  location,
  setLocation,
  chartsDisabled,
  route,
  center,
  addLocationBtn,
  mapHeight,
}: SelectLocationProps) {
  const { data } = useLatestVersionOfAllLocations()

  const [filters, setFilters] = useState<Filters>({})

  const allLocations = useMemo(() => data?.value || [], [data])

  const filteredLocations = useMemo(() => {
    return allLocations.filter(
      (loc) =>
        (!filters.areaId || loc.areas?.includes(filters.areaId)) &&
        (!filters.regionId || loc.regionId === filters.regionId) &&
        (!filters.locationTypeId ||
          loc.locationTypeId === filters.locationTypeId) &&
        (!filters.measureTypeId ||
          loc.charts?.includes(filters.measureTypeId)) &&
        (!filters.jurisdictionId ||
          loc.jurisdictionId === filters.jurisdictionId) &&
        (!chartsDisabled || loc.chartEnabled)
    )
  }, [allLocations, filters, chartsDisabled])

  const handleChange = useCallback(
    (_: React.SyntheticEvent, value: Location | null) => {
      if (value) {
        setLocation(value)
      }
    },
    [setLocation]
  )

  const updateFilters = useCallback((newFilters: Partial<Filters>) => {
    setFilters((prevFilters) => ({ ...prevFilters, ...newFilters }))
  }, [])

  return (
    <>
      <LocationInput
        location={location}
        locations={filteredLocations}
        handleChange={handleChange}
        filters={filters}
      />
      {addLocationBtn && <Button sx={{ ml: 0 }}>Add Location</Button>}
      <br />
      <SelectLocationMap
        location={location}
        setLocation={setLocation}
        locations={allLocations}
        filteredLocations={filteredLocations}
        center={center}
        route={route}
        mapHeight={mapHeight}
        filters={filters}
        updateFilters={updateFilters}
      />
    </>
  )
}

export default memo(SelectLocation)
