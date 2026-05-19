import { Segment } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo } from 'react'

const SegmentSelectMap = dynamic(() => import('./SegmentSelectMap'), {
  ssr: false,
})

export interface SegmentSelectMapProps {
  segments?: Segment[]
  selectedSegmentIds?: string[]
  onSegmentSelect?: (segment: Segment) => void
}

const SegmentSelectMapWrapper = ({
  segments,
  selectedSegmentIds,
  onSegmentSelect,
}: SegmentSelectMapProps) => {
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

export default memo(SegmentSelectMapWrapper)
