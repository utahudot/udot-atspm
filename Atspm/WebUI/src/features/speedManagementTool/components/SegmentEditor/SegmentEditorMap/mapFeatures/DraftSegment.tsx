import { markerIcon } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapIcons'
import { useCallback } from 'react'
import { Marker, Pane, Polyline } from 'react-leaflet'
import { useSegmentEditorStore } from '../../segmentEditorStore'

interface DraftSegmentProps {
  polylineCoordinates: number[][]
  mapRef: any
  removePolylineCoordinate: (index: number) => void
  updatePolylineCoordinate: (
    index: number,
    newPosition: [number, number]
  ) => void
}

const DraftSegment = ({
  polylineCoordinates,
  mapRef,
  removePolylineCoordinate,
  updatePolylineCoordinate,
}: DraftSegmentProps) => {
  const activeTab = useSegmentEditorStore((s) => s.activeTab)
  const show = useSegmentEditorStore((s) => s.legendVisibility.draft)

  const handleMarkerClick = useCallback(
    (index: number) => {
      removePolylineCoordinate(index)
    },
    [removePolylineCoordinate]
  )

  const handleMarkerDrag = useCallback(
    (index: number, newPosition: [number, number]) => {
      if (activeTab === 0) {
        updatePolylineCoordinate(index, newPosition)
      }
    },
    [updatePolylineCoordinate]
  )

  // nothing to render if legend says “hide”
  if (!show) return null

  const positions = polylineCoordinates.map(
    (coord) => [coord[1], coord[0]] as [number, number]
  )

  return (
    <>
      <Pane name="draft-line" style={{ zIndex: 6250 }}>
        {positions.length > 0 && (
          <Polyline
            positions={positions}
            weight={3}
            interactive
            lineCap="square"
            lineJoin="round"
          />
        )}
      </Pane>

      <Pane name="draft-marker" style={{ zIndex: 6500 }}>
        {positions.map((position, index) => (
          <Marker
            key={`marker-${index}-${activeTab}`}
            position={position}
            icon={markerIcon}
            draggable={activeTab === 0}
            eventHandlers={{
              click: (e) => {
                e.originalEvent.stopPropagation()
                handleMarkerClick(index)
              },
              dragstart: () => {
                mapRef?.dragging.disable()
              },
              dragend: (e) => {
                const { lat, lng } = e.target.getLatLng()
                handleMarkerDrag(index, [lng, lat])
                mapRef?.dragging.enable()
              },
            }}
          />
        ))}
      </Pane>
    </>
  )
}

export default DraftSegment
