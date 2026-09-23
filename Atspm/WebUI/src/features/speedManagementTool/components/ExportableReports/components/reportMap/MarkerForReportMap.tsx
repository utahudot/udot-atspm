import { useTheme } from '@mui/material/styles'
import Typography from '@mui/material/Typography'
import L from 'leaflet'
import { useEffect, useRef } from 'react'
import ReactDOMServer from 'react-dom/server'
import { Marker } from 'react-leaflet'

type RankMarkerProps = {
  position: [number, number]
  rank: number
  segmentId?: string
}

const MarkerForReportMap = ({ position, rank }: RankMarkerProps) => {
  const theme = useTheme()

  const iconContentNormal = ReactDOMServer.renderToString(
    <div style={{ position: 'relative' }}>
      <div
        style={{
          width: '28px',
          height: '28px',
          backgroundColor: theme.palette.primary.main,
          borderRadius: '50% 50% 50% 0',
          transform: 'rotate(-45deg)',
          translate: '-25% -60%',
          position: 'relative',
          border: `1px solid ${theme.palette.primary.dark}`,
        }}
      >
        <div
          style={{
            position: 'relative',
            transform: 'rotate(45deg) ',
            width: '100%',
            textAlign: 'center',
          }}
        >
          <Typography
            variant="button"
            style={{
              color: 'white',
              fontWeight: 'bold',
              fontSize: '14px',
              letterSpacing: '0px',
            }}
          >
            {rank}
          </Typography>
        </div>
      </div>
    </div>
  )

  const iconNormal = L.divIcon({
    html: iconContentNormal,
    className: '',
    iconAnchor: [12, 30],
  })

  const markerRef = useRef<L.Marker>(null)

  useEffect(() => {
    const marker = markerRef.current
    if (marker) {
      marker.setIcon(iconNormal)
    }
  }, [iconNormal])

  return (
    <Marker
      ref={markerRef}
      position={[position[1], position[0]]}
      icon={iconNormal}
    />
  )
}

export default MarkerForReportMap
