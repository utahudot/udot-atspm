import {
  useApiV1MapLayer,
  useApiV1MapLayerCount,
  useApiV1MapLayerKey,
  useApiV1MapLayerPutKey,
} from '@/api/config/aTSPMConfigurationApi'
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

type MapLayer = {
  id: number
  name: string
  mapLayerUrl: string
  showByDefault: boolean
  serviceType?: 'mapserver' | 'featureserver'
}

const MapLayers = () => {
  const pageAccess = useViewPage(PageNames.MapLayers)
  const [currentTab, setCurrentTab] = useState('1')
  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.MapLayers
  ) as GridColDef[]

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDelteClaim = useUserHasClaim('LocationConfiguration:Delete')

  const { data: mapLayerData, isLoading } = useApiV1MapLayerCount()
  const createMutation = useApiV1MapLayer()
  const editMutation = useApiV1MapLayerPutKey()
  const deleteMutation = useApiV1MapLayerKey()
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

  const HandleCreateMapLayer = async (mapLayerData: MapLayer) => {
    try {
      await createMutation.mutateAsync({ data: mapLayerData })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteMapLayer = async (mapLayerData: MapLayer) => {
    const { id } = mapLayerData
    console.log(id)
    try {
      await deleteMutation.mutateAsync({ key: id })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditMapLayer = async (mapLayerData: MapLayer) => {
    console.log('HandleEditMapLayer')
    try {
      await editMutation.mutateAsync({
        key: mapLayerData.id,
        data: mapLayerData,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteMapLayer = (data: MapLayer) => {
    HandleDeleteMapLayer(data)
  }

  const editMapLayer = (data: MapLayer) => {
    HandleEditMapLayer(data)
  }

  const createMapLayer = (data: MapLayer) => {
    HandleCreateMapLayer(data)
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
            onDelete={deleteMapLayer}
            onEdit={editMapLayer}
            onCreate={createMapLayer}
            hasEditPrivileges={hasLocationsEditClaim}
            hasDeletePrivileges={hasLocationsDelteClaim}
            customModal={
              <MapLayerCreateEditModal
                onCreate={createMapLayer}
                onEdit={editMapLayer}
                onDelete={deleteMapLayer}
              />
            }
          />
        </TabPanel>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default MapLayers
