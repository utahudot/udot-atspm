import { Route } from '@/features/routes/types'
import { TimeSpaceHistoricOptions } from '../../types'
import { HistoricOptionsComponent } from '../HistoricOptionsComponent'
import { useHistoricOptionsHandler } from '../handlers/handlers'

interface Props {
  handleToolOptions(value: Partial<TimeSpaceHistoricOptions>): void
  routes: Route[]
  routeId: string
  setRouteId(routeId: string): void
}

export const TimeSpaceHistoricContainer = ({
  handleToolOptions,
  routes,
  routeId,
  setRouteId,
}: Props) => {
  const handler = useHistoricOptionsHandler({
    routeId,
    setRouteId,
    routes,
    handleToolOptions,
  })

  return <HistoricOptionsComponent handler={handler} />
}
