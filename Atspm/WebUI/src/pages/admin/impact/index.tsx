import {
  useDeleteApiV1ImpactId,
  useGetApiV1Impact,
  usePostApiV1Impact,
  usePutApiV1ImpactId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import ImpactEditorModal from '@/features/speedManagementTool/components/ImpactEditorModal'
import { Impact, ImpactType } from '@/features/speedManagementTool/types/impact'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const ImpactAdmin = () => {
  useViewPage(PageNames.Impacts)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Impacts
  ) as GridColDef[]

  const createMutation = usePostApiV1Impact()
  const deleteMutation = useDeleteApiV1ImpactId()
  const editMutation = usePutApiV1ImpactId()

  const { data: impactsData, isLoading, refetch } = useGetApiV1Impact()

  console.log('impactsData', impactsData)

  useEffect(() => {
    if (impactsData) {
      setData(impactsData)
    }
  }, [impactsData])

  const HandleCreateImpact = async (impactData: Impact) => {
    const {
      description,
      start,
      end,
      startMile,
      endMile,
      impactTypeIds,
      impactTypes,
      segmentIds,
    } = impactData
    try {
      await createMutation.mutateAsync({
        id: null,
        description,
        start,
        end,
        startMile,
        endMile,
        impactTypeIds,
        impactTypes,
        segmentIds,
      })
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
    const {
      id,
      description,
      start,
      end,
      startMile,
      endMile,
      impactTypeIds,
      impactTypes,
      segmentIds,
    } = impactData
    try {
      await editMutation.mutateAsync({
        impactId: id,
        impactData: {
          description,
          start,
          end,
          startMile,
          endMile,
          impactTypeIds,
          impactTypes,
          segmentIds,
        },
      })
    } catch (error) {
      console.error('Edit Mutation Error:', error)
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

  if (!data) {
    return <div>Error returning data</div>
  }
  let impactTypeNames = ''

  const filteredData = data.map((obj: any) => {
    impactTypeNames = obj.impactTypes
      .map((impactType: ImpactType) => impactType?.name)
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
      impactTypesNames: impactTypeNames || 'none',
      segmentIds: obj.segmentIds,
    }
  })

  const baseType = {
    description: '',
    start: '',
    end: '',
    startMile: '',
    endMile: '',
    impactTypes: '',
    impactTypesNames: '',
  }

  return (
    <ResponsivePageLayout title={'Impacts'} noBottomMargin>
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
          <ImpactEditorModal onCreate={createImpact} onEdit={editImpact} />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ImpactAdmin
