import RoutesToggle from './RoutesToggle'
import ViolationRangeInput from './ViolationRangeSlider'

function SidePanel({ selectedRouteId }: { selectedRouteId: number }) {
  return (
    <div id="sidepanel-container">
      <RoutesToggle />
      <ViolationRangeInput />
      <hr />
    </div>
  )
}

export default SidePanel
