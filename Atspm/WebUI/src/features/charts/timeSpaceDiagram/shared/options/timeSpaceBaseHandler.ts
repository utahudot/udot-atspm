import { Route } from '@/features/routes/types'

export interface TSBaseHandler {
  routes: Route[]
  routeId: string
  speedLimit: number | null

  setRouteId(routeId: string): void
  setSpeedLimit(speedLimit: number | null): void
}
