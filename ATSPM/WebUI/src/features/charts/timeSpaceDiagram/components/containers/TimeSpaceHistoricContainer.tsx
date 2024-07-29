import { Route } from '@/features/routes/types'
import { TimeSpaceHistoricOptions } from '../../types'
import { HistoricOptionsComponent } from '../HistoricOptionsComponent'
import { useHistoricOptionsHandler } from '../handlers/handlers'

interface Props {
  handleToolOptions(value: Partial<TimeSpaceHistoricOptions>): void
  routes: Route[]
}

export const TimeSpaceHistoricContainer = ({
  handleToolOptions,
  routes,
}: Props) => {
  const handler = useHistoricOptionsHandler({ routes, handleToolOptions })

  return <HistoricOptionsComponent handler={handler} />
}
