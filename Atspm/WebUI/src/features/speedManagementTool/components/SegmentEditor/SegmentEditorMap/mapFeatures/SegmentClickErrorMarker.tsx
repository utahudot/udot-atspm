import { Alert, Typography } from '@mui/material'
import L from 'leaflet'
import { Marker, Tooltip } from 'react-leaflet'

const tooltipCSS = `
  .route-error-tooltip.leaflet-tooltip {
    margin: 0 !important;
    padding: 0 !important;
    border: '1px solid red' !important;
  }
  .route-error-tooltip.leaflet-tooltip .leaflet-tooltip-content {
    margin: 0 !important;
    padding: 0 !important;
    border: '1px solid red' !important;
  }
`

const SegmentClickErrorMarker = (
  positionObj: { position: [number, number] } | null
) => {
  if (!positionObj?.position) return null
  const { position } = positionObj

  return (
    <>
      <style>{tooltipCSS}</style>
      <Marker
        position={[position[1], position[0]]}
        icon={L.divIcon({ html: '', className: 'invisible-marker' })}
        interactive={false}
      >
        <Tooltip
          permanent
          direction="top"
          offset={[0, -10]}
          className="route-error-tooltip"
        >
          <Alert severity="error" sx={{ p: 1, m: 0 }}>
            <Typography variant="caption">
              No route found within range.
            </Typography>
          </Alert>
        </Tooltip>
      </Marker>
    </>
  )
}

export default SegmentClickErrorMarker
