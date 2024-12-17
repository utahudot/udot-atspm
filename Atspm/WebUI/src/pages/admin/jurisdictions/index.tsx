import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import {
  useCreateJurisdiction,
  useDeleteJurisdiction,
  useEditJurisdiction,
  useGetJurisdiction,
} from '@/features/jurisdictions/api/jurisdictionApi'
import JurisdictionEditorModal from '@/features/jurisdictions/components/JurisdictionEditorModal'
import { Jurisdiction } from '@/features/jurisdictions/types'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { Backdrop, CircularProgress } from '@mui/material'

const JurisdictionsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Jurisdiction)
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )
  const { mutateAsync: createMutation } = useCreateJurisdiction()
  const { mutateAsync: deleteMutation } = useDeleteJurisdiction()
  const { mutateAsync: editMutation } = useEditJurisdiction()

  const { data: locationsData } = useLatestVersionOfAllLocations()
  const locations = locationsData?.value

  const {
    data: jurisdictionData,
    isLoading,
    refetch: refetchJurisdictions,
  } = useGetJurisdiction()


  const jurisdictions = jurisdictionData?.value

  if (pageAccess.isLoading) {
    return null
  }

  const HandleCreateJurisdiction = async (jurisdictionData: Jurisdiction) => {
    const { id, otherPartners, countyParish, name, mpo } = jurisdictionData
    try {
      await createMutation({
        id,
        otherPartners,
        countyParish,
        name,
        mpo,
      })
      refetchJurisdictions()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteJurisdiction = async (id: number) => {
    try {
      await deleteMutation(id)
      refetchJurisdictions()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditJurisdiction = async (jurisdictionData: Jurisdiction) => {
    const { id, otherPartners, countyParish, name, mpo } = jurisdictionData
    try {
      await editMutation({
        data: { otherPartners, countyParish, name, mpo },
        id,
      })
      refetchJurisdictions()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  const filterAssociatedObjects = (
    jurisdictionId: number,
    objects: Location[]
  ): { id: number, name: string }[] => {
    const associatedLocations = objects.filter((object) => {
      return object.jurisdictionId === jurisdictionId;
    });
  
    return associatedLocations.map((location) => ({
      id: location.id,
      name: `${location.primaryName} & ${location.secondaryName}`
    }));
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!jurisdictions) {
    return <div>Error returning data</div>
  }

  const filteredData = jurisdictions.map((obj: Jurisdiction) => {
    return {
      id: obj.id,
      name: obj.name,
      mpo: obj.mpo,
      countyParish: obj.countyParish,
      otherPartners: obj.otherPartners,
    }
  })

  const headers = ['Name', 'Mpo', 'County/Parish', 'Other Partners']
  const headerKeys = ['name', 'mpo', 'countyParish', 'otherPartners']

  return (
    <ResponsivePageLayout title="Manage Jurisdictions" noBottomMargin>
      <AdminTable
        pageName="Jurisdictions"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={
          <JurisdictionEditorModal
            isOpen={true}
            onSave={HandleEditJurisdiction}
            onClose={onModalClose}
          />
        }
        createModal={
          <JurisdictionEditorModal
            isOpen={true}
            onSave={HandleCreateJurisdiction}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Jurisdiction"
            open={false}
            onClose={() => {}}
            onConfirm={HandleDeleteJurisdiction}
            associatedObjects={locations}
            associatedObjectsLabel="locations"
            filterFunction={filterAssociatedObjects}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default JurisdictionsAdmin
