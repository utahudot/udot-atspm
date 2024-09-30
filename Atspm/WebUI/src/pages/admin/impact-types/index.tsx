import {
  useDeleteApiV1ImpactTypeId,
  useGetApiV1ImpactType,
  usePostApiV1ImpactType,
  usePutApiV1ImpactTypeId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames } from '@/features/identity/pagesCheck'
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'

const ImpactTypeAdmin = () => {
  //   const pageAccess = useViewPage(PageNames.ImpactTypes)
  const { addNotification } = useNotificationStore()

  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.ImpactTypes
  ) as GridColDef[]

  const createMutation = usePostApiV1ImpactType()
  const deleteMutation = useDeleteApiV1ImpactTypeId()
  const editMutation = usePutApiV1ImpactTypeId()

  const { data: impactTypesData, isLoading } = useGetApiV1ImpactType()

  const handleCreateImpactType = async (impactType: ImpactType) => {
    const { name, description } = impactType
    const response = await createMutation.mutateAsync({
      data: { name, description },
    })

    if (response) {
      addNotification({
        title: 'Impact Type created successfully',
        type: 'success',
      })
    }
  }

  const handleDeleteImpactType = async (impactType: ImpactType) => {
    const { id } = impactType
    if (!id) return

    await deleteMutation.mutateAsync(
      { id },
      {
        onSuccess: () =>
          addNotification({
            title: 'Impact Type deleted successfully',
            type: 'success',
          }),
      }
    )
  }

  const handleEditImpactType = async (impactType: ImpactType) => {
    const { id, name, description } = impactType
    console.log('impactType', impactType)
    if (id) editMutation.mutateAsync({ id, data: { name, description } })
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

  const filteredData = impactTypesData.map((obj: ImpactType) => {
    return {
      id: obj.id,
      name: obj.name,
      description: obj.description,
    }
  })

  const baseType = {
    description: '',
    name: '',
  }

  return (
    <ResponsivePageLayout title={'Impact Types'} noBottomMargin>
      <GenericAdminChart
        pageName={PageNames.ImpactTypes}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={handleDeleteImpactType}
        onEdit={handleEditImpactType}
        onCreate={handleCreateImpactType}
        hasEditPrivileges={true}
        hasDeletePrivileges={true}
      />
    </ResponsivePageLayout>
  )
}

export default ImpactTypeAdmin
