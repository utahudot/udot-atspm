import { navigateToPage } from '@/utils/routes'

import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useCreateData } from '@/features/generic/api/createData'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import { useDeleteRoute } from '@/features/routes/api'
import { useGetRoute } from '@/features/routes/api/getRoutes'
import CreateRouteModal from '@/features/routes/components/CreateRouteModal'
import { Route } from '@/features/routes/types'
import { Backdrop, CircularProgress } from '@mui/material'

const apiCall = 'Route'

const RoutesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Routes)

  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createRoute } = useCreateData({ apiCall })
  const { mutateAsync: deleteRoute } = useDeleteRoute()

  const { data: routeData, isLoading, refetch: refetchRoutes } = useGetRoute()
  const routes = routeData?.value

  if (pageAccess.isLoading) {
    return
  }

  const HandleCreateRoute = async (routeData: Route) => {
    const { id, name } = routeData
    try {
      const newRoute = await createRoute({
        data: { id, name },
        apiCall,
      })
      console.log('new route create', newRoute.id)
      navigateToPage(`/admin/routes/${newRoute.id}/edit`)
      console.log('new route create 2', newRoute.id)

      refetchRoutes()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleDeleteRoute = async (id: Route) => {
    try {
      await deleteRoute(id)
      refetchRoutes()
    } catch (error) {
      console.error('Mutation Error:', error)
    }
  }

  const HandleEditRoute = (route: Route) => {
    navigateToPage(`/admin/routes/${route.id}/edit`)
  }

  if (isLoading) {
    return (
      <Backdrop open>
        <CircularProgress color="inherit" />
      </Backdrop>
    )
  }

  const onModalClose = () => {
    //add code for custom modal close
  }

  if (!routes) {
    return <div>Error returning data</div>
  }

  const filteredData = routes.map((obj: any) => {
    return {
      id: obj.id,
      name: obj.name,
    }
  })

  const headers = ['Name']
  const headerKeys = ['name']

  return (
    <ResponsivePageLayout title="Manage Routes" noBottomMargin>
      <AdminTable
        pageName="Route"
        headers={headers}
        headerKeys={headerKeys}
        data={filteredData}
        hasEditPrivileges={hasLocationsEditClaim}
        hasDeletePrivileges={hasLocationsDeleteClaim}
        customEditFunction={HandleEditRoute}
        createModal={
          <CreateRouteModal
            isOpen={true}
            onSave={HandleCreateRoute}
            onClose={onModalClose}
          />
        }
        deleteModal={
          <DeleteModal
            id={0}
            name={''}
            objectType="Route"
            open={false}
            onConfirm={HandleDeleteRoute}
          />
        }
      />
    </ResponsivePageLayout>
  )
}

export default RoutesAdmin
