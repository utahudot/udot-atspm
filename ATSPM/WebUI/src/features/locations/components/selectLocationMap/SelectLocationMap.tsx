import { Location } from '@/features/locations/types'
import { Skeleton } from '@mui/material'
import dynamic from 'next/dynamic'
import { memo, useMemo } from 'react'

type SelectLocationMapProps = {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  route?: number[][]
  zoom?: number
  center?: [number, number]
  mapHeight?: number | string
}

function SelectLocationMap({
  location,
  setLocation,
  locations,
  route,
  zoom,
  center,
  mapHeight,
}: SelectLocationMapProps) {
  const Map = useMemo(
    () =>
      dynamic(() => import('@/components/map/Map'), {
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
      route,
      zoom,
      center,
      mapHeight,
    }),
    [location, setLocation, locations, route, zoom, center, mapHeight]
  )

  return <Map {...mapProps} />
}

export default memo(SelectLocationMap)
