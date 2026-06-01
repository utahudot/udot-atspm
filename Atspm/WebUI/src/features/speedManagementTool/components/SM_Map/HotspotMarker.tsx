import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { darken, useTheme } from '@mui/material/styles'
import Typography from '@mui/material/Typography'
import L from 'leaflet'
import { useEffect, useMemo, useRef } from 'react'
import ReactDOMServer from 'react-dom/server'
import { Marker, useMap } from 'react-leaflet'

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
  const map = useMap()
  const { hoveredHotspot } = useSpeedManagementStore()
  const isHovered = hoveredHotspot === segmentId

  // For the arrow marker, we calculate a display position if the actual pin is off-screen.
  let arrowNeeded = false
  let arrowDirection = 0
  let arrowDisplayPosition: [number, number] = position

  if (map && isHovered) {
    const bounds = map.getBounds()
    const actualLatLng = L.latLng(position)

    if (!bounds.contains(actualLatLng)) {
      const center = map.getCenter()
      const dx = position[1] - center.lng
      const dy = position[0] - center.lat

      const north = bounds.getNorth()
      const south = bounds.getSouth()
      const east = bounds.getEast()
      const west = bounds.getWest()

      let tX = Infinity
      let tY = Infinity

      if (dx > 0) {
        tX = (east - center.lng) / dx
      } else if (dx < 0) {
        tX = (west - center.lng) / dx
      }
      if (dy > 0) {
        tY = (north - center.lat) / dy
      } else if (dy < 0) {
        tY = (south - center.lat) / dy
      }
      const t = Math.min(tX, tY)
      const bufferFactor = 0.9
      arrowDisplayPosition = [
        center.lat + dy * t * bufferFactor,
        center.lng + dx * t * bufferFactor,
      ]
      arrowNeeded = true
      // Calculate arrow rotation so that it points toward the actual pin.
      arrowDirection = -(Math.atan2(dy, dx) * 180) / Math.PI
    }
  }

  // pin icon.
  const pinIconHtml = useMemo(
    () =>
      ReactDOMServer.renderToString(
        <div style={{ position: 'relative' }}>
          <div
            style={{
              width: isHovered ? '28px' : '25px',
              height: isHovered ? '28px' : '25px',
              backgroundColor: isHovered
                ? '#f07b29'
                : theme.palette.primary.main,
              borderRadius: '50% 50% 50% 0',
              transform: 'rotate(-45deg)',
              position: 'relative',
              border: isHovered
                ? `1px solid ${darken(theme.palette.primary.dark, 0.2)}`
                : `1px solid ${theme.palette.primary.dark}`,
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
                  fontSize: isHovered ? '13px' : '12px',
                  letterSpacing: '0px',
                }}
              >
                {rank}
              </Typography>
            </div>
          </div>
        </div>
      ),
    [isHovered, rank, theme]
  )

  const pinIcon = L.divIcon({
    html: pinIconHtml,
    className: '',
    iconAnchor: isHovered ? [14, 33] : [12, 30],
  })

  // arrow icon.
  const arrowIconHtml = useMemo(() => {
    if (!arrowNeeded) return ''
    return ReactDOMServer.renderToString(
      <div style={{ width: '45px', height: '45px' }}>
        <div
          style={{
            width: '100%',
            height: '100%',
            transform: `rotate(${arrowDirection}deg)`,
          }}
        >
          <svg
            width="100%"
            height="100%"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M4.49746 20.835L21.0072 13.4725C22.3309 12.8822 22.3309 11.1178 21.0072 10.5275L4.49746 3.16496C3.00163 2.49789 1.45006 3.97914 2.19099 5.36689L5.34302 11.2706C5.58817 11.7298 5.58818 12.2702 5.34302 12.7294L2.19099 18.6331C1.45007 20.0209 3.00163 21.5021 4.49746 20.835Z"
              fill="#f07b29"
              style={{ borderRadius: '4px', border: '1px solid black' }}
            />
          </svg>
          <div
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              height: '100%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              transform: `rotate(${-arrowDirection}deg)`,
            }}
          >
            <Typography
              variant="button"
              style={{
                color: 'white',
                fontWeight: 'bold',
                fontSize: '13px',
              }}
            >
              {rank}
            </Typography>
          </div>
        </div>
      </div>
    )
  }, [arrowNeeded, arrowDirection, rank])

  const arrowIcon = L.divIcon({
    html: arrowIconHtml,
    className: '',
    iconAnchor: [22, 22],
  })

  // Refs for the markers so we can update their icons or z-index.
  const pinMarkerRef = useRef<L.Marker>(null)
  const arrowMarkerRef = useRef<L.Marker>(null)

  useEffect(() => {
    if (pinMarkerRef.current) {
      pinMarkerRef.current.setIcon(pinIcon)
      // Bring the pin marker to the front when hovered.
      pinMarkerRef.current.setZIndexOffset(isHovered ? 1000 : 0)
    }
  }, [pinIcon, isHovered])

  useEffect(() => {
    if (arrowNeeded && arrowMarkerRef.current) {
      arrowMarkerRef.current.setIcon(arrowIcon)
      arrowMarkerRef.current.setZIndexOffset(999)
    }
  }, [arrowIcon, arrowNeeded])

  return (
    <>
      {/* Pin Marker */}
      <Marker
        ref={pinMarkerRef}
        position={position}
        icon={pinIcon}
        eventHandlers={{
          click: () => onClick(segmentId),
        }}
      />
      {/* Render arrow marker */}
      {isHovered && arrowNeeded && (
        <Marker
          ref={arrowMarkerRef}
          position={arrowDisplayPosition}
          icon={arrowIcon}
          interactive={false}
        />
      )}
    </>
  )
}

export default RankMarker
