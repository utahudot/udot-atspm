import { PersistedMapLayer, ServiceType } from '@/features/mapLayers/types'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Box, Collapse, IconButton, Typography } from '@mui/material'
import L from 'leaflet'
import { useEffect, useRef, useState } from 'react'

type LegendEntry = { title?: string; imgUrl: string; label?: string }
type ArcGisLegendItem = {
  label?: string
  imageData?: string
  contentType?: string
}
type ArcGisLegendLayer = {
  layerName?: string
  legend?: ArcGisLegendItem[]
}

function wmsLegendUrl(
  base: string,
  layerName: string,
  style?: string | null,
  version = '1.3.0'
) {
  const url = new URL(base)
  url.searchParams.set('service', 'WMS')
  url.searchParams.set('version', version)
  url.searchParams.set('request', 'GetLegendGraphic')
  url.searchParams.set('format', 'image/png')
  url.searchParams.set('layer', layerName)
  if (style) url.searchParams.set('style', style)
  return url.toString()
}

function guessWorkspaceWmsEndpoint(base: string, resourceId?: string | null) {
  if (!resourceId || !resourceId.includes(':')) return base
  const [workspace] = resourceId.split(':')
  try {
    const url = new URL(base)
    const parts = url.pathname.replace(/\/+$/, '').split('/')
    parts[parts.length - 1] = `${workspace}/wms`
    url.pathname = parts.join('/')
    return url.toString()
  } catch {
    return base
  }
}

async function fetchArcgisLegend(serviceUrl: string): Promise<LegendEntry[]> {
  const url = `${serviceUrl.replace(/\/+$/, '')}/legend?f=pjson`
  const res = await fetch(url)
  if (!res.ok) throw new Error(`Legend ${res.status}`)
  const json = (await res.json()) as { layers?: ArcGisLegendLayer[] }
  const entries: LegendEntry[] = []

  for (const layer of json.layers ?? []) {
    for (const item of layer.legend ?? []) {
      const mime = item.contentType || 'image/png'
      if (item.imageData) {
        entries.push({
          title: layer.layerName,
          label: item.label,
          imgUrl: `data:${mime};base64,${item.imageData}`,
        })
      }
    }
  }

  return entries
}

export const MapLayersLegends = ({
  activeMapLayers,
}: {
  activeMapLayers: PersistedMapLayer[]
}) => {
  const rootRef = useRef<HTMLDivElement | null>(null)
  const [legends, setLegends] = useState<Record<number, LegendEntry[]>>({})
  const [isMinimized, setIsMinimized] = useState(false)

  useEffect(() => {
    if (!rootRef.current) return
    L.DomEvent.disableClickPropagation(rootRef.current)
    L.DomEvent.disableScrollPropagation(rootRef.current)
  }, [])

  useEffect(() => {
    let cancelled = false

    async function buildLegends() {
      const newLegends: Record<number, LegendEntry[]> = {}

      for (const layer of activeMapLayers) {
        if (
          layer.serviceType === ServiceType.MapServer ||
          layer.serviceType === ServiceType.FeatureServer
        ) {
          try {
            const entries = await fetchArcgisLegend(
              layer.serviceType === ServiceType.FeatureServer
                ? /\/FeatureServer\/\d+$/.test(layer.mapLayerUrl ?? '')
                  ? (layer.mapLayerUrl ?? '')
                  : `${(layer.mapLayerUrl ?? '').replace(/\/?$/, '/')}${
                      layer.resourceId ?? '0'
                    }`
                : (layer.mapLayerUrl ?? '')
            )
            if (!cancelled) newLegends[layer.id] = entries
          } catch {
            // Ignore legend fetch errors; the layer itself can still render.
          }
        }

        if (layer.serviceType === ServiceType.WMS && layer.resourceId) {
          try {
            const base = guessWorkspaceWmsEndpoint(
              layer.mapLayerUrl ?? '',
              layer.resourceId
            )
            const url = wmsLegendUrl(base, layer.resourceId, layer.style)
            newLegends[layer.id] = [{ imgUrl: url }]
          } catch {
            // Ignore malformed legend URLs.
          }
        }
      }

      if (!cancelled) setLegends(newLegends)
    }

    buildLegends()
    return () => {
      cancelled = true
    }
  }, [activeMapLayers])

  if (!activeMapLayers.length) return null

  return (
    <Box
      ref={rootRef}
      sx={{
        position: 'absolute',
        right: 10,
        bottom: 20,
        zIndex: 1000,
        maxWidth: 320,
        bgcolor: 'background.paper',
        borderRadius: 1,
        boxShadow: 3,
        width: '200px',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          p: 0.25,
          pl: 1,
        }}
      >
        <Typography variant="h6">Legend</Typography>
        <IconButton
          onClick={() => setIsMinimized((prev) => !prev)}
          size="small"
          aria-label="minimize"
        >
          {isMinimized ? (
            <ExpandMoreIcon fontSize="small" />
          ) : (
            <ExpandLessIcon fontSize="small" />
          )}
        </IconButton>
      </Box>
      <Collapse in={!isMinimized} timeout="auto" unmountOnExit>
        <Box sx={{ maxHeight: '200px', overflowY: 'auto', pl: 1 }}>
          {activeMapLayers.map((layer) => (
            <Box key={layer.id} sx={{ mb: 1.5 }}>
              <Box sx={{ fontWeight: 600, fontSize: 13, mb: 0.5 }}>
                {layer.name}
              </Box>
              {(legends[layer.id] ?? []).map((entry, index) => (
                <Box
                  key={`${entry.imgUrl}-${index}`}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    mb: 0.5,
                  }}
                >
                  <Box
                    component="img"
                    src={entry.imgUrl}
                    alt={entry.label || entry.title || 'legend'}
                    width={24}
                    height={24}
                  />
                  <Box sx={{ fontSize: 12, color: 'text.secondary' }}>
                    {entry.label || entry.title || ''}
                  </Box>
                </Box>
              ))}
            </Box>
          ))}
        </Box>
      </Collapse>
    </Box>
  )
}

export default MapLayersLegends
