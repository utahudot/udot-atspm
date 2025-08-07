import { SearchLocation as Location } from '@/api/config'
import { Filters } from '@/features/locations/components/selectLocation/SelectLocation'
import { Skeleton } from '@mui/material'
import dynamic from 'next/dynamic'
import { memo, useMemo } from 'react'

type SelectLocationMapProps = {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  filteredLocations: Location[]
  route?: number[][]
  center?: [number, number]
  mapHeight?: number | string
  filters: Filters
  updateFilters: (filters: Partial<Filters>) => void
}

function SelectLocationMap({
  location,
  setLocation,
  locations,
  filteredLocations,
  route,
  center,
  mapHeight,
  filters,
  updateFilters,
}: SelectLocationMapProps) {
  const LocationMap = useMemo(
    () =>
      dynamic(() => import('@/components/LocationMap'), {
        loading: () => (
          <Skeleton variant="rectangular" height={mapHeight ?? 400} />
        ),
        ssr: false,
      }),
    [mapHeight]
  )

  const mapProps = useMemo(
    () => ({
      location,
      setLocation,
      locations,
      filteredLocations,
      route,
      center,
      mapHeight,
      filters,
      updateFilters,
    }),
    [
      location,
      setLocation,
      locations,
      filteredLocations,
      route,
      center,
      mapHeight,
      filters,
      updateFilters,
    ]
  )

  return <LocationMap {...mapProps} />
}

export default memo(SelectLocationMap)
