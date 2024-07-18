import { useGetRoutes } from '@/features/routes/api'
import { Route } from '@/features/routes/types'
import { useEffect, useState } from 'react'

export interface RouteHandler {
  routeId: string
  routes: Route[]
  changeRouteId(routeId: string): void
}

export const useRouteHandler = () => {
  const { data } = useGetRoutes()
  const [routeId, setRouteId] = useState('')
  const [routes, setRoutes] = useState<Route[]>([])

  useEffect(() => {
    if (data) {
      const sortedRoutes = data.value.sort((a, b) => {
        return a.name.localeCompare(b.name)
      })
      setRoutes(sortedRoutes)
    }
  }, [data])

  const component: RouteHandler = {
    routes,
    routeId,
    changeRouteId(routeId) {
      setRouteId(routeId)
    },
  }

  return component
}
