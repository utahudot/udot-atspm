import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { useDeleteImpacts } from '@/features/speedManagementTool/api/deleteImpact'
import { useGetImpacts } from '@/features/speedManagementTool/api/getImpacts'
import { usePostImpacts } from '@/features/speedManagementTool/api/postImpacts'
import ImpactEditorModal from '@/features/speedManagementTool/modals/impactEditorModal'
import { Impact, ImpactType } from '@/features/speedManagementTool/types/impact'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'

const ImpactAdmin = () => {
  const pageAccess = useViewPage(PageNames.Impacts)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Impacts
  ) as GridColDef[]

  const createMutation = usePostImpacts()
  const deleteMutation = useDeleteImpacts()
  // const editMutation = useEditArea()

  const { data: impactsData, isLoading } = useGetImpacts()

  const HandleCreateImpact = async (impactData: Impact) => {
    const { description, start,end,startMile,endMile, impactTypeIds, impactTypes} = impactData
    try {
        await createMutation.mutateAsync({ description, start,end,startMile,endMile, impactTypeIds, impactTypes})
    } catch (error) {
        console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteImpact = async (impactData: Impact) => {
    const { id } = impactData
    try {
        await deleteMutation.mutateAsync(id)
    } catch (error) {
        console.error('Mutation Error:', error)
    }
  }

  const HandleEditImpact = async (impactData: Impact) => {
    // const { description, start,end,startMile,endMile, impactTypeId, impactTypes} = impactData
    console.log('Test edit')
    try {
      //   await editMutation.mutateAsync({ data: { name }, id })
    } catch (error) {
      //   console.error('Mutation Error:', error)
      console.log('Test edit')
    }
  }

  const deleteImpact = (data: Impact) => {
    HandleDeleteImpact(data)
  }

  const editImpact = (data: Impact) => {
    HandleEditImpact(data)
  }

  const createImpact = (data: Impact) => {
    HandleCreateImpact(data)
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!impactsData) {
    return <div>Error returning data</div>
  }
  let impactTypeNames = 'hello'

  const filteredData = impactsData.map((obj: any) => {
    impactTypeNames = obj.impactTypes
      .map((impactType: ImpactType) => impactType.name)
      .join(', ')

    return {
      id: obj.id,
      description: obj.description,
      start: obj.start,
      end: obj.end,
      startMile: obj.startMile,
      endMile: obj.endMile,
      impactTypeId: obj.impactTypeId,
      impactTypes: obj.impactTypes,
    }
  })

  const baseType = {
    description: '',
    start: '',
    end: '',
    startMile: '',
    endMile: '',
    impactTypes: '',
  }

  return (
    <ResponsivePageLayout title={'Impact Editor'} noBottomMargin>
      <GenericAdminChart
        pageName={PageNames.Impacts}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteImpact}
        onEdit={editImpact}
        onCreate={createImpact}
        hasEditPrivileges={true}
        hasDeletePrivileges={true}
        customModal={
          <ImpactEditorModal
            onCreate={createImpact}
            onEdit={editImpact}
            onDelete={deleteImpact}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ImpactAdmin
