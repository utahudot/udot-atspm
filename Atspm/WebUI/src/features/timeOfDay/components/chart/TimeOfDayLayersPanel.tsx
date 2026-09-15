import { DashedLineSeriesSymbol } from '@/features/charts/utils'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Box, Checkbox, IconButton, Paper, Typography } from '@mui/material'
import { useState } from 'react'
import type {
  TimeOfDayChartLayer,
  TimeOfDayChartLayerGroup,
  TimeOfDayChartPreset,
} from '../../transformers'
import { getTimeOfDayPresetSeriesSelection } from '../../transformers'

export type TimeOfDayAnalysisMode = Exclude<TimeOfDayChartPreset, 'combined'>

interface TimeOfDayLayersPanelProps {
  layers: TimeOfDayChartLayer[]
  selectedSeries: Record<string, boolean>
  onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
}

const layerGroupOrder: TimeOfDayChartLayerGroup[] = [
  'Schedules',
  'Corridor Demand',
  'Movement Demand',
  'Locations',
]

export const getLayerAnalysisMode = (
  layer: TimeOfDayChartLayer
): TimeOfDayAnalysisMode | undefined => {
  if (layer.group === 'Schedules') return undefined
  if (layer.group === 'Corridor Demand') return 'recommendation'
  if (layer.group === 'Movement Demand') return 'pressure'

  return layer.id === 'signal-peaks' ? 'recommendation' : 'pressure'
}

export const getAnalysisModeSeriesSelection = (
  layers: TimeOfDayChartLayer[],
  activeModes: TimeOfDayAnalysisMode[],
  currentSelection: Record<string, boolean>
) => {
  if (activeModes.length > 0) {
    const preset: TimeOfDayChartPreset =
      activeModes.length === 2 ? 'combined' : activeModes[0]

    return getTimeOfDayPresetSeriesSelection(layers, preset, currentSelection)
  }

  const nextSelection: Record<string, boolean> = {}
  layers.forEach((layer) => {
    const scheduleVisible =
      layer.group === 'Schedules' &&
      layer.available &&
      layer.seriesNames.every(
        (seriesName) => currentSelection[seriesName] === true
      )

    layer.seriesNames.forEach((seriesName) => {
      nextSelection[seriesName] =
        layer.group === 'Schedules' ? scheduleVisible : false
    })
  })

  return nextSelection
}

export const getSidebarLayers = (
  layers: TimeOfDayChartLayer[],
  activeModes: TimeOfDayAnalysisMode[]
) =>
  layers.filter((layer) => {
    const mode = getLayerAnalysisMode(layer)
    return mode === undefined || activeModes.includes(mode)
  })

const dashedLineSvgPath = DashedLineSeriesSymbol.replace(
  /^path:\/\//,
  ''
).replace(/zm?,\s*/g, 'z ')

function DashedLinePreview({
  color,
  width = 34,
  height = 4,
}: {
  color: string
  width?: number
  height?: number
}) {
  return (
    <Box
      component="svg"
      viewBox="180 880 1660 240"
      preserveAspectRatio="none"
      aria-hidden
      sx={{
        display: 'block',
        width,
        height,
        flexShrink: 0,
        color,
        overflow: 'visible',
      }}
    >
      <path d={dashedLineSvgPath} fill="currentColor" />
    </Box>
  )
}

const hatchBackgroundImage = (color: string) =>
  `repeating-linear-gradient(135deg, transparent 0 4px, ${color} 4px 6px)`

