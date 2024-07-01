import {
  LaneType,
  Location,
  LocationExpanded,
} from '@/features/locations/types'
import { Dispatch, SetStateAction, useEffect, useState } from 'react'

export interface ExpandLocationHandler {
  updatedLocations: ExpandLocationForAggregation[]
  updateLocationOpen(location: ExpandLocationForAggregation): void
  deleteLocation(location: ExpandLocationForAggregation): void
  updateLocationExclude(location: ExpandLocationForAggregation): void
  updateApproachOpen(
    location: ExpandLocationForAggregation,
    approach: ExpandApproachForAggregation
  ): void
  updateApproachExclude(
    location: ExpandLocationForAggregation,
    approach: ExpandApproachForAggregation
  ): void
  updateDetectorExclude(
    location: ExpandLocationForAggregation,
    approach: ExpandApproachForAggregation,
    detector: ExpandDetectorForAggregation
  ): void
}

export interface ExpandDetectorForAggregation {
  id: number
  dectectorIdentifier: string
  detChannel: number
  laneNumber: number
  laneType: LaneType
  exclude: boolean
}

export interface ExpandApproachForAggregation {
  description: string
  approachId: number
  exclude: boolean
  open: boolean
  detectors: ExpandDetectorForAggregation[]
}

export interface ExpandLocationForAggregation {
  locationIdentifier: string
  primaryName: string
  secondaryName: string
  exclude: boolean
  open: boolean
  approaches: ExpandApproachForAggregation[]
}

interface props {
  locations: LocationExpanded[]
  setSelectedLocations: Dispatch<SetStateAction<LocationExpanded[]>>
  changeLocation(location: Location | null): void
}

export const useExpandLocationHandler = ({
  locations,
  setSelectedLocations,
  changeLocation,
}: props): ExpandLocationHandler => {
  const [updatedLocations, setUpdatedLocations] = useState<
    ExpandLocationForAggregation[]
  >([])

  useEffect(() => {
    const tranformedExpandedLocation = locations.map((location) => {
      return {
        locationIdentifier: location.locationIdentifier,
        primaryName: location.primaryName,
        secondaryName: location.secondaryName,
        exclude: false,
        open: false,
        approaches: location.approaches.map((approach) => {
          return {
            approachId: approach.approachId,
            description: approach.description,
            exclude: false,
            open: false,
            detectors: approach.detectors.map((detector) => {
              return {
                id: detector.id,
                dectectorIdentifier: detector.dectectorIdentifier,
                detChannel: detector.detChannel,
                laneNumber: detector.laneNumber,
                laneType: detector.laneType,
                exclude: false,
              }
            }),
          }
        }),
      }
    })

    setUpdatedLocations(tranformedExpandedLocation)
  }, [locations])

  const component: ExpandLocationHandler = {
    updatedLocations,
    updateLocationOpen(location) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            oldLocation.open = !oldLocation.open
          }
          return oldLocation
        })
      )
    },
    deleteLocation(location) {
      setSelectedLocations((prevArr) =>
        prevArr.filter(
          (oldLocation) =>
            oldLocation.locationIdentifier !== location.locationIdentifier
        )
      )
      changeLocation(null)
    },
    updateLocationExclude(location) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            oldLocation.exclude = !oldLocation.exclude
          }
          return oldLocation
        })
      )
    },
    updateApproachOpen(location, approach) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            oldLocation.approaches.forEach((oldApproach) => {
              if (oldApproach.description === approach.description) {
                oldApproach.open = !oldApproach.open
              }
            })
          }
          return oldLocation
        })
      )
    },
    updateApproachExclude(location, approach) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            oldLocation.approaches.forEach((oldApproach) => {
              if (oldApproach.description === approach.description) {
                oldApproach.exclude = !oldApproach.exclude
              }
            })
          }
          return oldLocation
        })
      )
    },
    updateDetectorExclude(location, approach, detector) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            oldLocation.approaches.forEach((oldApproach) => {
              if (oldApproach.description === approach.description) {
                oldApproach.detectors.forEach((oldDetector) => {
                  if (oldDetector.id === detector.id) {
                    oldDetector.exclude = !oldDetector.exclude
                  }
                })
              }
            })
          }
          return oldLocation
        })
      )
    },
  }

  return component
}
