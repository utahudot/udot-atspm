import { Segment } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { ROUTE_COLORS } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/colors'
import { Fragment, memo } from 'react'
import { Polyline } from 'react-leaflet'

interface SegmentPolylinesProps {
  segments: Segment[]
  selectedSegmentIds: string[]
  onSegmentSelect: (segment: Segment) => void
  zoomLevel: number
  setHoveredSegment: (segment: Segment | null) => void
}

const getPolylineWeight = (zoom: number) => {
  if (zoom >= 18) return 5
  if (zoom >= 15) return 3
  if (zoom >= 12) return 2
  if (zoom >= 10) return 1
  return 2
}

const SegmentPolylines = memo<SegmentPolylinesProps>(
  ({
    segments,
    onSegmentSelect,
    zoomLevel,
    setHoveredSegment,
    selectedSegmentIds,
  }) => {
    const calculateWeight = (selected: boolean) => {
      return getPolylineWeight(zoomLevel) + (selected ? 1 : 0)
    }

    return (
      <>
        {segments.map((segment, index) => {
          if (!segment.geometry?.coordinates) return null
          return (
            <Fragment key={`segment-${segment.id}-${index}`}>
              <Polyline
                key={`segment-${segment.id}-main${segment.properties.udotRouteNumber}`}
                pathOptions={{
                  color: selectedSegmentIds?.includes(segment.id)
                    ? ROUTE_COLORS.Draft.hover
                    : ROUTE_COLORS.Draft.main,
                  weight: calculateWeight(
                    selectedSegmentIds?.includes(segment.id)
                  ),
                  lineCap: 'round',
                  opacity: 1,
                }}
                smoothFactor={0}
                positions={segment.geometry.coordinates}
                eventHandlers={{
                  click: () => onSegmentSelect(segment),
                  mouseover: (e) => {
                    setHoveredSegment(segment)
                    e.target.bringToFront()
                    e.target.setStyle({
                      weight: calculateWeight(true) + 2,
                      color: ROUTE_COLORS.Draft.hover,
                    })
                  },
                  mouseout: (e) => {
                    setHoveredSegment(null)
                    e.target.setStyle({
                      weight: calculateWeight(
                        selectedSegmentIds?.includes(segment.id)
                      ),
                      color: selectedSegmentIds?.includes(segment.id)
                        ? ROUTE_COLORS.Draft.hover
                        : ROUTE_COLORS.Draft.main,
                    })
                  },
                }}
              />
            </Fragment>
          )
        })}
      </>
    )
  }
)

SegmentPolylines.displayName = 'SegmentPolylines'

export default SegmentPolylines
