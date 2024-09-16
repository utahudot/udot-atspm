import { useGetApiImpactType } from '@/api/speedManagement'
import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames } from '@/features/identity/pagesCheck'
import { useDeleteImpactType } from '@/features/speedManagementTool/api/deleteImpactType'
import { useEditImpactType } from '@/features/speedManagementTool/api/editImpactType'
import { usePostImpactType } from '@/features/speedManagementTool/api/postImpactType'
import { ImpactType } from '@/features/speedManagementTool/types/impact'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'

const ImpactTypeAdmin = () => {
  //   const pageAccess = useViewPage(PageNames.ImpactTypes)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.ImpactTypes
  ) as GridColDef[]

  const createMutation = usePostImpactType()
  const deleteMutation = useDeleteImpactType()
  const editMutation = useEditImpactType()

  const { data: impactTypesData, isLoading } = useGetApiImpactType()

  const HandleCreateImpactType = async (impactType: ImpactType) => {
    const { name, description } = impactType
    try {
      await createMutation.mutateAsync({ name, description })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteImpactType = async (impactType: ImpactType) => {
    const { id } = impactType
    try {
      await deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditImpactType = async (impactType: ImpactType) => {
    const { id, name, description } = impactType
    console.log('EDIT ', id)
    try {
      await editMutation.mutateAsync({ name, description, id })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteImpactType = (data: ImpactType) => {
    HandleDeleteImpactType(data)
  }

  const editImpactType = (data: ImpactType) => {
    HandleEditImpactType(data)
  }

  const createImpactType = (data: ImpactType) => {
    HandleCreateImpactType(data)
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
        onDelete={deleteImpactType}
        onEdit={editImpactType}
        onCreate={createImpactType}
        hasEditPrivileges={true}
        hasDeletePrivileges={true}
      />
    </ResponsivePageLayout>
  )
}

export default ImpactTypeAdmin
