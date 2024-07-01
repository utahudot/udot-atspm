import GenericAdminChart, {
  pageNameToHeaders,
} from '@/components/GenericAdminChart'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { PageNames, useUserHasClaim, useViewPage } from '@/features/identity/pagesCheck'
import {
  useCreateJurisdiction,
  useDeleteJurisdiction,
  useEditJurisdiction,
  useGetJurisdiction,
} from '@/features/jurisdictions/api/jurisdictionApi'
import jurisdictionDto from '@/features/jurisdictions/types/jurisdictionDto'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import { useEffect, useState } from 'react'

const JurisdictionsAdmin = () => {
  const pageAccess = useViewPage(PageNames.Jurisdiction) 

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Jurisdiction
  ) as GridColDef[]
  const hasEditClaim = useUserHasClaim('LocationConfiguration:Edit');
  const hasDeleteClaim = useUserHasClaim('LocationConfiguration:Delete');
  const createMutation = useCreateJurisdiction()
  const deleteMutation = useDeleteJurisdiction()
  const editMutation = useEditJurisdiction()
  const { data: locationsData } = useLatestVersionOfAllLocations()

  const locations = locationsData?.value

  const { data: jurisdictionData, isLoading } = useGetJurisdiction()

  
  useEffect(() => {
    if (jurisdictionData) {
      setData(jurisdictionData)
    }
  }, [jurisdictionData])

  if (pageAccess.isLoading) {
    return
  }


  const HandleCreateJurisdiction = async (jurisdictionData: Jurisdiction) => {
    const { id, otherPartners, countyParish, name, mpo } = jurisdictionData
    try {
      createMutation.mutateAsync({
        id,
        otherPartners,
        countyParish,
        name,
        mpo,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteJurisdiction = async (
    jurisdictionData: jurisdictionDto
  ) => {
    const { id } = jurisdictionData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditJurisdiction = async (jurisdictionData: Jurisdiction) => {
    const { id, otherPartners, countyParish, name, mpo } = jurisdictionData
    try {
      editMutation.mutateAsync({
        data: { otherPartners, countyParish, name, mpo },
        id,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteJurisdiction = (data: Jurisdiction) => {
    HandleDeleteJurisdiction(data)
  }

  const editJurisdiction = (data: Jurisdiction) => {
    HandleEditJurisdiction(data)
  }

  const createJurisdiction = (data: Jurisdiction) => {
    HandleCreateJurisdiction(data)
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

  const filteredData = data.value.map((obj: jurisdictionDto) => {
    return {
      id: obj.id,
      name: obj.name,
      mpo: obj.mpo,
      countyParish: obj.countyParish,
      otherPartners: obj.otherPartners,
    }
  })

  const baseType = {
    name: '',
    mpo: '',
    countyParish: '',
    otherPartners: '',
  }

  return (
    <ResponsivePageLayout title={'Jurisdictions'} noBottomMargin>
      <GenericAdminChart
        headers={headers}
        pageName={PageNames.Jurisdiction}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteJurisdiction}
        onEdit={editJurisdiction}
        onCreate={createJurisdiction}
        locations={locations}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
      />
    </ResponsivePageLayout>
  )
}

export default JurisdictionsAdmin
