import { getRouteColor } from '@/features/speedManagementTool/components/SM_Map/SM_Legend'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import React, { useEffect, useMemo, useRef, useState } from 'react'
import { Polyline } from 'react-leaflet'

type RoutePolylineProps = {
  route: SpeedManagementRoute
  selectedRouteIds: string[]
  setSelectedRouteId: (routeId: string) => void
  zoomLevel: number
  setHoveredSegment: (route: SpeedManagementRoute | null) => void
}

export const getPolylineWeight = (zoom: number) => {
  if (zoom >= 18) return 10
  if (zoom >= 15) return 5
  if (zoom >= 12) return 4
  if (zoom >= 10) return 3
  if (zoom >= 8) return 2
  return 2
}

const getBorderWeight = (baseWeight: number, zoom: number) => {
  if (zoom >= 18) return baseWeight + 3
  if (zoom >= 13) return baseWeight + 2
  return baseWeight + 1
}

export const getColor = (
  route: SpeedManagementRoute,
  routeRenderOption: RouteRenderOption,
  mediumMin: number,
  mediumMax: number
) => {
  let field
  switch (routeRenderOption) {
    case RouteRenderOption.Violations:
      field = 'violations'
      break
    case RouteRenderOption.Posted_Speed:
      field = 'Speed_Limit'
      break
    case RouteRenderOption.Average_Speed:
      field = 'averageSpeed'
      break
    case RouteRenderOption.Percentile_85th:
      field = 'averageEightyFifthSpeed'
      break
    default:
      field = 'averageSpeed'
      break
  }

  const val = route.properties[
    field as keyof SpeedManagementRoute['properties']
  ] as number

  if (routeRenderOption === RouteRenderOption.Violations) {
    if (val <= mediumMin) return ViolationColors.Low
    if (val < mediumMax) return ViolationColors.Medium
    return ViolationColors.High
  }

  if (val === null) return '#000'

  return getRouteColor(val)
}

const RoutePolyline: React.FC<RoutePolylineProps> = ({
  route,
  selectedRouteIds,
  setSelectedRouteId,
  zoomLevel,
  setHoveredSegment,
}) => {
  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()
  const [isHovered, setIsHovered] = useState(false)
  const borderRef = useRef<L.Polyline>(null)
  const mainRef = useRef<L.Polyline>(null)

  const isSegmentInteractive = useMemo(
    () => routeRenderOption !== RouteRenderOption.Posted_Speed,
    [routeRenderOption]
  )

  const polylineColor = useMemo(
    () =>
      isHovered || selectedRouteIds.includes(route.properties.route_id)
        ? 'blue'
        : getColor(route, routeRenderOption, mediumMin, mediumMax),
    [
      isHovered,
      selectedRouteIds,
      route,
      routeRenderOption,
      mediumMin,
      mediumMax,
    ]
  )

  const baseWeight = getPolylineWeight(zoomLevel)
  const hoverWeight = baseWeight + 3
  const borderWeight = getBorderWeight(
    isHovered ? hoverWeight : baseWeight,
    zoomLevel
  )

  useEffect(() => {
    if (borderRef.current && mainRef.current) {
      if (isHovered || selectedRouteIds.includes(route.properties.route_id)) {
        borderRef.current.bringToFront()
        mainRef.current.bringToFront()
      }
    }
  }, [isHovered, selectedRouteIds, route.properties.route_id])

  return (
    <>
      {/* Border Polyline (drawn first, thicker with rounded ends) */}

      <Polyline
        ref={mainRef}
        pathOptions={{
          color: polylineColor,
          weight: isHovered ? hoverWeight : baseWeight,
          lineCap: 'round',
          opacity: 1,
        }}
        smoothFactor={0}
        positions={route.geometry.coordinates}
        interactive={isSegmentInteractive}
        eventHandlers={{
          click: () => setSelectedRouteId(route.properties.route_id),
          mouseover: () => {
            setIsHovered(true)
            setHoveredSegment(route)
          },
          mouseout: () => {
            setIsHovered(false)
            setHoveredSegment(null)
          },
        }}
      />
    </>
  )
}

export default RoutePolyline
