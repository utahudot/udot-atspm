import { Box } from '@mui/material'
import ChartsContainer from './ChartsContainer'
import RoutesToggle from './RoutesToggle'
import ViolationRangeInput from './ViolationRangeSlider'

function SidePanel({ selectedRouteId }: { selectedRouteId: number }) {
  return (
    <div id="sidepanel-container">
      <RoutesToggle />
      <ViolationRangeInput />
      <hr />
      {selectedRouteId ? (
        <Box sx={{ height: '500px', overflowY: 'auto' }}>
          <ChartsContainer selectedRouteId={selectedRouteId} />
        </Box>
      ) : null}
    </div>
  )
}

export default SidePanel
