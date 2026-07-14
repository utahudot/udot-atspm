import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import {
  useCreateMapLayer,
  useDeleteMapLayer,
  useGetMapLayers,
  useUpdateMapLayer,
} from '@/features/mapLayers/api'
import MapLayerCreateEditModal from '@/features/mapLayers/MapLayerEditorModal'
import type { MapLayer, PersistedMapLayer } from '@/features/mapLayers/types'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, Checkbox, CircularProgress } from '@mui/material'

const BooleanCell = ({ value }: { value: unknown }) => (
  <Checkbox checked={Boolean(value)} disabled />
)

const MapLayers = () => {
  const pageAccess = useViewPage(PageNames.MapLayers)
  const { addNotification } = useNotificationStore()
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const {
    data: mapLayers = [],
    isLoading,
    refetch: fetchMapLayers,
  } = useGetMapLayers()
  const { mutateAsync: addMapLayer } = useCreateMapLayer()
  const { mutateAsync: updateMapLayer } = useUpdateMapLayer()
  const { mutateAsync: removeMapLayer } = useDeleteMapLayer()

  if (pageAccess.isLoading) {
    return null
  }

  const handleCreateMapLayer = async (mapLayerData: MapLayer) => {
    try {
      await addMapLayer(mapLayerData)
      addNotification({
        title: 'Map Layer created successfully',
        type: 'success',
      })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: 'Error creating Map Layer',
        type: 'error',
      })
    }
  }

  const handleDeleteMapLayer = async (id: string | number) => {
    try {
      await removeMapLayer(Number(id))
      addNotification({
        title: 'Map Layer deleted successfully',
        type: 'success',
      })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: 'Error deleting Map Layer',
        type: 'error',
      })
    }
  }

  const handleEditMapLayer = async (mapLayerData: MapLayer) => {
    if (typeof mapLayerData.id !== 'number') {
      addNotification({
        title: 'Map Layer ID is required for editing',
        type: 'error',
      })
      return
    }

    try {
      await updateMapLayer(mapLayerData as PersistedMapLayer)
      addNotification({
        title: 'Map Layer updated successfully',
        type: 'success',
      })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        title: 'Error updating Map Layer',
        type: 'error',
      })
    }
  }

  const onModalClose = () => undefined

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  const cells = [
    { key: 'name', label: 'Name' },
    { key: 'mapLayerUrl', label: 'Url' },
    { key: 'serviceType', label: 'Service Type' },
    { key: 'resourceId', label: 'Resource ID' },
    { key: 'style', label: 'Style' },
    {
      key: 'showByDefault',
      label: 'Show by Default?',
      component: BooleanCell,
    },
    { key: 'refreshIntervalSeconds', label: 'Refresh Rate (Seconds)' },
  ]

  return (
    <ResponsivePageLayout title="Manage Map Layers" noBottomMargin>
      <AdminTable
        pageName="Map Layer"
        cells={cells}
        data={mapLayers}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        editModal={
          <MapLayerCreateEditModal
            isOpen={true}
            onSave={handleEditMapLayer}
            onClose={onModalClose}
          />
        }
        createModal={
          <MapLayerCreateEditModal
            isOpen={true}
            onSave={handleCreateMapLayer}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name=""
            objectType="Map Layer"
            open={false}
            onClose={onModalClose}
            onConfirm={handleDeleteMapLayer}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default MapLayers
