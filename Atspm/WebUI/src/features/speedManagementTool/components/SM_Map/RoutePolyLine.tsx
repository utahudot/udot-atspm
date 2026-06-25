import {
  NO_DATA_ROUTE_COLOR,
  NO_DATA_ROUTE_DASH_ARRAY,
  NO_DATA_ROUTE_OPACITY,
  getNoDataRouteWeight,
  getRouteColor,
  routeHasData,
  routeHasNoData,
} from '@/features/speedManagementTool/components/SM_Map/SM_Legend'
import {
  RouteRenderOption,
  isViolationRenderOption,
} from '@/features/speedManagementTool/enums'
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
  if (!routeHasData(route.properties)) return NO_DATA_ROUTE_COLOR

  let field
  switch (routeRenderOption) {
    case RouteRenderOption.Violations:
      field = 'violations'
      break
    case RouteRenderOption.Percent_Violations:
      field = 'percentViolations'
      break
    case RouteRenderOption.Percent_Extreme_Violations:
      field = 'percentExtremeViolations'
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
  ] as number | null | undefined

  if (isViolationRenderOption(routeRenderOption)) {
    if (val === null || val === undefined) return NO_DATA_ROUTE_COLOR
    if (val <= mediumMin) return ViolationColors.Low
    if (val < mediumMax) return ViolationColors.Medium
    return ViolationColors.High
  }

  if (val === null || val === undefined) return NO_DATA_ROUTE_COLOR

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

  const isSelected = selectedRouteIds.includes(route.properties.route_id)
  const isNoData = routeHasNoData(route.properties)
  const isEmphasized = isHovered || isSelected

  const polylineColor = useMemo(
    () =>
      isEmphasized
        ? 'blue'
        : getColor(route, routeRenderOption, mediumMin, mediumMax),
    [
      isEmphasized,
      route,
      routeRenderOption,
      mediumMin,
      mediumMax,
    ]
  )

  const baseWeight = getPolylineWeight(zoomLevel)
  const hoverWeight = baseWeight + 3
  const routeWeight =
    isNoData && !isEmphasized ? getNoDataRouteWeight(baseWeight) : baseWeight
  const routeOpacity = isNoData && !isEmphasized ? NO_DATA_ROUTE_OPACITY : 1
  const borderWeight = getBorderWeight(
    isHovered ? hoverWeight : baseWeight,
    zoomLevel
  )

  useEffect(() => {
    if (borderRef.current && mainRef.current) {
      if (isEmphasized) {
        borderRef.current.bringToFront()
        mainRef.current.bringToFront()
      }
    }
  }, [isEmphasized])

  return (
    <>
      {/* Border Polyline (drawn first, thicker with rounded ends) */}

      <Polyline
        ref={mainRef}
        pathOptions={{
          color: polylineColor,
          weight: isHovered ? hoverWeight : routeWeight,
          lineCap: 'round',
          opacity: routeOpacity,
          dashArray: isNoData ? NO_DATA_ROUTE_DASH_ARRAY : undefined,
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
