import { Box, Typography } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import React, { memo } from 'react'

const SegmentSelectMap = dynamic(() => import('./Map'), {
  ssr: false,
})

interface SegmentSelectMapProps {
  selectedSegmentIds: string[]
  onSegmentSelect: (id: string, startMile: number, endMile: number) => void
  segments: any[] | undefined
}

const MapWrapper: React.FC<SegmentSelectMapProps> = memo(
  ({ selectedSegmentIds, segments, onSegmentSelect }) => {
    if (!segments) {
      return <Typography sx={{ ml: 3 }}>Loading...</Typography>
    }

    return (
      <Box sx={{ height: '100%', width: '100%' }}>
        <SegmentSelectMap
          segments={segments}
          selectedSegmentIds={selectedSegmentIds}
          onSegmentSelect={onSegmentSelect}
        />
      </Box>
    )
  }
)

MapWrapper.displayName = 'MapWrapper'

export default MapWrapper
