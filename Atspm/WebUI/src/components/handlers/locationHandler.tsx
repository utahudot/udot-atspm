import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
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
