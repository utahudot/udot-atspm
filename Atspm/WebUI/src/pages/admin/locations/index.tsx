import { getLocationFromKey } from '@/api/config/aTSPMConfigurationApi'
import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import LocationSetupWizard from '@/features/locations/components/LocationSetupWizard'
import EditLocation from '@/features/locations/components/editLocation/EditLocation'
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
    newestLocation.approaches = sortApproachesByPhaseNumber(
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

  const [isWizardOpen, setIsWizardOpen] = useState(false)

  const locationHandler = useLocationConfigHandler({
    location: location as Location,
  })

  if (pageAccess.isLoading) {
    return null
  }

  const handleLocationChange = (newLocation: Location) => {
    setLocation(newLocation)
  }

  const handleOpenWizard = () => {
    setIsWizardOpen(true)
  }

  return (
    <ResponsivePageLayout title="Manage Locations">
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

      {location !== null ? (
        <EditLocation
          handler={locationHandler}
          updateLocationVersion={setLocation}
        />
      ) : null}

      {isModalOpen && (
        <NewLocationModal
          closeModal={() => setModalOpen(false)}
          setLocation={handleLocationChange}
          onCreatedFromTemplate={handleOpenWizard}
        />
      )}

      {location && <LocationSetupWizard />}
    </ResponsivePageLayout>
  )
}

export default LocationsAdmin
