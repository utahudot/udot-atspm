import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import dynamic from 'next/dynamic'
import { memo } from 'react'

const PedatMap = dynamic(() => import('./PedatMap'), {
  ssr: false,
})

const PedatMapWrapper = ({ data }: PedatChartsContainerProps) => {
  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <PedatMap data={data} />
    </Box>
  )
}

PedatMapWrapper.displayName = 'PedatMapWrapper'

export default memo(PedatMapWrapper)
