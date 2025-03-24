import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import LocationSetupWizard from '@/features/locations/components/LocationSetupWizard'
import EditLocation from '@/features/locations/components/editLocation/EditLocation'
import NewLocationModal from '@/features/locations/components/editLocation/NewLocationModal'
import { useLocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { Location } from '@/features/locations/types/Location'
import { useState } from 'react'

const LocationsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Location)

  const [location, setLocation] = useState<Location | null>(null)
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
    <ResponsivePageLayout title={'Manage Locations'}>
      <AddButton
        label="New Location"
        onClick={() => setModalOpen(true)}
        sx={{ mb: 1, width: '200px' }}
      />

      <StyledPaper sx={{ width: '50%', minWidth: '400px', p: 3 }}>
        <SelectLocation
          location={location}
          setLocation={setLocation}
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
