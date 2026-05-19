import {
  useDeleteApiV1ImpactTypeId,
  useGetApiV1ImpactType,
  usePostApiV1ImpactType,
  usePutApiV1ImpactTypeId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import AdminTable from '@/components/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import ImpactTypeEditorModal from '@/features/speedManagementTool/components/ImpactTypesEditorModal'
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'

const ImpactTypeAdmin = () => {
  const pageAccess = useViewPage(PageNames.ImpactTypes)

  const hasEditClaim = useUserHasClaim('SpeedConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('SpeedConfiguration:Delete')

  const { addNotification } = useNotificationStore()

  const { mutateAsync: createImpactType } = usePostApiV1ImpactType()
  const { mutateAsync: deleteImpactType } = useDeleteApiV1ImpactTypeId()
  const { mutateAsync: updateImpactType } = usePutApiV1ImpactTypeId()

  const {
    data: impactTypesData,
    isLoading,
    refetch: refetchImpactTypes,
  } = useGetApiV1ImpactType()

  if (pageAccess.isLoading) return null

  const handleCreateImpactType = async (data: ImpactType) => {
    const { name, description } = data
    try {
      await createImpactType({ data: { name, description } })
      await refetchImpactTypes()
      addNotification({ title: 'Impact Type Created', type: 'success' })
    } catch {
      addNotification({ title: 'Error Creating Impact Type', type: 'error' })
    }
  }

  const handleEditImpactType = async (data: ImpactType) => {
    const { id, name, description } = data
    if (!id) return
    try {
      await updateImpactType({ id, data: { name, description } })
      await refetchImpactTypes()
      addNotification({ title: 'Impact Type Updated', type: 'success' })
    } catch {
      addNotification({ title: 'Error Updating Impact Type', type: 'error' })
    }
  }

  const handleDeleteImpactType = async (id: number) => {
    try {
      await deleteImpactType({ id })
      await refetchImpactTypes()
      addNotification({ title: 'Impact Type Deleted', type: 'success' })
    } catch {
      addNotification({ title: 'Error Deleting Impact Type', type: 'error' })
    }
  }

  const onModalClose = () => {
    // custom modal close logic (optional)
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!impactTypesData) {
    return <div>Error returning data</div>
  }

  const filteredData = impactTypesData.map((obj: ImpactType) => ({
    id: obj.id,
    name: obj.name,
    description: obj.description,
  }))

  const headers = ['Name', 'Description']
  const headerKeys = ['name', 'description']

  return (
    <ResponsivePageLayout title="Impact Types" noBottomMargin>
      <AdminTable
        pageName="Impact Type"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        editModal={
          <ImpactTypeEditorModal
            isOpen={true}
            onSave={handleEditImpactType}
            onClose={onModalClose}
          />
        }
        createModal={
          <ImpactTypeEditorModal
            isOpen={true}
            onSave={handleCreateImpactType}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Impact Type"
            open={false}
            onClose={() => {}}
            onConfirm={handleDeleteImpactType}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ImpactTypeAdmin
