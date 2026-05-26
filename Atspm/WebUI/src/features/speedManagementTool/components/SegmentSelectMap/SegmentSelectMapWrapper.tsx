import type { AllSegmentsSegment } from '@/features/speedManagementTool/api/getSegments'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo } from 'react'

const SegmentSelectMap = dynamic(() => import('./SegmentSelectMap'), {
  ssr: false,
})

export interface SegmentSelectMapProps {
  segments?: AllSegmentsSegment[]
  focusedSegmentId?: string | null
  selectedSegmentIds?: string[]
  onSegmentSelect?: (segment: AllSegmentsSegment) => void
}

const SegmentSelectMapWrapper = ({
  focusedSegmentId,
  segments,
  selectedSegmentIds,
  onSegmentSelect,
}: SegmentSelectMapProps) => {
  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <SegmentSelectMap
        focusedSegmentId={focusedSegmentId}
        segments={segments}
        selectedSegmentIds={selectedSegmentIds}
        onSegmentSelect={onSegmentSelect}
      />
    </Box>
  )
}

export default memo(SegmentSelectMapWrapper)
