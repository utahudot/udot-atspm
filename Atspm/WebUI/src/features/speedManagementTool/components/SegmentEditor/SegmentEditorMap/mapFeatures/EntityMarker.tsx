import { DataSource } from '@/features/speedManagementTool/enums'
import { Icon, IconOptions, PointExpression } from 'leaflet'
import { Marker } from 'react-leaflet'
import { useSegmentEditorStore } from '../../segmentEditorStore'

const sourceIconDefaults: Partial<IconOptions> = {
  iconSize: [22, 35] as PointExpression,
  iconAnchor: [13, 40] as PointExpression,
  shadowSize: [40, 40] as PointExpression,
  shadowAnchor: [40, 62] as PointExpression,
}

export const pemsIcon = new Icon({
  ...sourceIconDefaults,
  iconUrl: '/images/speed-management/pems-source.svg',
})
export const pemsSelectedIcon = new Icon({
  ...sourceIconDefaults,
  iconUrl: '/images/speed-management/pems-source-selected.svg',
})
export const atspmIcon = new Icon({
  ...sourceIconDefaults,
  iconUrl: '/images/speed-management/atspm-source.svg',
})
export const atspmSelectedIcon = new Icon({
  ...sourceIconDefaults,
  iconUrl: '/images/speed-management/atspm-source-selected.svg',
})

const ICON_PATHS = [
  '/images/speed-management/pems-source.svg',
  '/images/speed-management/pems-source-selected.svg',
  '/images/speed-management/atspm-source.svg',
  '/images/speed-management/atspm-source-selected.svg',
]
ICON_PATHS.forEach((src) => {
  const img = new Image()
  img.src = src
})

interface EntityMarkerProps {
  entity: {
    id: string
    sourceId: DataSource
    coordinates: number[][]
    entityId: string
    version: string
    entityType: string
    direction: string
    startDate: string
    active?: boolean
  }
  isSelected: boolean
  associatedEntityIds: string[]
  setAssociatedEntityIds: (ids: string[]) => void
  setHoveredEntity: (entity: EntityMarkerProps['entity'] | null) => void
}

const EntityMarker = ({
  entity,
  isSelected,
  associatedEntityIds,
  setAssociatedEntityIds,
  setHoveredEntity,
}: EntityMarkerProps) => {
  const vis = useSegmentEditorStore((s) => s.legendVisibility)

  if (entity.sourceId === DataSource.PeMS) {
    if (isSelected && !vis['pems-selected']) return null
    if (!isSelected && !vis.pems) return null
  }
  if (entity.sourceId === DataSource.ATSPM) {
    if (isSelected && !vis['atspm-selected']) return null
    if (!isSelected && !vis.atspm) return null
  }

  const handleEntityClick = () => {
    const updated = isSelected
      ? associatedEntityIds.filter((id) => id !== entity.id)
      : [...associatedEntityIds, entity.id]
    setAssociatedEntityIds(updated)
  }

  const iconToUse =
    entity.sourceId === DataSource.PeMS
      ? isSelected
        ? pemsSelectedIcon
        : pemsIcon
      : isSelected
        ? atspmSelectedIcon
        : atspmIcon

  return (
    <Marker
      // attach the raw entity so clusters can read it
      {...{ entity }}
      position={[entity.coordinates[0][1], entity.coordinates[0][0]]}
      icon={iconToUse}
      zIndexOffset={1000}
      eventHandlers={{
        click: () => handleEntityClick(),
        mouseover: () => setHoveredEntity(entity),
        mouseout: () => setHoveredEntity(null),
      }}
    />
  )
}

export default EntityMarker
