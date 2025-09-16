import { Approach, Detector, Location } from '@/api/config'
import { useGetLocation } from '@/features/locations/api'
import { sortApproachesByPhaseNumber } from '@/features/locations/components/editApproach/utils/sortApproaches'
import { useEffect, useState } from 'react'

interface LocationHandlerProps {
  location: Location | null
}

export interface ConfigApproach
  extends Omit<Approach, 'id' | 'detectors' | 'protectedPhaseNumber'> {
  id?: number
  index?: number
  open?: boolean
  isNew?: boolean
  detectors: ConfigDetector[]
  protectedPhaseNumber: number | null
}

export interface ConfigDetector extends Omit<Detector, 'id' | 'approachId'> {
  id?: number
  approachId?: number
  isNew?: boolean
}

export interface LocationConfigHandler {
  approaches: ConfigApproach[]
  expandedLocation: Location | null
  updateApproaches: (approaches: ConfigApproach[]) => void
  updateExpandedLocation: (location: Location) => void
  handleAddNewApproach: () => void
  handleLocationEdit: (name: string, value: string) => void
  refetchLocation: () => void
}

export const useLocationConfigHandler = ({
  location,
}: LocationHandlerProps): LocationConfigHandler => {
  const [expandedLocation, setExpandedLocation] = useState<Location | null>(
    null
  )
  const [activeLocationId, setActiveLocationId] = useState(location?.id)
  const [approaches, setApproaches] = useState<ConfigApproach[]>([])

  const { data: expandedData, refetch: refetchLocation } =
    useGetLocation(activeLocationId)

  useEffect(() => {
    if (activeLocationId) refetchLocation()
  }, [activeLocationId, refetchLocation])

  useEffect(() => {
    if (!location) {
      setActiveLocationId('')
    } else if (location?.id !== activeLocationId) {
      setActiveLocationId(location.id)
    }
  }, [location, activeLocationId, refetchLocation])

  useEffect(() => {
    if (!expandedLocation) return
    const existingApproaches = sortApproachesByPhaseNumber(
      expandedLocation.approaches.map((approach, index) => ({
        ...approach,
        index,
        open: false,
        isNew: false,
      }))
    )
    const newApproaches = approaches.filter((item) => item.isNew)
    setApproaches([
      ...newApproaches,
      ...(existingApproaches as ConfigApproach[]),
    ])
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expandedLocation])

  useEffect(() => {
    if (!expandedData) return
    setExpandedLocation(expandedData.value[0])
  }, [expandedData])

  const addNewApproach = () => {
    const id = Math.round(Math.random() * 10000)
    const index = approaches.length
    const newApproach: Partial<ConfigApproach> = {
      id: id,
      index,
      description: 'New Approach',
      isNew: true,
      detectors: [],
      isProtectedPhaseOverlap: false,
      isPermissivePhaseOverlap: false,
      isPedestrianPhaseOverlap: false,
      permissivePhaseNumber: null,
      pedestrianPhaseNumber: null,
      pedestrianDetectors: '',
      locationId: expandedLocation?.id,
      directionType: {
        id: '0',
        abbreviation: 'NA',
        description: 'Unknown',
        displayOrder: 0,
      },
    }

    setApproaches((prev) => [newApproach as ConfigApproach, ...prev])
  }

  const component: LocationConfigHandler = {
    approaches,
    expandedLocation,
    updateApproaches(approaches) {
      setApproaches(approaches)
      refetchLocation()
    },
    updateExpandedLocation(location) {
      setExpandedLocation(location)
    },
    handleAddNewApproach() {
      addNewApproach()
    },
    handleLocationEdit(name, value) {
      setExpandedLocation({
        ...expandedLocation,
        [name as string]: value,
      })
    },
    refetchLocation() {
      refetchLocation()
    },
  }

  return component
}
