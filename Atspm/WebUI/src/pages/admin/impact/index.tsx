import {
  useDeleteApiV1ImpactId,
  useGetApiV1Impact,
  useGetApiV1SegmentAllSegments,
  usePostApiV1Impact,
  usePutApiV1ImpactId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import ImpactEditorModal from '@/features/speedManagementTool/components/ImpactEditorModal'
import { Impact, ImpactType } from '@/features/speedManagementTool/types/impact'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useMemo } from 'react'

interface AllSegmentsDto {
  type: string
  features: Feature[]
}

interface Feature {
  type: string
  geometry: Geometry
  properties: Properties
}

interface Geometry {
  type: string
  coordinates: number[][]
}

interface Properties {
  AccessCategory: string
  AlternateIdentifier: string
  City: string
  County: string
  Direction: string
  EndMilePoint: number
  FunctionalType: string
  Id: string
  Name: string
  Offset: number
  Region: string
  SpeedLimit: number
  StartMilePoint: number
  UdotRouteNumber: string
}

const ImpactAdmin = () => {
  useViewPage(PageNames.Impacts)

  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Impacts
  ) as GridColDef[]

  const { mutateAsync: createImpact } = usePostApiV1Impact()
  const { mutateAsync: deleteImpact } = useDeleteApiV1ImpactId()
  const { mutateAsync: editImpact } = usePutApiV1ImpactId()

  const hasEditClaim = useUserHasClaim('GeneralConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('GeneralConfiguration:Delete')

  const {
    data: impacts,
    isLoading,
    refetch: refetchImpacts,
  } = useGetApiV1Impact<Impact[]>()
  const { data: segments } = useGetApiV1SegmentAllSegments<AllSegmentsDto>()

  const filteredSegments = useMemo(() => {
    if (!segments) return []
    return (
      segments?.features
        ?.filter((feature) => feature?.geometry?.type === 'LineString')
        ?.map((feature) => ({
          ...feature,
          geometry: {
            ...feature.geometry,
            coordinates: feature?.geometry?.coordinates.map((coord) => [
              coord[1],
              coord[0],
            ]),
          },
        })) || []
    )
  }, [segments])

  const handleCreateImpact = async (impactData: Impact) => {
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
      await createImpact({
        data: {
          description,
          start,
          end,
          startMile,
          endMile,
          impactTypeIds,
          impactTypes,
          segmentIds,
        },
        onSuccess: () => refetchImpacts(),
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleDeleteImpact = async (impactData: Impact) => {
    // const { id } = impactData
    console.log('impactData:', impactData)
    if (!impactData?.id) return
    const { id } = impactData
    const response = await deleteImpact({ id })
    console.log('response:', response)
  }

  const handleEditImpact = async (impactData: Impact) => {
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
      await editImpact({
        data: {
          description,
          start,
          end,
          startMile,
          endMile,
          impactTypeIds,
          impactTypes,
          segmentIds,
        },
        id,
        onSuccess: () => refetchImpacts(),
      })
    } catch (error) {
      console.error('Edit Mutation Error:', error)
    }
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!impacts) {
    return <div>Error returning data</div>
  }
  let impactTypeNames = ''

  const filteredImpacts = impacts.map((impact) => {
    if (!impact.impactTypes) {
      impact.impactTypes = []
    }
    impactTypeNames = impact.impactTypes
      .map((impactType: ImpactType) => impactType?.name)
      .join(', ')

    return {
      id: impact.id,
      description: impact.description,
      start: impact.start,
      end: impact.end,
      startMile: impact.startMile,
      endMile: impact.endMile,
      impactTypeId: impact.impactTypeId,
      impactTypes: impact.impactTypes,
      impactTypesNames: impactTypeNames || 'none',
      segmentIds: impact.segmentIds,
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
        data={filteredImpacts}
        baseRowType={baseType}
        onDelete={handleDeleteImpact}
        onEdit={handleEditImpact}
        onCreate={handleCreateImpact}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        customModal={
          <ImpactEditorModal
            onCreate={handleCreateImpact}
            onEdit={handleEditImpact}
            onDelete={handleDeleteImpact}
            segments={filteredSegments}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ImpactAdmin
