import { pageNameToHeaders } from '@/components/GenericAdminChart'
import RouteChart from '@/components/GenericAdminChart/RouteChart'
import Header from '@/components/header'
import { useCreateData } from '@/features/generic/api/createData'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useDeleteRoute } from '@/features/routes/api'
import { useGetRoute } from '@/features/routes/api/getRoutes'
import { Route } from '@/features/routes/types'
import { Backdrop, CircularProgress } from '@mui/material'
import { GridColDef } from '@mui/x-data-grid'
import Head from 'next/head'
import { useEffect, useState } from 'react'

const apiCall = 'Route'

const RoutesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Routes)

  const [data, setData] = useState<any>(null)
  const headers: GridColDef[] = pageNameToHeaders.get(
    PageNames.Routes
  ) as GridColDef[]

  const hasEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasDeleteClaim = useUserHasClaim('LocationConfiguration:Delete')

  const createMutation = useCreateData({ apiCall })
  const deleteMutation = useDeleteRoute()

  const { data: routeData, isLoading } = useGetRoute()

  useEffect(() => {
    if (routeData) {
      setData(routeData)
    }
  }, [routeData])

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateRoute = async (routeData: Route) => {
    const { id, name } = routeData
    try {
      createMutation.mutateAsync({
        data: { id, name },
        apiCall,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteRoute = async (routeData: Route) => {
    const { id } = routeData
    try {
      deleteMutation.mutateAsync(id)
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditRoute = async (routeData: Route) => {
    const { id, name, body, orderNumber } = routeData
    try {
      editMutation.mutateAsync({
        data: { id, name, body, orderNumber },
        apiCall: `${apiCall}/${id}`,
      })
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const deleteRoute = (data: Route) => {
    HandleDeleteRoute(data)
  }

  const editRoute = (data: Route) => {
    HandleEditRoute(data)
  }

  const createRoute = (data: Route) => {
    HandleCreateRoute(data)
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
      name: obj.name,
    }
  })

  const baseType = {
    name: '',
  }
  const urlPath = '/admin/routes'

  return (
    <>
      <Head>
        <title>Manage Route</title>
      </Head>
      <Header title="Manage Routes" />
      <RouteChart
        headers={headers}
        data={filteredData}
        baseRowType={baseType}
        onDelete={deleteRoute}
        onEdit={editRoute}
        onCreate={createRoute}
        pageName={'Route'}
        urlPath={urlPath}
        hasEditPrivileges={hasEditClaim}
        hasDeletePrivileges={hasDeleteClaim}
      />
    </>
  )
}

export default RoutesAdmin
