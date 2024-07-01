import LocationDto from '@/features/locations/types/LocationDto'
import VersionDto from '@/features/locations/types/VersionDto'

export const convertConfigVersionToDto = (
  data: any
): { locationInfo: LocationDto[]; versionInfo: VersionDto[] } => {
  const locationInfo: LocationDto[] = createLocationInfo(data)
  const versionInfo: VersionDto[] = createVersionInfo(data)

  return {
    locationInfo,
    versionInfo,
  }
}

const createLocationInfo = (data: any): LocationDto[] => {
  return data.map((location: any) => {
    return {
      controllerType: location?.controllerType?.description,
      description: location?.region?.description,
      displayOnMap: location?.chartEnabled,
      latitude: location?.latitude,
      longitude: location?.longitude,
      pedsare1to1: location?.pedsare1to1,
      primaryName: location?.primaryName,
      secondaryName: location?.secondaryName,
      id: location?.id,
    }
  })
}
function createVersionInfo(data: any): VersionDto[] {
  return data.map((version: any) => {
    return {
      id: version.id,
      name: version.note,
      date: new Date(version.start),
    }
  })
}
