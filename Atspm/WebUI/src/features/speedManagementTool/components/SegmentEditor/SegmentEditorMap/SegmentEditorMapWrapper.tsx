import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo } from 'react'

const SegmentEditorMap = dynamic(() => import('./SegmentEditorMap'), {
  ssr: false,
})

const SegmentEditorMapWrapper: React.FC = () => {
  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <SegmentEditorMap />
    </Box>
  )
}

SegmentEditorMapWrapper.displayName = 'SegmentEditorMapWrapper'

export default memo(SegmentEditorMapWrapper)
