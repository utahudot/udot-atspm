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
}: {
  locations: Location[];
  setSelectedLocations: Dispatch<SetStateAction<Location[]>>;
  changeLocation: (location: Location | null) => void;
}): ExpandLocationHandler => {
  const [updatedLocations, setUpdatedLocations] = useState<
    ExpandLocationForAggregation[]
  >([])

  useEffect(() => {
    setUpdatedLocations((prevLocations) => {
      return locations.map((location) => {
        const existingLocation = prevLocations.find(
          (l) => l.locationIdentifier === location.locationIdentifier
        );

        return {
          locationIdentifier: location.locationIdentifier,
          primaryName: location.primaryName,
          secondaryName: location.secondaryName,
          exclude: existingLocation?.exclude ?? false,
          open: existingLocation?.open ?? false,
          approaches: location.approaches.map((approach) => {
            const existingApproach = existingLocation?.approaches.find(
              (a) => a.approachId === approach.id
            );

            return {
              approachId: approach.id,
              description: approach.description,
              exclude: existingApproach?.exclude ?? false,
              open: existingApproach?.open ?? false,
              detectors: approach.detectors.map((detector) => {
                const existingDetector = existingApproach?.detectors.find(
                  (d) => d.id === detector.id
                );

                return {
                  id: detector.id,
                  dectectorIdentifier: detector.dectectorIdentifier,
                  detChannel: detector.detectorChannel,
                  laneNumber: detector.laneNumber as number,
                  laneType: detector.laneType as LaneType,
                  exclude: existingDetector?.exclude ?? false,
                }
              }),
            };
          }),
        };
      });
    });
  }, [locations])

  const component: ExpandLocationHandler = {
    updatedLocations,
    updateLocationOpen(location) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            return { ...oldLocation, open: !oldLocation.open };
          }
          return oldLocation;
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
            return { ...oldLocation, exclude: !oldLocation.exclude };
          }
          return oldLocation;
        })
      )
    },
    updateApproachOpen(location, approach) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            return {
              ...oldLocation,
              approaches: oldLocation.approaches.map((oldApproach) => {
                if (oldApproach.description === approach.description) {
                  return { ...oldApproach, open: !oldApproach.open };
                }
                return oldApproach;
              }),
            };
          }
          return oldLocation;
        })
      )
    },
    updateApproachExclude(location, approach) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            return {
              ...oldLocation,
              approaches: oldLocation.approaches.map((oldApproach) => {
                if (oldApproach.description === approach.description) {
                  return { ...oldApproach, exclude: !oldApproach.exclude };
                }
                return oldApproach;
              }),
            };
          }
          return oldLocation;
        })
      )
    },
    updateDetectorExclude(location, approach, detector) {
      setUpdatedLocations((prevArr) =>
        prevArr.map((oldLocation) => {
          if (oldLocation.locationIdentifier === location.locationIdentifier) {
            return {
              ...oldLocation,
              approaches: oldLocation.approaches.map((oldApproach) => {
                if (oldApproach.description === approach.description) {
                  return {
                    ...oldApproach,
                    detectors: oldApproach.detectors.map((oldDetector) => {
                      if (oldDetector.id === detector.id) {
                        return { ...oldDetector, exclude: !oldDetector.exclude };
                      }
                      return oldDetector;
                    }),
                  };
                }
                return oldApproach;
              }),
            };
          }
          return oldLocation;
        })
      )
    },
  }

  return component
}