function LayerPreview({ layer }: { layer: TimeOfDayChartLayer }) {
  const previewColors = [layer.color, ...(layer.additionalColors ?? [])]
  const commonLineSx = {
    width: 34,
    borderTop: '3px solid',
    borderColor: layer.color,
  }

  return (
    <Box
      sx={{
        width: 56,
        flexShrink: 0,
        bgcolor: '#eef1f5',
        borderRight: '1px solid',
        borderColor: 'divider',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      {layer.preview === 'schedule' ? (
        <Box sx={{ width: 36, display: 'flex' }}>
          {previewColors.map((color, index) => (
            <Box
              key={`${color}-${index}`}
              sx={{ flex: 1, height: 20, bgcolor: color, opacity: 0.35 }}
            />
          ))}
        </Box>
      ) : layer.preview === 'area' ? (
        <Box
          sx={{
            width: 34,
            height: 24,
            bgcolor: layer.color,
            opacity: 0.18,
          }}
        />
      ) : layer.preview === 'hatch' ? (
        <Box
          sx={{
            width: 34,
            height: 24,
            border: '1px dashed',
            borderColor: layer.color,
            bgcolor: 'grey.100',
            backgroundImage: hatchBackgroundImage(layer.color),
          }}
        />
      ) : layer.preview === 'star' ? (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {previewColors.map((color, index) => (
            <Box
              key={`${color}-${index}`}
              sx={{
                width: previewColors.length > 1 ? 16 : 18,
                height: previewColors.length > 1 ? 16 : 18,
                ml: index === 0 ? 0 : '-6px',
                bgcolor: color,
                opacity: 0.82,
                zIndex: index + 1,
                clipPath:
                  'polygon(50% 0%, 61% 35%, 98% 35%, 68% 57%, 79% 94%, 50% 72%, 21% 94%, 32% 57%, 2% 35%, 39% 35%)',
              }}
            />
          ))}
        </Box>
      ) : (layer.preview === 'circle' || layer.preview === 'square') &&
        (previewColors.length > 1 || layer.previewLabel) ? (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          {previewColors.map((color, index) => (
            <Box
              key={`${color}-${index}`}
              sx={{
                width: 16,
                height: 16,
                ml: index === 0 ? 0 : '-6px',
                borderRadius: layer.preview === 'circle' ? '50%' : '2px',
                bgcolor: color,
                color: 'common.white',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontSize: 9,
                fontWeight: 600,
                lineHeight: 1,
                opacity: 0.82,
                zIndex: index + 1,
                boxShadow: '0 0 0 1px #eef1f5',
              }}
            >
              {index === previewColors.length - 1 ? layer.previewLabel : null}
            </Box>
          ))}
        </Box>
      ) : layer.preview === 'circle' || layer.preview === 'square' ? (
        <Box
          sx={{
            width: 16,
            height: 16,
            borderRadius: layer.preview === 'circle' ? '50%' : '2px',
            bgcolor: layer.color,
            opacity: 0.82,
          }}
        />
      ) : layer.preview === 'dashed-line' && previewColors.length > 1 ? (
        <Box
          sx={{
            width: 34,
            display: 'flex',
            flexDirection: 'column',
            gap: 0.25,
          }}
        >
          {previewColors.map((color, index) => (
            <DashedLinePreview
              key={`${color}-${index}`}
              color={color}
              height={4}
            />
          ))}
        </Box>
      ) : layer.preview === 'dashed-line' ? (
        <DashedLinePreview color={layer.color} />
      ) : (
        <Box
          sx={{
            ...commonLineSx,
            borderTopStyle: 'solid',
          }}
        />
      )}
    </Box>
  )
}

function LegendItemPreview({
  color,
  preview,
}: {
  color: string
  preview: 'area' | 'hatch'
}) {
  return (
    <Box
      aria-hidden
      sx={{
        width: 22,
        height: 14,
        flexShrink: 0,
        border: '1px solid',
        borderColor: preview === 'hatch' ? color : 'divider',
        bgcolor: preview === 'hatch' ? 'grey.100' : color,
        opacity: preview === 'hatch' ? 1 : 0.35,
        backgroundImage:
          preview === 'hatch' ? hatchBackgroundImage(color) : 'none',
      }}
    />
  )
}

export default function TimeOfDayLayersPanel({
  layers,
  selectedSeries,
  onSetSeriesVisibility,
}: TimeOfDayLayersPanelProps) {
  const [collapsedGroups, setCollapsedGroups] = useState<
    Partial<Record<TimeOfDayChartLayerGroup, boolean>>
  >({})
  const [expandedLayerDetails, setExpandedLayerDetails] = useState<
    Record<string, boolean>
  >({})

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
      {layerGroupOrder.map((group) => {
        const groupLayers = layers.filter((layer) => layer.group === group)
        if (!groupLayers.length) return null

        const availableLayers = groupLayers.filter((layer) => layer.available)
        const availableSeriesNames = availableLayers.flatMap(
          (layer) => layer.seriesNames
        )
        const visibleSeriesCount = availableSeriesNames.filter(
          (seriesName) => selectedSeries[seriesName]
        ).length
        const allVisible =
          availableSeriesNames.length > 0 &&
          visibleSeriesCount === availableSeriesNames.length
        const someVisible = visibleSeriesCount > 0 && !allVisible
        const collapsed = collapsedGroups[group] === true

        return (
          <Box key={group}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                pl: 0.75,
                mb: collapsed ? 0 : 0.75,
              }}
            >
              <Typography variant="subtitle2" sx={{ fontSize: '0.85rem' }}>
                {group}
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                {group !== 'Schedules' && (
                  <Checkbox
                    size="small"
                    checked={allVisible}
                    indeterminate={someVisible}
                    disabled={!availableSeriesNames.length}
                    onChange={() =>
                      onSetSeriesVisibility(availableSeriesNames, !allVisible)
                    }
                    inputProps={{ 'aria-label': `Toggle all ${group}` }}
                    sx={{ p: 0.25 }}
                  />
                )}
                <IconButton
                  size="small"
                  onClick={() =>
                    setCollapsedGroups((current) => ({
                      ...current,
                      [group]: !collapsed,
                    }))
                  }
                  aria-label={
                    collapsed ? `Expand ${group}` : `Collapse ${group}`
                  }
                  sx={{ p: 0.2 }}
                >
                  {collapsed ? (
                    <ExpandMoreIcon fontSize="small" />
                  ) : (
                    <ExpandLessIcon fontSize="small" />
                  )}
                </IconButton>
              </Box>
            </Box>

            {!collapsed && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.8 }}>
                {groupLayers.map((layer) => {
                  const visibleCount = layer.seriesNames.filter(
                    (seriesName) => selectedSeries[seriesName]
                  ).length
                  const allLayerSeriesVisible =
                    layer.available &&
                    layer.seriesNames.length > 0 &&
                    visibleCount === layer.seriesNames.length
                  const someLayerSeriesVisible =
                    visibleCount > 0 && !allLayerSeriesVisible
                  const seriesControls = layer.seriesControls ?? []
                  const legendItems = layer.legendItems ?? []
                  const hasDetails =
                    seriesControls.length > 0 || legendItems.length > 0
                  const detailsExpanded =
                    expandedLayerDetails[layer.id] === true

                  return (
                    <Paper
                      key={layer.id}
                      variant="outlined"
                      sx={{
                        display: 'flex',
                        minHeight: 68,
                        overflow: 'hidden',
                        opacity: layer.available
                          ? allLayerSeriesVisible || someLayerSeriesVisible
                            ? 1
                            : 0.62
                          : 0.45,
                        bgcolor:
                          allLayerSeriesVisible || someLayerSeriesVisible
                            ? 'common.white'
                            : 'grey.100',
                      }}
                    >
                      <LayerPreview layer={layer} />
                      <Box sx={{ flex: 1, minWidth: 0, p: 0.9 }}>
                        <Box
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            gap: 0.5,
                          }}
                        >
                          <Typography
                            variant="subtitle2"
                            sx={{ fontSize: '0.8rem' }}
                          >
                            {layer.label}
                          </Typography>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            {hasDetails && (
                              <IconButton
                                size="small"
                                onClick={() =>
                                  setExpandedLayerDetails((current) => ({
                                    ...current,
                                    [layer.id]: !detailsExpanded,
                                  }))
                                }
                                aria-label={`${
                                  detailsExpanded ? 'Hide' : 'Show'
                                } ${layer.label} details`}
                                sx={{ p: 0.15, color: 'text.secondary' }}
                              >
                                {detailsExpanded ? (
                                  <ExpandLessIcon fontSize="small" />
                                ) : (
                                  <ExpandMoreIcon fontSize="small" />
                                )}
                              </IconButton>
                            )}
                            <Checkbox
                              size="small"
                              checked={allLayerSeriesVisible}
                              indeterminate={someLayerSeriesVisible}
                              disabled={!layer.available}
                              onChange={() =>
                                onSetSeriesVisibility(
                                  layer.seriesNames,
                                  !allLayerSeriesVisible
                                )
                              }
                              inputProps={{
                                'aria-label': `Toggle ${layer.label}`,
                              }}
                              sx={{ p: 0.15 }}
                            />
                          </Box>
                        </Box>
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          sx={{ display: 'block', lineHeight: 1.35 }}
                        >
                          {layer.available
                            ? layer.description
                            : 'No data available for this layer.'}
                        </Typography>
                        {hasDetails && detailsExpanded && (
                          <Box
                            sx={{
                              display: 'flex',
                              flexDirection: 'column',
                              gap: 0.35,
                              mt: 0.7,
                              pt: 0.7,
                              borderTop: '1px solid',
                              borderColor: 'divider',
                            }}
                          >
                            {legendItems.map((item) => (
                              <Box
                                key={item.label}
                                sx={{
                                  display: 'grid',
                                  gridTemplateColumns: '24px minmax(0, 1fr)',
                                  alignItems: 'center',
                                  columnGap: 0.6,
                                  minWidth: 0,
                                  minHeight: 22,
                                }}
                              >
                                <LegendItemPreview
                                  color={item.color}
                                  preview={item.preview}
                                />
                                <Typography
                                  variant="caption"
                                  sx={{
                                    minWidth: 0,
                                    color: 'text.secondary',
                                    fontSize: '0.7rem',
                                    lineHeight: 1.2,
                                  }}
                                >
                                  {item.label}
                                </Typography>
                              </Box>
                            ))}
                            {seriesControls.map((control) => {
                              const controlVisible =
                                selectedSeries[control.seriesName] === true

                              return (
                                <Box
                                  key={control.seriesName}
                                  sx={{
                                    display: 'grid',
                                    gridTemplateColumns:
                                      '24px minmax(0, 1fr) auto',
                                    alignItems: 'center',
                                    columnGap: 0.6,
                                    minWidth: 0,
                                    opacity: control.available ? 1 : 0.5,
                                  }}
                                >
                                  <DashedLinePreview
                                    color={control.color}
                                    width={22}
                                  />
                                  <Typography
                                    variant="caption"
                                    sx={{
                                      minWidth: 0,
                                      color: 'text.secondary',
                                      fontSize: '0.7rem',
                                      lineHeight: 1.2,
                                    }}
                                  >
                                    {control.label}
                                  </Typography>
                                  <Checkbox
                                    size="small"
                                    checked={controlVisible}
                                    disabled={!control.available}
                                    onChange={() =>
                                      onSetSeriesVisibility(
                                        [control.seriesName],
                                        !controlVisible
                                      )
                                    }
                                    inputProps={{
                                      'aria-label': `Toggle ${control.label}`,
                                    }}
                                    sx={{ p: 0.1 }}
                                  />
                                </Box>
                              )
                            })}
                          </Box>
                        )}
                      </Box>
                    </Paper>
                  )
                })}
              </Box>
            )}
          </Box>
        )
      })}
    </Box>
  )
}
