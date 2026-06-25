import { Route } from '@/features/routes/types'
import { TimeSpaceAverageOptions } from '../../types'
import { AverageOptionsComponent } from '../AverageOptionsComponent'
import { useAverageOptionsHandler } from '../handlers/handlers'

interface Props {
  handleToolOptions(value: Partial<TimeSpaceAverageOptions>): void
  routes: Route[]
  routeId: string
  setRouteId(routeId: string): void
}

export const TimeSpaceAverageContainer = ({
  handleToolOptions,
  routes,
  routeId,
  setRouteId,
}: Props) => {
  const handler = useAverageOptionsHandler({
    routeId,
    setRouteId,
    routes,
    handleToolOptions,
  })

  return <AverageOptionsComponent handler={handler} />
}
