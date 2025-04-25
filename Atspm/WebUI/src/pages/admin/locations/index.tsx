import { getLocationFromKey } from '@/api/config/aTSPMConfigurationApi'
import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { sortApproachesAndDetectors } from '@/features/locations/components/editApproach/utils/sortApproaches'
import LocationEditor from '@/features/locations/components/editLocation/EditLocation'
import NewLocationModal from '@/features/locations/components/editLocation/NewLocationModal'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { useCallback, useState } from 'react'

export async function getLocation(locationId: number) {
  const locationResponse = await getLocationFromKey(locationId, {
    expand:
      'areas, devices, approaches($expand=Detectors($expand=DetectionTypes, detectorComments))',
  })
  if (locationResponse?.value?.length) {
    const newestLocation = locationResponse.value[0]
    newestLocation.approaches = sortApproachesAndDetectors(
      newestLocation.approaches
    )
    return newestLocation
  }
  return null
}

const LocationsAdmin = () => {
  const location = useLocationStore((s) => s.location)
  const setLocation = useLocationStore((s) => s.setLocation)

  const pageAccess = useViewPage(PageNames.Location)
  const [isModalOpen, setModalOpen] = useState(false)

  const handleSetLocation = useCallback(
    async (selectedLocation: Location | null) => {
      if (selectedLocation) {
        setLocation(await getLocation(selectedLocation.id))
      } else {
        setLocation(null)
      }
    },
    [setLocation]
  )

  const openNewLocationModal = useCallback(() => setModalOpen(true), [])
  const closeModal = useCallback(() => setModalOpen(false), [])

  if (pageAccess.isLoading) return null

  return (
    <ResponsivePageLayout title="Manage Locations" useFullWidth>
      <AddButton
        label="New Location"
        onClick={openNewLocationModal}
        sx={{ mb: 1, width: '200px' }}
      />
      <StyledPaper sx={{ width: '50%', minWidth: '400px', p: 3 }}>
        <SelectLocation
          location={location}
          setLocation={handleSetLocation}
          mapHeight={400}
        />
      </StyledPaper>
      {location && <LocationEditor />}
      {isModalOpen && (
        <NewLocationModal
          closeModal={closeModal}
          setLocation={handleSetLocation}
        />
      )}
    </ResponsivePageLayout>
  )
}

export default LocationsAdmin
