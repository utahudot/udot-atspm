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
import { Tab } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'

type MapLayer = {
  id: number
  name: string
  mapURL: string
  showByDefault: boolean
  serviceType?: 'mapserver' | 'featureserver' // Add this
  blob: Blob
}

export const MapLayersMockData: MapLayer[] = [
  {
    id: 1,
    name: 'Udot Traffic Speed',
    mapURL:
      'https://maps.udot.utah.gov/central/rest/services/TrafficAndSafety/UDOT_Speed_Limits/MapServer/0/query?where=1%3D1&outFields=*&f=geojson',
    showByDefault: true,
    serviceType: 'mapserver',
    blob: new Blob(),
  },
  {
    id: 2,
    name: 'ArcGis Earthquakes since 1970',
    mapURL:
      'https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/FeatureServer/0',
    showByDefault: false,
    serviceType: 'featureserver',
    blob: new Blob(),
  },
]

const Admin = () => {
  const pageAccess = useViewPage(PageNames.MapLayers)
  const [currentTab, setCurrentTab] = useState('1')
  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.MapLayers
  ) as GridColDef[]

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDelteClaim = useUserHasClaim('LocationConfiguration:Delete')

  //   const { data: mapLayerData, isLoading } = useGetApiGetMapLayers()

  //   useEffect(() => {
  //     if (mapLayerData) {
  //       setData(mapLayerData)
  //     }
  //   }, [mapLayerData])

  //   if (pageAccess.isLoading) {
  //     return
  //   }

  const handleChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  const HandleCreateMapLayer = async (mapLayerData: MapLayer) => {
    // const { id, name, mapURL,showByDefault,blob } = mapLayerData
    // try {
    //   await createMutation.mutateAsync({ id, name })
    // } catch (error) {
    //   console.error('Mutation Error:', error)
    // }
  }

  const HandleDeleteMapLayer = async (mapLayerData: MapLayer) => {
    const { id } = mapLayerData
    // try {
    //   await deleteMutation.mutateAsync(id)
    // } catch (error) {
    //   console.error('Mutation Error:', error)
    // }
  }

  const HandleEditMapLayer = async (mapLayerData: MapLayer) => {
    const { id, name } = mapLayerData
    // try {
    //   await editMutation.mutateAsync({ data: { name }, id })
    // } catch (error) {
    //   console.error('Mutation Error:', error)
    // }
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

  //   if (isLoading) {
  //     return (
  //       <Backdrop open>
  //         <CircularProgress color="inherit" />
  //       </Backdrop>
  //     )
  //   }

  //   if (!data) {
  //     return <div>Error returning data</div>
  //   }

  const filteredData = MapLayersMockData.map((obj: any) => {
    return {
      id: obj.id,
      name: obj.name,
      mapURL: obj.mapURL,
      showByDefault: obj.showByDefault,
      serviceType: obj.serviceType
    }
  })

  const baseType = {
    name: '',
    mapURL: '',
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
            customModal={<MapLayerCreateEditModal />}
          />
        </TabPanel>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default Admin
