import { Box, Typography } from '@mui/material'
import { point } from '@turf/helpers'
import { nearestPointOnLine } from '@turf/turf'
import type { LatLngExpression } from 'leaflet'
import React from 'react'
import { Polyline, Popup, useMapEvent } from 'react-leaflet'

interface Entity {
  id: string
  entityId: string
  name?: string | null
  direction?: string | null
  entityType: string
  startDate: string
  coordinates: number[][]
}

interface ClearGuideSpiderLayerProps {
  entities: Entity[]
  associatedEntityIds: string[]
  setAssociatedEntityIds: (ids: string[]) => void
  setHoveredEntity: (entity: Entity | null) => void
}

export function ClearGuideSpiderLayer({
  entities,
  associatedEntityIds,
  setAssociatedEntityIds,
  setHoveredEntity,
}: ClearGuideSpiderLayerProps) {
  const [spiderLines, setSpiderLines] = React.useState<LatLngExpression[][]>([])
  const [popupInfo, setPopupInfo] = React.useState<{
    pos: LatLngExpression
    segments: Entity[]
  } | null>(null)

  useMapEvent('click', (e) => {
    const pt = point([e.latlng.lng, e.latlng.lat])
    const nearby = entities.filter((ent) => {
      const line = {
        type: 'Feature',
        geometry: { type: 'LineString', coordinates: ent.coordinates },
      }
      const nearest = nearestPointOnLine(line as any, pt, { units: 'meters' })
      return (nearest.properties as any).dist < 20
    })
    if (!nearby.length) return

    if (nearby.length === 1) {
      const seg = nearby[0]
      const next = associatedEntityIds.includes(seg.id)
        ? associatedEntityIds.filter((x) => x !== seg.id)
        : [...associatedEntityIds, seg.id]
      setAssociatedEntityIds(next)
      return
    }

    const legs = nearby.map((ent) => {
      const turfLine = {
        type: 'Feature',
        geometry: { type: 'LineString', coordinates: ent.coordinates },
      }
      const nearest = nearestPointOnLine(turfLine as any, pt, {
        units: 'meters',
      })
      const [lng, lat] = nearest.geometry.coordinates as [number, number]
      return [
        [e.latlng.lat, e.latlng.lng] as LatLngExpression,
        [lat, lng] as LatLngExpression,
      ]
    })

    setSpiderLines(legs)
    setPopupInfo({ pos: [e.latlng.lat, e.latlng.lng], segments: nearby })
  })

  return (
    <>
      {spiderLines.map((coords, i) => (
        <Polyline
          key={i}
          positions={coords}
          pathOptions={{ color: 'green', weight: 2 }}
          eventHandlers={{ click: (e) => e.originalEvent.preventDefault() }}
        />
      ))}

      {popupInfo && (
        <Popup position={popupInfo.pos} closeOnClick={false} autoClose={false}>
          <Box
            component="ul"
            sx={{ width: 280, maxWidth: '100%', m: 0, p: 0, listStyle: 'none' }}
          >
            {popupInfo.segments.map((seg, idx) => {
              const isSel = associatedEntityIds.includes(seg.id)
              const label = [
                seg.name != null ? `${seg.name} – ` : '',
                seg.entityId,
                seg.direction ? ` (${seg.direction})` : '',
              ].join('')
              const subtitle = `${seg.entityType} • ${seg.startDate}`

              return (
                <React.Fragment key={seg.id}>
                  {idx > 0 && (
                    <Box
                      component="li"
                      sx={{
                        borderTop: '1px solid rgba(0,0,0,0.12)',
                        m: 0,
                        p: 0,
                      }}
                    />
                  )}
                  <Box
                    component="li"
                    onClick={() => {
                      const next = isSel
                        ? associatedEntityIds.filter((x) => x !== seg.id)
                        : [...associatedEntityIds, seg.id]
                      setAssociatedEntityIds(next)
                    }}
                    sx={{
                      display: 'flex',
                      alignItems: 'flex-start',
                      cursor: 'pointer',
                      px: 1,
                      py: 0.5,
                      '&:hover': { backgroundColor: 'rgba(0,0,0,0.05)' },
                    }}
                  >
                    <Box
                      sx={{
                        mt: 0.25,
                        width: 12,
                        height: 12,
                        borderRadius: '50%',
                        border: '2px solid green',
                        backgroundColor: isSel ? 'green' : 'transparent',
                        flexShrink: 0,
                        mr: 1,
                      }}
                    />

                    <Box>
                      <Typography
                        variant="body2"
                        component="div"
                        sx={{ lineHeight: 1 }}
                      >
                        {label}
                      </Typography>
                      <Typography
                        variant="caption"
                        component="div"
                        color="text.secondary"
                        sx={{ lineHeight: 2 }}
                      >
                        {subtitle}
                      </Typography>
                    </Box>
                  </Box>
                </React.Fragment>
              )
            })}
          </Box>
        </Popup>
      )}
    </>
  )
}
