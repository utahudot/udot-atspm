import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useRegion } from '@/features/generic/api/getData'
import { PageNames, useUserHasClaim, useViewPage } from '@/features/identity/pagesCheck'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import {
  useCreateRegion,
  useDeleteRegion,
  useEditRegion,
} from '@/features/region/api/regionApi'
import { regionDto } from '@/features/region/types/regionDto'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const RegionsAdmin = () => {
  useViewPage(PageNames.Region)
  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Region
  ) as GridColDef[]

  const hasEditClaim = useUserHasClaim('LocationConfiguration:Edit');
  const hasDeleteClaim = useUserHasClaim('LocationConfiguration:Delete');

  const createMutation = useCreateRegion()
  const deleteMutation = useDeleteRegion()
  const editMutation = useEditRegion()
  const { data: locationsData } = useLatestVersionOfAllLocations()

  const locations = locationsData?.value

  const { data: regionData, isLoading } = useRegion()

  useEffect(() => {
    if (regionData) {
      setData(regionData)
    }
  }, [regionData])

  const HandleCreateRegion = async (regionData: Region) => {
    const { id, description } = regionData
    try {
      createMutation.mutateAsync({ id, description })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteRegion = async (regionData: regionDto) => {
    const { id } = regionData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditRegion = async (regionData: Region) => {
    const { id, description } = regionData
    try {
      editMutation.mutateAsync({
        data: { id, description },
        id,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteRegion = (data: Region) => {
    HandleDeleteRegion(data)
  }

  const editRegion = (data: Region) => {
    HandleEditRegion(data)
  }

  const createRegion = (data: Region) => {
    HandleCreateRegion(data)
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

  const filteredData = data.value.map((obj: any) => {
    return {
      id: obj.id,
      description: obj.description,
    }
  })

  const baseType = {
    description: '',
  }

  return (
    <ResponsivePageLayout title={'Manage Regions'} noBottomMargin>
      <GenericAdminChart
        headers={headers}
        pageName={PageNames.Region}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteRegion}
        onEdit={editRegion}
        onCreate={createRegion}
        locations={locations}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
      />
    </ResponsivePageLayout>
  )
}

export default RegionsAdmin
