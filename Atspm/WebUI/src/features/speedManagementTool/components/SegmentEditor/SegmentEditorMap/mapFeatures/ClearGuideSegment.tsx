import { ROUTE_COLORS } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/colors'
import { Polyline } from 'react-leaflet'
import { useSegmentEditorStore } from '../../segmentEditorStore'

interface ClearGuideSegmentProps {
  entity: {
    id: string
    coordinates: number[][]
    properties?: { name: string; speedLimit: number }
  }
  associatedEntityIds: string[]
  setAssociatedEntityIds: (ids: string[]) => void
  setHoveredEntity: (entity: ClearGuideSegmentProps['entity'] | null) => void
}

const getPolylineWeight = (zoom = 15) => {
  if (zoom >= 18) return 3
  if (zoom >= 15) return 4
  if (zoom >= 12) return 5
  if (zoom >= 10) return 6
  return 6
}

export default function ClearGuideSegment({
  entity,
  associatedEntityIds,
  setHoveredEntity,
}: ClearGuideSegmentProps) {
  const showBase = useSegmentEditorStore((s) => s.legendVisibility.clearguide)
  const showSelected = useSegmentEditorStore(
    (s) => s.legendVisibility['clearguide-selected']
  )
  const isSelected = associatedEntityIds.includes(entity.id)

  if ((isSelected && !showSelected) || (!isSelected && !showBase)) {
    return null
  }

  return (
    <Polyline
      positions={entity.coordinates.map<[number, number]>(([lng, lat]) => [
        lat,
        lng,
      ])}
      weight={getPolylineWeight()}
      lineCap="square"
      lineJoin="round"
      pathOptions={{
        color: isSelected
          ? ROUTE_COLORS.ClearGuide.hover
          : ROUTE_COLORS.ClearGuide.main,
      }}
      eventHandlers={{
        mouseover: (e) => {
          setHoveredEntity(entity)
          e.target.bringToFront()
          e.target.setStyle({ color: ROUTE_COLORS.ClearGuide.hover })
        },
        mouseout: (e) => {
          setHoveredEntity(null)
          e.target.setStyle({
            color: isSelected
              ? ROUTE_COLORS.ClearGuide.hover
              : ROUTE_COLORS.ClearGuide.main,
          })
        },
      }}
    />
  )
}
