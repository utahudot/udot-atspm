import {
  useDeleteMapLayerFromKey,
  useGetMapLayer,
  usePatchMapLayerFromKey,
  usePostMapLayer,
} from '@/api/config/aTSPMConfigurationApi'
import { MapLayer } from '@/api/config/aTSPMConfigurationApi.schemas'
import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import MapLayerCreateEditModal from '@/components/GenericAdminChart/MapLayerCreateEditModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Backdrop, CircularProgress, Tab } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const MapLayers = () => {
  const pageAccess = useViewPage(PageNames.MapLayers)
  const [currentTab, setCurrentTab] = useState('1')
  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.MapLayers
  ) as GridColDef[]

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDelteClaim = useUserHasClaim('LocationConfiguration:Delete')

  const {
    data: mapLayerData,
    isLoading,
    refetch: fetchMapLayers,
  } = useGetMapLayer()
  const { mutate: addMapLayer } = usePostMapLayer()
  const { mutate: updateMapLayer } = usePatchMapLayerFromKey()
  const { mutate: removeMapLayer } = useDeleteMapLayerFromKey()
  useEffect(() => {
    if (mapLayerData) {
      setData(mapLayerData)
    }
  }, [mapLayerData])

  if (pageAccess.isLoading) {
    return
  }

  const handleChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  const handleCreateMapLayer = async (mapLayerData: MapLayer) => {
    try {
      await addMapLayer({ data: mapLayerData })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleDeleteMapLayer = async (mapLayerData: MapLayer) => {
    const { id } = mapLayerData
    console.log(id)
    try {
      await removeMapLayer({ key: id })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const handleEditMapLayer = async (mapLayerData: MapLayer) => {
    try {
      await updateMapLayer(
        {
          key: mapLayerData.id,
          data: mapLayerData,
        },
        {
          onSuccess: () => {
            fetchMapLayers()
          },
        }
      )
    } catch (error) {
      console.error('Mutation Error:', error)
    }
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

  const filteredData = data?.value.map((obj: any) => {
    return {
      id: obj.id,
      name: obj.name,
      mapLayerUrl: obj.mapLayerUrl,
      showByDefault: obj.showByDefault,
      serviceType: obj.serviceType,
    }
  })

  const baseType = {
    name: '',
    mapLayerUrl: '',
    showByDefault: '',
    serviceType: '',
  }

  console.log('filteredData', filteredData)

  return (
    <ResponsivePageLayout title={'General Admin'} noBottomMargin>
      <TabContext value={currentTab}>
        <TabList
          onChange={handleChange}
          aria-label="Maps"
          textColor="primary"
          indicatorColor="primary"
        >
          <Tab label="Map" value="1" />
        </TabList>
        <TabPanel value="1" sx={{ padding: '0px' }}>
          <GenericAdminChart
            pageName={PageNames.MapLayers}
            headers={headers}
            data={filteredData}
            baseRowType={baseType}
            onDelete={handleDeleteMapLayer}
            onEdit={handleEditMapLayer}
            onCreate={handleCreateMapLayer}
            hasEditPrivileges={hasLocationsEditClaim}
            hasDeletePrivileges={hasLocationsDelteClaim}
            customModal={
              <MapLayerCreateEditModal
                onCreate={handleCreateMapLayer}
                onEdit={handleEditMapLayer}
                onDelete={handleDeleteMapLayer}
              />
            }
          />
        </TabPanel>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default MapLayers
