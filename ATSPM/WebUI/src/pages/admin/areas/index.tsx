import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  useCreateArea,
  useDeleteArea,
  useEditArea,
  useGetAreas,
} from '@/features/areas/api/areaApi'
import { Area } from '@/features/areas/types'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const AreasAdmin = () => {
  const pageAccess = useViewPage(PageNames.Areas)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Areas
  ) as GridColDef[]

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDelteClaim = useUserHasClaim('LocationConfiguration:Delete')

  const createMutation = useCreateArea()
  const deleteMutation = useDeleteArea()
  const editMutation = useEditArea()
  const { data: locationsData } = useLatestVersionOfAllLocations()

  const locations = locationsData?.value

  const { data: areaData, isLoading } = useGetAreas()

  useEffect(() => {
    if (areaData) {
      setData(areaData)
    }
  }, [areaData])

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateArea = async (areaData: Area) => {
    const { id, name } = areaData
    try {
      createMutation.mutateAsync({ id, name })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteArea = async (areaData: Area) => {
    const { id } = areaData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditArea = async (areaData: Area) => {
    const { id, name } = areaData
    try {
      editMutation.mutateAsync({ data: { name }, id })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteArea = (data: Area) => {
    HandleDeleteArea(data)
  }

  const editArea = (data: Area) => {
    HandleEditArea(data)
  }

  const createArea = (data: Area) => {
    HandleCreateArea(data)
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
    }
  })

  const baseType = {
    name: '',
  }

  return (
    <ResponsivePageLayout title={'Manage Areas'} noBottomMargin>
      <GenericAdminChart
        pageName={PageNames.Areas}
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteArea}
        onEdit={editArea}
        onCreate={createArea}
        locations={locations}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDelteClaim}
      />
    </ResponsivePageLayout>
  )
}

export default AreasAdmin
