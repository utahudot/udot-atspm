import { darken, useTheme } from '@mui/material/styles'
import Typography from '@mui/material/Typography'
import L from 'leaflet'
import { useEffect, useRef } from 'react'
import ReactDOMServer from 'react-dom/server'
import { Marker } from 'react-leaflet'

type RankMarkerProps = {
  position: [number, number]
  rank: number
  segmentId: string
  onClick: (segmentId: string) => void
}

const RankMarker = ({
  position,
  rank,
  segmentId,
  onClick,
}: RankMarkerProps) => {
  const theme = useTheme()
  const hoverColor = darken(theme.palette.primary.main, 0.2)

  // Normal Icon Content
  const iconContentNormal = ReactDOMServer.renderToString(
    <div style={{ position: 'relative' }}>
      <div
        style={{
          width: '25px',
          height: '25px',
          backgroundColor: theme.palette.primary.main,
          borderRadius: '50% 50% 50% 0',
          transform: 'rotate(-45deg)',
          position: 'relative',
          border: `1px solid ${theme.palette.primary.dark}`,
        }}
      >
        <div
          style={{
            position: 'absolute',
            top: '70%',
            left: '10%',
            transform: 'rotate(45deg) translate(-50%, -50%)',
            width: '100%',
            textAlign: 'center',
          }}
        >
          <Typography
            variant="button"
            style={{
              color: 'white',
              fontWeight: 'bold',
              fontSize: '12px',
              letterSpacing: '0px',
            }}
          >
            {rank}
          </Typography>
        </div>
      </div>
    </div>
  )

  // Hover Icon Content
  const iconContentHover = ReactDOMServer.renderToString(
    <div style={{ position: 'relative' }}>
      <div
        style={{
          width: '28px',
          height: '28px',
          backgroundColor: hoverColor,
          borderRadius: '50% 50% 50% 0',
          transform: 'rotate(-45deg)',
          position: 'relative',
          border: `1px solid ${darken(theme.palette.primary.dark, 0.2)}`,
        }}
      >
        <div
          style={{
            position: 'absolute',
            top: '70%',
            left: '10%',
            transform: 'rotate(45deg) translate(-50%, -50%)',
            width: '100%',
            textAlign: 'center',
          }}
        >
          <Typography
            variant="button"
            style={{
              color: 'white',
              fontWeight: 'bold',
              fontSize: '13px',
              letterSpacing: '0px',
            }}
          >
            {rank}
          </Typography>
        </div>
      </div>
    </div>
  )

  // Create Normal Icon
  const iconNormal = L.divIcon({
    html: iconContentNormal,
    className: '',
    iconAnchor: [12, 30], // Adjusted for center alignment
  })

  // Create Hover Icon
  const iconHover = L.divIcon({
    html: iconContentHover,
    className: '',
    iconAnchor: [14, 33], // Adjusted for larger size
  })

  // Reference to the marker instance
  const markerRef = useRef<L.Marker>(null)

  useEffect(() => {
    const marker = markerRef.current
    if (marker) {
      marker.off('mouseover')
      marker.off('mouseout')

      const onMouseOver = () => {
        marker.setIcon(iconHover)
        marker.off('mouseover', onMouseOver)
      }

      const onMouseOut = () => {
        marker.setIcon(iconNormal)
        marker.on('mouseover', onMouseOver)
      }

      marker.on('mouseover', onMouseOver)
      marker.on('mouseout', onMouseOut)

      return () => {
        marker.off('mouseover', onMouseOver)
        marker.off('mouseout', onMouseOut)
      }
    }
  }, [iconNormal, iconHover, segmentId])

  return (
    <Marker
      ref={markerRef}
      position={[position[1], position[0]]}
      icon={iconNormal}
      riseOnHover
      eventHandlers={{
        click: () => onClick(segmentId),
      }}
    />
  )
}

export default RankMarker
