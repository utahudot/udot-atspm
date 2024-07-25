import { useGetLocation } from '@/features/locations/api'
import { sortApproachesByPhaseNumber } from '@/features/locations/components/editApproach/utils/sortApproaches'
import {
  Approach,
  Detector,
  Location,
  LocationExpanded,
} from '@/features/locations/types'
import { useEffect, useState } from 'react'

interface LocationHandlerProps {
  location: Location
}

export interface ApproachForConfig extends Approach {
  index?: number
  open?: boolean
  isNew?: boolean
}

export interface DetectorForConfig extends Detector {
  isNew: boolean
}

export interface LocationConfigHandler {
  approaches: ApproachForConfig[]
  expandedLocation: LocationExpanded | null
  updateApproaches: (approaches: ApproachForConfig[]) => void
  updateExpandedLocation: (location: LocationExpanded) => void
  handleAddNewApproach: () => void
  handleLocationEdit: (name: string, value: string) => void
  refetchLocation: () => void
}

export const useLocationConfigHandler = ({
  location,
}: LocationHandlerProps): LocationConfigHandler => {
  const [expandedLocation, setExpandedLocation] =
    useState<LocationExpanded | null>(null)
  const [activeLocationId, setActiveLocationId] = useState(location?.id)
  const [approaches, setApproaches] = useState<ApproachForConfig[]>([])

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
      ...(existingApproaches as ApproachForConfig[]),
    ])
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expandedLocation])

  useEffect(() => {
    if (!expandedData) return
    setExpandedLocation(expandedData.value[0])
  }, [expandedData])

  const addNewApproach = () => {
    const newApproach: Partial<ApproachForConfig> = {
      id: crypto.randomUUID(),
      description: 'New Approach',
      isNew: true,
      detectors: [],
      isProtectedPhaseOverlap: false,
      isPermissivePhaseOverlap: false,
      isPedestrianPhaseOverlap: false,
      protectedPhaseNumber: null,
      permissivePhaseNumber: null,
      pedestrianPhaseNumber: null,
      pedestrianDetectors: '',
      locationId: parseInt((expandedLocation as LocationExpanded).id),
      directionType: {
        id: '0',
        abbreviation: 'NA',
        description: 'Unknown',
        displayOrder: 0,
      },
    }

    setApproaches((prev) => [newApproach as ApproachForConfig, ...prev])
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
        ...(expandedLocation as LocationExpanded),
        [name as string]: value,
      })
    },
    refetchLocation() {
      refetchLocation()
    },
  }

  return component
}
