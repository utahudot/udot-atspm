import { UdotRoute } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/types'
import { ROUTE_COLORS } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/colors'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { useMemo } from 'react'
import { Polyline } from 'react-leaflet'

interface UdotLrsSegmentsProps {
  segments: UdotRoute[]
  segmentRouteId: string | null
}

export const UdotLrsSegments = ({
  segments,
  segmentRouteId,
}: UdotLrsSegmentsProps) => {
  const show = useSegmentEditorStore((s) => s.legendVisibility.lrs)

  const polylines = useMemo(() => {
    if (!show) return null
    if (!Array.isArray(segments) || segments.length === 0) return null

    return segments.map((segment) => {
      const coords = segment.geometry.paths[0].map((point) => [
        point[1],
        point[0],
      ]) as [number, number][]
      const selected = segment.attributes.ROUTE_ID === segmentRouteId

      return (
        <Polyline
          key={`udot-route-${segment.attributes.ROUTE_ID}-${
            selected ? 'selected' : 'default'
          }`}
          positions={coords}
          weight={selected ? 2 : 1}
          color={selected ? ROUTE_COLORS.Lrs.hover : ROUTE_COLORS.Lrs.main}
          interactive={false}
        />
      )
    })
  }, [segments, segmentRouteId, show])

  return <>{polylines}</>
}
