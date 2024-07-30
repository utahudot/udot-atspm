import { Route } from '@/features/routes/types'
import { TimeSpaceAverageOptions } from '../../types'
import { AverageOptionsComponent } from '../AverageOptionsComponent'
import { useAverageOptionsHandler } from '../handlers/handlers'

interface Props {
  handleToolOptions(value: Partial<TimeSpaceAverageOptions>): void
  routes: Route[]
}

export const TimeSpaceAverageContainer = ({
  handleToolOptions,
  routes,
}: Props) => {
  const handler = useAverageOptionsHandler({ routes, handleToolOptions })

  return <AverageOptionsComponent handler={handler} />
}
