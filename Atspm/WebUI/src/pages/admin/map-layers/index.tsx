import {
  useDeleteMapLayerFromKey,
  useGetMapLayer,
  usePatchMapLayerFromKey,
  usePostMapLayer,
} from '@/api/config/aTSPMConfigurationApi'
import { MapLayer } from '@/api/config/aTSPMConfigurationApi.schemas'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import MapLayerCreateEditModal from '@/features/mapLayer/components/MapLayerCreateEditModal'
import { Backdrop, CircularProgress } from '@mui/material'

const MapLayers = () => {
  const pageAccess = useViewPage(PageNames.MapLayers)

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const {
    data: mapLayerData,
    isLoading,
    refetch: fetchMapLayers,
  } = useGetMapLayer()
  const { mutateAsync: addMapLayer } = usePostMapLayer()
  const { mutateAsync: updateMapLayer } = usePatchMapLayerFromKey()
  const { mutateAsync: removeMapLayer } = useDeleteMapLayerFromKey()

  const mapLayers = mapLayerData?.value as MapLayer[]

  if (pageAccess.isLoading) {
    return
  }

  const handleCreateMapLayer = async (mapLayerData: MapLayer) => {
    try {
      await addMapLayer({ data: mapLayerData })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleDeleteMapLayer = async (id: number) => {
    try {
      await removeMapLayer({ key: id })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleEditMapLayer = async (mapLayerData: MapLayer) => {
    if (!mapLayerData.id) {
      console.error('Map Layer ID is required for editing')
      return
    }
    try {
      await updateMapLayer({
        key: mapLayerData.id,
        data: mapLayerData,
      })
      fetchMapLayers()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const onModalClose = () => {
    //do something?? potentially just delete
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  if (!mapLayers) {
    return <div>Error returning data</div>
  }

  const filteredData = mapLayers.map((obj: MapLayer) => {
    return {
      id: obj.id,
      name: obj.name,
      mapLayerUrl: obj.mapLayerUrl,
      showByDefault: obj.showByDefault,
      serviceType: obj.serviceType,
      refreshIntervalSeconds:obj.refreshIntervalSeconds
    }
  })

  const headers = ['Name', 'Map Layer Url', 'Show By Default', 'Service Type', "Refresh in Seconds"]
  const headerKeys = ['name', 'mapLayerUrl', 'showByDefault', 'serviceType','refreshIntervalSeconds']

  const customCellRender = [
    {
      headerKey: 'showByDefault',
      component: (value: any) => <>{value ? 'true' : 'false'}</>,
    },
  ]
  return (
    <ResponsivePageLayout title="Manage Map Layers" noBottomMargin>
      <AdminTable
        pageName="Map Layer"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        customCellRender={customCellRender}
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
            name={''}
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
