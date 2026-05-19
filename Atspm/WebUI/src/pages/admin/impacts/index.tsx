import {
  useDeleteApiV1ImpactId,
  useGetApiV1Impact,
  usePostApiV1Impact,
  usePutApiV1ImpactId,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import {
  Impact,
  ImpactType,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import AdminTable from '@/components/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useTransformedSegments } from '@/features/speedManagementTool/api/getSegments'
import ImpactEditorModal from '@/features/speedManagementTool/components/ImpactEditor/ImpactEditorModal'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'
import { useState } from 'react'

const ImpactAdmin = () => {
  useViewPage(PageNames.Impacts)

  const { addNotification } = useNotificationStore()

  const { mutateAsync: createImpact } = usePostApiV1Impact()
  const { mutateAsync: deleteImpact } = useDeleteApiV1ImpactId()
  const { mutateAsync: editImpact } = usePutApiV1ImpactId()

  const hasEditClaim = useUserHasClaim('SpeedConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('SpeedConfiguration:Delete')

  const {
    data: impacts,
    isLoading,
    refetch: refetchImpacts,
  } = useGetApiV1Impact<Impact[]>()

  const { data: segments, isLoading: isSegmentsLoading } =
    useTransformedSegments()

  // State to force re-render of GenericAdminChart
  const [dataVersion, setDataVersion] = useState(0)

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
      })
      await refetchImpacts()
      setDataVersion((prev) => prev + 1)
      addNotification({
        title: 'Impact Created',
        type: 'success',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: 'Error Creating Impact',
        type: 'error',
      })
    }
  }

  const handleDeleteImpact = async (id: string) => {
    if (!id) return
    try {
      await deleteImpact({ id })
      await refetchImpacts()
      setDataVersion((prev) => prev + 1)
      addNotification({
        title: 'Impact Deleted',
        type: 'success',
      })
    } catch (error) {
      console.error('Delete Mutation Error:', error)
      addNotification({
        title: 'Error Deleting Impact',
        type: 'error',
      })
    }
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
      })
      await refetchImpacts()
      setDataVersion((prev) => prev + 1)
      addNotification({
        title: 'Impact Updated',
        type: 'info',
      })
    } catch (error) {
      console.error('Edit Mutation Error:', error)
      addNotification({
        title: 'Error Updating Impact',
        type: 'error',
      })
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
    impactTypeNames =
      impact?.impactTypes
        ?.map((impactType: ImpactType) => impactType?.name)
        .join(', ') || 'none'

    return {
      ...impact,
      name: impact.description,
      impactTypes: impact.impactTypes || [],
      impactTypesNames: impactTypeNames,
    }
  })

  const headers = [
    'Description',
    'Start',
    'End',
    'Start Mile',
    'End Mile',
    'Impact Types',
  ]

  const headerKeys = [
    'description',
    'start',
    'end',
    'startMile',
    'endMile',
    'impactTypesNames',
  ]

  return (
    <ResponsivePageLayout title={'Impacts'} noBottomMargin>
      <AdminTable
        key={dataVersion}
        pageName={'Impacts'}
        headers={headers}
        headerKeys={headerKeys}
        data={filteredImpacts}
        onDelete={handleDeleteImpact}
        onEdit={handleEditImpact}
        onCreate={handleCreateImpact}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
        editModal={
          <ImpactEditorModal
            onEdit={handleEditImpact}
            onDelete={handleDeleteImpact}
            segments={segments}
            isSegmentsLoading={isSegmentsLoading}
          />
        }
        createModal={
          <ImpactEditorModal
            onCreate={handleCreateImpact}
            onDelete={handleDeleteImpact}
            segments={segments}
            isSegmentsLoading={isSegmentsLoading}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Impact"
            open={false}
            onClose={() => {}}
            onConfirm={handleDeleteImpact}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default ImpactAdmin
