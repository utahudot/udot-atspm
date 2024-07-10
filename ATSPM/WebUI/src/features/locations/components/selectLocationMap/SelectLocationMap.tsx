import { Location } from '@/features/locations/types'
import { Skeleton } from '@mui/material'
import dynamic from 'next/dynamic'
import { memo, useEffect, useMemo, useState } from 'react'

type SelectLocationMapProps = {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  route?: number[][]
  zoom?: number
  center?: [number, number]
  mapHeight?: number | string
}

function SelectLocationMap({
  location,
  setLocation,
  locations,
  route,
  zoom,
  center,
  mapHeight,
}: SelectLocationMapProps) {
  const [isInteractive, setIsInteractive] = useState(false);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [showPopup, setShowPopup] = useState(false);
  const Map = useMemo(
    () =>
      dynamic(() => import('@/components/map/Map'), {
        loading: () => (
          <Skeleton variant="rectangular" height={mapHeight ?? 400} />
        ),
        ssr: false,
      }),
    [mapHeight]
  )
  const mapProps = useMemo(
    () => ({
      location,
      setLocation,
      locations,
      route,
      zoom,
      center,
      mapHeight,
      interactive: isInteractive,
    }),
    [location, setLocation, locations, route, zoom, center, mapHeight]
  )

  const handleMouseMove = (event) => {
    setMousePosition({ x: event.clientX, y: event.clientY });
  };

  const handleMouseLeave = () => {
    setShowPopup(false);
    
  };

  const handleActivationClick = () => {
    setIsInteractive(true);
    setShowPopup(false); 
  };

  useEffect(() => {
    if (!isInteractive) {
      const timer = setTimeout(() => {
        setShowPopup(true);
      }, 2000);
      return () => clearTimeout(timer);
    }
  }, [isInteractive]);
  return (
    <div
      style={{ position: 'relative', height: mapHeight }}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
    >
      <Map {...mapProps} />
      {!isInteractive && (
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            cursor: 'pointer',
          }}
          onClick={handleActivationClick}
        />
      )}
      {showPopup && !isInteractive && (
        <div
          style={{
            position: 'absolute',
            left: `${mousePosition.x - 470}px`,
            top: `${mousePosition.y - 350}px`,
            pointerEvents: 'none',
            background: 'rgba(255, 255, 255, 0.6)',
            padding: '8px 12px',
            borderRadius: '5px',
            fontSize: '14px',
            zIndex: 1100,
          }}
        >
          Click to scroll
        </div>
      )}
    </div>
  );
}

export default memo(SelectLocationMap);