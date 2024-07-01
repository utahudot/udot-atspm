import { Location } from '@/features/locations/types'
import { useState } from 'react'

export interface LocationHandler {
  location: Location | null
  changeLocation(location: Location | null): void
}

export const useLocationHandler = () => {
  const [location, setLocation] = useState<Location | null>(null)

  const component: LocationHandler = {
    location,
    changeLocation(location) {
      setLocation(location)
    },
  }

  return component
}
