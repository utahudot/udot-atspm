import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  useCreateArea,
  useDeleteArea,
  useEditArea,
  useGetAreas,
} from '@/features/areas/api/areaApi'
import AreaEditorModal from '@/features/areas/components/AreaEditorModal'
import { Area } from '@/features/areas/types'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { Location } from '@/features/locations/types'
import { Backdrop, CircularProgress } from '@mui/material'

const AreasAdmin = () => {
  const pageAccess = useViewPage(PageNames.Areas)

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createArea } = useCreateArea()
  const { mutateAsync: deleteArea } = useDeleteArea()
  const { mutateAsync: updateArea } = useEditArea()

  const { data: locationsData } = useLatestVersionOfAllLocations()
  const { data: areaData, isLoading, refetch: refetchAreas } = useGetAreas()

  const locations = locationsData?.value
  const areas = areaData?.value

  if (pageAccess.isLoading) {
    return null
  }

  const handleCreateArea = async (areaData: Area) => {
    const { name } = areaData
    await createArea({ name })
    refetchAreas()
  }

  const handleDeleteArea = async (id: number) => {
    await deleteArea(id)
    refetchAreas()
  }

  const handleEditArea = async (areaData: Area) => {
    console.log(areaData)
    // const { id, name } = areaData
    // await updateArea({ data: { name }, id })
    // refetchAreas()
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

  return (
    <ResponsivePageLayout title="Manage Areas" noBottomMargin>
      <AdminTable
        pageName="Area"
        headers={headers}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={<AreaEditorModal isOpen={true} onSave={handleEditArea} />}
        createModal={
          <AreaEditorModal isOpen={true} onSave={handleCreateArea} />
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
