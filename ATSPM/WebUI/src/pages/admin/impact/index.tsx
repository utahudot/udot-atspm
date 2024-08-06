import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useViewPage } from '@/features/identity/pagesCheck'
import { Impact, ImpactType } from '@/features/speedManagementTool/types/impact'
import { GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'

const ImpactAdmin = () => {
  const pageAccess = useViewPage(PageNames.Impacts)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Impacts
  ) as GridColDef[]

  // const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  // const hasLocationsDelteClaim = useUserHasClaim('LocationConfiguration:Delete')

  // const createMutation = useCreateArea()
  // const deleteMutation = useDeleteArea()
  // const editMutation = useEditArea()

  // const { data: areaData, isLoading } = useGetAreas()

  // if (pageAccess.isLoading) {
  //   return
  // }

  const impactTypes: ImpactType[] = [
    {
      id: '1',
      name: 'Roadwork',
      description: 'Construction work on the road',
    },
    {
      id: '2',
      name: 'Accident',
      description: 'Traffic accident causing delays',
    },
  ]

  const impacts: Impact[] = [
    {
      id: '101',
      description: 'Lane closure due to roadwork',
      start: new Date('2024-08-01T08:00:00Z'),
      end: new Date('2024-08-01T17:00:00Z'),
      startMile: 10,
      endMile: 12,
      impactTypeId: '1',
      impactType: impactTypes[0],
    },
    {
      id: '102',
      description: 'Traffic delay due to accident',
      start: new Date('2024-08-02T09:00:00Z'),
      end: new Date('2024-08-02T11:00:00Z'),
      startMile: 20,
      endMile: 22,
      impactTypeId: '2',
      impactType: impactTypes[1],
    },
  ]

  const HandleCreateImpact = async (impactData: Impact) => {
    // const { description, start,end,startMile,endMile, impactTypeId, impactType} = impactData
    try {
      //   await createMutation.mutateAsync({ description, start,end,startMile,endMile, impactTypeId, impactType})
      console.log('Test create')
    } catch (error) {
      //   console.error('Mutation Error:', error)
      console.log('Test create')
    }
  }

  const HandleDeleteImpact = async (impactData: Impact) => {
    // const { id } = impactData
    try {
      //   await deleteMutation.mutateAsync(id)
      console.log('Test delete')
    } catch (error) {
      //   console.error('Mutation Error:', error)
      console.log('Test delete')
    }
  }

  const HandleEditImpact = async (impactData: Impact) => {
    // const { description, start,end,startMile,endMile, impactTypeId, impactType} = impactData
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

  const filteredData = impacts.map((obj: Impact) => {
    return {
      id: obj.id,
      description: obj.description,
      start: obj.start,
      end: obj.end,
      startMile: obj.startMile,
      endMile: obj.endMile,
      impactTypeId: obj.impactTypeId,
      impactType: obj.impactType,
    }
  })

  const baseType = {
    id: '',
    description: '',
    start: '',
    startMile: '',
    endMile: '',
    impactTypeId: '',
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
      />
    </ResponsivePageLayout>
  )
}

export default ImpactAdmin
