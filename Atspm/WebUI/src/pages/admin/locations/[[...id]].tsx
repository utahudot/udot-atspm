import { getLocationFromKey, Location } from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import { AddButton } from '@/components/addButton'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { sortApproachesAndDetectors } from '@/features/locations/components/editApproach/utils/sortApproaches'
import LocationEditor from '@/features/locations/components/editLocation/EditLocation'
import NewLocationModal from '@/features/locations/components/editLocation/NewLocationModal'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import SelectLocation from '@/features/locations/components/selectLocation/SelectLocation'
import { useRouter } from 'next/router'
import { useCallback, useEffect, useRef, useState } from 'react'

export async function getLocation(locationId: number) {
  const res = await getLocationFromKey(locationId, {
    expand:
      'areas, devices, approaches($expand=Detectors($expand=DetectionTypes, detectorComments))',
  })
  if (res?.value?.length) {
    const newest = res.value[0]
    newest.approaches = sortApproachesAndDetectors(newest.approaches)
    return newest
  }
  return null
}

const LocationsAdmin = () => {
  const { push, query, isReady } = useRouter()
  const raw = query.id
  const locationId = Array.isArray(raw) ? Number(raw[0]) : Number(raw)

  const location = useLocationStore((s) => s.location)
  const setLocation = useLocationStore((s) => s.setLocation)

  const pageAccess = useViewPage(PageNames.Location)
  const [isModalOpen, setModalOpen] = useState(false)
  const [isWizardOpen, setIsWizardOpen] = useState(false)

  const onSelectLocation = useCallback(
    (sel: Location | number | null) => {
      const id =
        sel && typeof sel === 'object'
          ? sel.id
          : typeof sel === 'number'
            ? sel
            : null
      if (id) push(`/admin/locations/${id}`, undefined, { shallow: true })
      else push('/admin/locations', undefined, { shallow: true })
    },
    [push]
  )

  useEffect(() => {
    if (!isReady) return
    if (!Number.isFinite(locationId)) {
      setLocation(null)
      return
    }
    ;(async () => {
      setLocation(await getLocation(locationId))
    })()
  }, [isReady, locationId, setLocation])

  const lastFetchedId = useRef<number | null>(null)
  useEffect(() => {
    if (location?.id && location.id !== lastFetchedId.current) {
      lastFetchedId.current = location.id
      onSelectLocation(location.id)
      ;(async () => setLocation(await getLocation(location.id)))()
    }
  }, [location?.id, setLocation, onSelectLocation])

  const handleOpenWizard = () => setIsWizardOpen(true)
  const openNewLocationModal = useCallback(() => setModalOpen(true), [])
  const closeModal = useCallback(() => setModalOpen(false), [])

  if (pageAccess.isLoading) return null

  return (
    <ResponsivePageLayout title="Manage Locations" useFullWidth>
      <AddButton
        label="New Location"
        onClick={openNewLocationModal}
        sx={{ mb: 1, width: 200 }}
      />
      <StyledPaper sx={{ width: '50%', minWidth: 400, p: 3 }}>
        <SelectLocation
          location={location}
          setLocation={onSelectLocation}
          mapHeight={400}
        />
      </StyledPaper>
      {location && <LocationEditor />}
      {isModalOpen && (
        <NewLocationModal
          closeModal={closeModal}
          setLocation={onSelectLocation}
          onCreatedFromTemplate={handleOpenWizard}
        />
      )}
    </ResponsivePageLayout>
  )
}

export default LocationsAdmin
