import { useGetLocation } from '@/features/locations/api'
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
  index: number
  open: boolean
  isNew: boolean
}

export interface DetectorForConfig extends Detector {
  isNew: boolean
}

export interface LocationConfigHandler {
  approaches: ApproachForConfig[]
  expandedLocation: LocationExpanded | null
  updateApproaches: (approaches: ApproachForConfig[]) => void
  updateApproach: (approach: ApproachForConfig) => void
  updateExpandedLocation: (location: LocationExpanded) => void
  handleAddNewApproach: () => void
  handleLocationEdit: (name: string, value: string) => void
  refetchLocation: () => void
  isApproachesEditable: boolean
  updateIsApproachesEditable: (value: boolean) => void
}

export const useLocationConfigHandler = ({
  location,
}: LocationHandlerProps): LocationConfigHandler => {
  const [expandedLocation, setExpandedLocation] =
    useState<LocationExpanded | null>(null)
  const [activeLocationId, setActiveLocationId] = useState<string>(location?.id)
  const [approaches, setApproaches] = useState<ApproachForConfig[]>([])
  const [isApproachesEditable, setIsApproachesEditable] =
    useState<boolean>(false)

  const { data: expandedData, refetch } = useGetLocation(activeLocationId)

  useEffect(() => {
    if (activeLocationId) refetch()
  }, [activeLocationId, refetch])

  useEffect(() => {
    if (!location) {
      setActiveLocationId('')
    } else if (location?.id !== activeLocationId) {
      setActiveLocationId(location.id)
    }
  }, [location, activeLocationId, refetch])

  useEffect(() => {
    if (!expandedLocation) return
    const existingApproaches = expandedLocation.approaches.map(
      (approach, index) => ({
        ...approach,
        index,
        open: false,
        isNew: false,
      })
    )
    const newApproaches = approaches.filter((item) => item.isNew)
    setApproaches([
      ...(existingApproaches as ApproachForConfig[]),
      ...newApproaches,
    ])
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expandedLocation])

  useEffect(() => {
    if (!expandedData) return
    setExpandedLocation(expandedData.value[0])
  }, [expandedData])

  const addNewApproach = () => {
    const newApproach: Partial<ApproachForConfig> = {
      description: 'New Approach',
      index: approaches.length,
      open: false,
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

    setApproaches((prev) => [...prev, newApproach as ApproachForConfig])
  }

  const component: LocationConfigHandler = {
    approaches,
    expandedLocation,
    isApproachesEditable,
    updateIsApproachesEditable(value) {
      setIsApproachesEditable(value)
    },
    updateApproach(approach) {
      setApproaches((prev) =>
        prev.map((val) => (val.index === approach.index ? approach : val))
      )
      refetch()
    },
    updateApproaches(approaches) {
      setApproaches(approaches)
      refetch()
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
      refetch()
    },
  }

  return component
}
