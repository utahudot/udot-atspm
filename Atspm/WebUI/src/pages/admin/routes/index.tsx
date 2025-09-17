import { navigateToPage } from '@/utils/routes'

import {
  Route,
  useDeleteRouteFromKey,
  useGetRoute,
  usePostRoute,
} from '@/api/config'
import AdminTable from '@/components/AdminTable/AdminTable'
import DeleteModal from '@/components/AdminTable/DeleteModal'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import {
  PageNames,
  useUserHasClaim,
  useViewPage,
} from '@/features/identity/pagesCheck'
import CreateRouteModal from '@/features/routes/components/CreateRouteModal'
import { useNotificationStore } from '@/stores/notifications'
import { Backdrop, CircularProgress } from '@mui/material'

const RoutesAdmin = () => {
  const pageAccess = useViewPage(PageNames.Routes)
  const { addNotification } = useNotificationStore()
  const hasLocationsEditClaim = useUserHasClaim('LocationConfiguration:Edit')
  const hasLocationsDeleteClaim = useUserHasClaim(
    'LocationConfiguration:Delete'
  )

  const { mutateAsync: createRoute } = usePostRoute()
  const { mutateAsync: deleteRoute } = useDeleteRouteFromKey()

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
      })
      navigateToPage(`/admin/routes/${newRoute.id}/edit`)
      addNotification({
        title: 'Route Created',
        type: 'success',
      })

      refetchRoutes()
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Creating Route',
      })
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

  const HandleDeleteRoute = async (id: number) => {
    try {
      await deleteRoute({ key: id })
      refetchRoutes()
      addNotification({
        title: 'Route Deleted',
        type: 'success',
      })
    } catch (error) {
      console.error('Mutation Error:', error)
      addNotification({
        type: 'error',
        title: 'Error Deleting Route',
      })
    }
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
