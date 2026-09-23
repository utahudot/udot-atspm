import { TSAverageHandler } from '@/features/charts/timeSpaceDiagram/average/TimeSpaceAverageOptions/timeSpaceAverageOptions.handler'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceRouteSelect/TimeSpaceRouteSelect'
import SequenceAndCoordinationComponent, {
  TIME_SPACE_AVERAGE_ROUTE_PANEL_WIDTH,
} from './SequenceAndCoordinationSelector'

interface Props {
  handler: TSAverageHandler
}

export const TimeSpaceAverageRouteSelect = ({ handler }: Props) => (
  <TimeSpaceRouteSelect
    handler={handler}
    paperSx={
      handler.routeId
        ? {
            boxSizing: 'border-box',
            flex: '0 0 auto',
            maxWidth: TIME_SPACE_AVERAGE_ROUTE_PANEL_WIDTH,
            minWidth: TIME_SPACE_AVERAGE_ROUTE_PANEL_WIDTH,
            width: TIME_SPACE_AVERAGE_ROUTE_PANEL_WIDTH,
          }
        : undefined
    }
    renderRouteDetails={(routeRows) => (
      <SequenceAndCoordinationComponent
        routeRows={routeRows}
        locationWithSequence={handler.routeLocationWithSequence}
        locationWithCoordPhases={handler.routeLocationWithCoordPhases}
        updateLocationWithCoordPhases={handler.updateLocationWithCoordPhases}
        updateLocationWithSequence={handler.updateLocationWithSequence}
      />
    )}
  />
)

export default TimeSpaceAverageRouteSelect
