import React from 'react'
import { Box, Stack, Typography } from '@mui/material'

interface LegendItem {
  name: string
  color: string
  selected: boolean
}

interface ChartLegendProps {
  legendData: LegendItem[]
  onToggle: (name: string) => void
  isMobile: boolean
}

const SunburstLegend: React.FC<ChartLegendProps> = ({ legendData, onToggle, isMobile }) => {
  return (
    <Stack
      direction={isMobile ? "row" : "column"}
      alignItems="flex-start"
      flexWrap={isMobile ? "wrap" : "nowrap"}
      justifyContent={isMobile ? "center" : "flex-start"}
    >
      {legendData.map((item) => (
        <Box
          key={item.name}
          sx={{
            cursor: 'pointer',
            margin: isMobile ? '2px' : '1px 0',
            display: 'flex',
            alignItems: 'center',
            opacity: item.selected ? 1 : 0.5,
          }}
          onClick={() => onToggle(item.name)}
        >
          <Box
            sx={{
              display: 'inline-block',
              width: '25px',
              height: '14px',
              borderRadius: '15%',
              backgroundColor: item.color,
              marginRight: '5px',
            }}
          />
          <Typography variant={isMobile ? "caption" : "body2"}>{item.name}</Typography>
        </Box>
      ))}
    </Stack>
  )
}

export default SunburstLegend