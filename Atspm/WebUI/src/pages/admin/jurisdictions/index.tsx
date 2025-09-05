import {
  Jurisdiction,
  useDeleteJurisdictionFromKey,
  useGetJurisdiction,
  useGetLocationLocationsForSearch,
  usePatchJurisdictionFromKey,
  usePostJurisdiction,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'

import JurisdictionEditorModal from '@/features/jurisdictions/components/JurisdictionEditorModal'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'

const JurisdictionsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Jurisdiction)
  const { addNotification } = useNotificationStore()
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const {
    data: jurisdictionData,
    isLoading,
    refetch: refetchJurisdictions,
  } = useGetJurisdiction()
  const { mutateAsync: createMutation } = usePostJurisdiction()
  const { mutateAsync: editMutation } = usePatchJurisdictionFromKey()
  const { mutateAsync: deleteMutation } = useDeleteJurisdictionFromKey()
  const { data: locationsData } = useGetLocationLocationsForSearch()
  const locations = locationsData?.value

  const jurisdictions = jurisdictionData?.value

  if (pageAccess.isLoading) {
    return null
  }

  const HandleCreateJurisdiction = async (jurisdictionData: Jurisdiction) => {
    try {
      await createMutation({
        data: jurisdictionData,
      })
      refetchJurisdictions()
      addNotification({ title: 'Jurisdiction Created', type: 'success' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: 'Error Creating Jurisdiction', type: 'error' })
    }
  }

  const HandleEditJurisdiction = async (jurisdictionData: Jurisdiction) => {
    try {
      await editMutation({
        data: jurisdictionData,
        key: jurisdictionData.id,
      })
      refetchJurisdictions()
      addNotification({ title: 'Jurisdiction Edited', type: 'success' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: 'Error Editing Jurisdiction', type: 'error' })
    }
  }

  const HandleDeleteJurisdiction = async (id: number) => {
    try {
      await deleteMutation({ key: id })
      refetchJurisdictions()
      addNotification({ title: 'Jurisdiction Deleted', type: 'success' })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({ title: 'Error Deleting Jurisdiction', type: 'error' })
    }
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  const filterAssociatedObjects = (
    jurisdictionId: number,
    objects: Location[]
  ): { id: number; name: string }[] => {
    const associatedLocations = objects.filter((object) => {
      return object.jurisdictionId === jurisdictionId
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
        pageName="Jurisdiction"
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
