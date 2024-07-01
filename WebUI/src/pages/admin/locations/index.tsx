import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import EditLocation from '@/features/locations/components/editLocation/EditLocation'
import NewLocationModal from '@/features/locations/components/editLocation/NewLocationModal'
import { useLocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { Location } from '@/features/locations/types/Location'
import { Dialog, DialogContent } from '@mui/material'
import { useState } from 'react'

const LocationsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Location)

  const [location, setLocation] = useState<Location | null>(null)
  const [isModalOpen, setModalOpen] = useState(false)
  const locationHandler = useLocationConfigHandler({
    location: location as Location,
  })

  if (pageAccess.isLoading) {
    return
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

      <Dialog
        open={isModalOpen}
        onClose={() => setModalOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <h2 style={{ marginTop: '1.5rem', marginLeft: '1.5rem' }}>
          Add Location
        </h2>
        <DialogContent>
          <NewLocationModal
            closeModal={() => setModalOpen(false)}
            setLocation={setLocation}
          />
        </DialogContent>
      </Dialog>
    </ResponsivePageLayout>
  )
}

export default LocationsAdmin
