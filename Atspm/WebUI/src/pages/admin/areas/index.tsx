import {
  Area,
  Location,
  useDeleteAreaFromKey,
  useGetArea,
  useGetLocationLocationsForSearch,
  usePatchAreaFromKey,
  usePostArea,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import AreaEditorModal from '@/features/areas/components/AreaEditorModal'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'
const AreasAdmin = () => {
  const pageAccess = useViewPage(PageNames.Areas)
  const { addNotification } = useNotificationStore()
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createArea } = usePostArea()
  const { mutateAsync: deleteArea } = useDeleteAreaFromKey()
  const { mutateAsync: updateArea } = usePatchAreaFromKey()

  const { data: locationsData } = useGetLocationLocationsForSearch()
  const { data: areaData, isLoading, refetch: refetchAreas } = useGetArea()

  const locations = locationsData?.value
  const areas = areaData?.value

  if (pageAccess.isLoading) {
    return null
  }

  const handleCreateArea = async (areaData: Area) => {
    const { name } = areaData
    try {
      await createArea({ data: { name } })
      refetchAreas()
      addNotification({ title: 'Area Created', type: 'success' })
    } catch (error) {
      addNotification({
        type: 'error',
        title: 'Error Creating Area',
      })
    }
  }

  const handleEditArea = async (areaData: Area) => {
    const { id, name } = areaData
    try {
      await updateArea({ data: { name }, key: id })
      refetchAreas()
      addNotification({ type: 'success', title: 'Area Updated' })
    } catch (error) {
      addNotification({ type: 'error', title: 'Error Updating Area' })
    }
  }

  const handleDeleteArea = async (id: number) => {
    try {
      await deleteArea({ key: id })
      refetchAreas()
      addNotification({ type: 'success', title: 'Area Deleted' })
    } catch (error) {
      addNotification({ type: 'error', title: 'Error Deleting Area' })
    }
  }

  const onModalClose = () => {
    //add code for custom modal close
  }

  const filterAssociatedObjects = (areaId: number, objects: Location[]) => {
    const associatedLocations = objects.filter((object) => {
      return object.areas?.some((id) => id === areaId)
    })

    return associatedLocations.map((location) => ({
      id: location.id,
      name: `${location.primaryName} & ${location.secondaryName}`,
    }))
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!areas) {
    return <div>Error returning data</div>
  }

  const filteredData = areas.map((area) => ({
    id: area.id,
    name: area.name,
  }))

  const headers = ['Name']
  const headerKeys = ['name']

  return (
    <ResponsivePageLayout title="Manage Areas" noBottomMargin>
      <AdminTable
        pageName="Area"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={
          <AreaEditorModal
            isOpen={true}
            onSave={handleEditArea}
            onClose={onModalClose}
          />
        }
        createModal={
          <AreaEditorModal
            isOpen={true}
            onSave={handleCreateArea}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Area"
            open={false}
            onClose={() => {}}
            onConfirm={handleDeleteArea}
            associatedObjects={locations}
            associatedObjectsLabel="locations"
            filterFunction={filterAssociatedObjects}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default AreasAdmin
