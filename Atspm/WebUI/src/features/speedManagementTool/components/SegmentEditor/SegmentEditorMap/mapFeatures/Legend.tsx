import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Box,
  Checkbox,
  IconButton,
  SxProps,
  Theme,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'

export type LegendShape = 'line' | 'circle'

export interface LegendItem {
  id: string
  label: string
  color: string
  width?: number // thickness for lines, diameter for circles
  shape?: LegendShape
  isSolid?: boolean
  renderer?: (item: LegendItem) => React.ReactNode
}

export interface LegendProps {
  title?: string | React.ReactNode
  items: LegendItem[]
  sx?: SxProps<Theme>
}

const Legend = ({ title, items, sx }: LegendProps) => {
  const [expanded, setExpanded] = useState(true)

  const legendVisibility = useSegmentEditorStore(
    (state) => state.legendVisibility
  )
  const toggleLegendVisibility = useSegmentEditorStore(
    (state) => state.toggleLegendVisibility
  )

  return (
    <Box
      sx={{
        p: 1,
        pl: 2,
        pr: 1,
        backgroundColor: 'rgba(255,255,255,0.95)',
        borderRadius: '4px',
        fontSize: '0.75rem',
        boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
        width: expanded ? 'max-content' : 120,
        ...sx,
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', mb: expanded ? 2 : 0 }}>
        {title && (
          <Typography variant="subtitle2" sx={{ flexGrow: 1 }}>
            {title}
          </Typography>
        )}
        <IconButton
          size="small"
          onClick={() => setExpanded((prev) => !prev)}
          sx={{ p: 0.25 }}
        >
          {expanded ? (
            <ExpandMoreIcon fontSize="small" />
          ) : (
            <ExpandLessIcon fontSize="small" />
          )}
        </IconButton>
      </Box>

      {expanded &&
        items.map((item) => {
          const key = item.id as keyof typeof legendVisibility
          const visible = legendVisibility[key] ?? false

          return (
            <Box
              key={item.id}
              sx={{ display: 'flex', alignItems: 'center', mb: 1 }}
            >
              <Checkbox
                size="small"
                checked={visible}
                onChange={() => toggleLegendVisibility(key)}
                sx={{ p: 0, mr: 1 }}
              />

              {item.renderer ? (
                item.renderer(item)
              ) : item.shape === 'circle' ? (
                <Box
                  sx={{
                    width: item.width ?? 8,
                    height: item.width ?? 8,
                    borderRadius: '50%',
                    backgroundColor: item.color,
                    mr: 1,
                  }}
                />
              ) : (
                <Box
                  sx={{
                    width: 18,
                    height: item.width ?? 2,
                    backgroundColor: item.color,
                    mr: 1,
                  }}
                />
              )}

              <Box component="span" sx={{ lineHeight: 1 }}>
                {item.label}
              </Box>
            </Box>
          )
        })}
    </Box>
  )
}

export default Legend
