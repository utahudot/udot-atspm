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
  segmentData: any[]
  isLoadingSegments: boolean
}

const MapWrapper: React.FC<SegmentSelectMapProps> = memo(
  ({ selectedSegmentIds, segmentData, isLoadingSegments, onSegmentSelect }) => {
    if (isLoadingSegments || !segmentData) {
      return <Typography sx={{ ml: 3 }}>Loading...</Typography>
    }

    return (
      <Box sx={{ height: '100%', width: '100%' }}>
        <SegmentSelectMap
          segments={segmentData}
          selectedSegmentIds={selectedSegmentIds}
          onSegmentSelect={onSegmentSelect}
        />
      </Box>
    )
  }
)

MapWrapper.displayName = 'MapWrapper'

export default MapWrapper
