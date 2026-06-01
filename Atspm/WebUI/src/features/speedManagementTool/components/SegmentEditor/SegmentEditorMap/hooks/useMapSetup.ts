import { getEnv } from '@/utils/getEnv'
import L, { latLngBounds, Map as LeafletMap } from 'leaflet'
import { useEffect, useRef, useState } from 'react'

export const useMapSetup = (
  mapRef: LeafletMap | null,
  polylineCoordinates: number[][]
) => {
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  const [zoomLevel, setZoomLevel] = useState(10)
  const [hasZoomedToSegment, setHasZoomedToSegment] = useState(false)
  const isDragging = useRef(false)
  const hasFitBounds = useRef(false)

  useEffect(() => {
    if (hasZoomedToSegment) return
    if (mapRef && polylineCoordinates.length >= 2) {
      // convert [lng,lat] → [lat,lng] and fit bounds
      const latLngs = polylineCoordinates.map(
        ([lng, lat]) => [lat, lng] as [number, number]
      )
      setHasZoomedToSegment(true)
      mapRef.fitBounds(latLngBounds(latLngs))
    }
  }, [mapRef, polylineCoordinates, hasZoomedToSegment])

  // Initialize map with default coordinates
  useEffect(() => {
    const fetchEnv = async () => {
      try {
        const env = await getEnv()
        if (!env?.MAP_DEFAULT_LATITUDE || !env?.MAP_DEFAULT_LONGITUDE) {
          console.error('Missing map default coordinates in environment')
          return
        }
        setInitialLatLong([
          parseFloat(env.MAP_DEFAULT_LATITUDE),
          parseFloat(env.MAP_DEFAULT_LONGITUDE),
        ])
      } catch (error) {
        console.error('Failed to fetch environment variables:', error)
      }
    }
    fetchEnv()
  }, [])

  // Handle map bounds and dragging
  useEffect(() => {
    if (!mapRef) return

    if (polylineCoordinates.length > 0) {
      const bounds = L.latLngBounds(
        polylineCoordinates.map((coord) => [coord[1], coord[0]])
      )

      if (bounds.isValid() && !hasFitBounds.current) {
        mapRef.fitBounds(bounds)
        hasFitBounds.current = true
      }
    } else if (initialLatLong && !hasFitBounds.current) {
      mapRef.setView(initialLatLong, zoomLevel)
      hasFitBounds.current = true
    }

    const mapContainer = mapRef.getContainer()

    const handleResize = () => {
      mapRef.invalidateSize()
    }

    const handleMouseDown = () => {
      isDragging.current = false
    }

    const handleMouseMove = () => {
      isDragging.current = true
    }

    mapRef.on('mousedown', handleMouseDown)
    mapRef.on('mousemove', handleMouseMove)

    const resizeObserver = new ResizeObserver(handleResize)
    resizeObserver.observe(mapContainer)

    return () => {
      resizeObserver.disconnect()
      mapRef.off('mousedown', handleMouseDown)
      mapRef.off('mousemove', handleMouseMove)
    }
  }, [mapRef, polylineCoordinates, initialLatLong])

  return { zoomLevel, setZoomLevel }
}
