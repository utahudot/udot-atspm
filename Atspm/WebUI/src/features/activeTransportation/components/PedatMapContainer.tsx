import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import PedatMapWrapper from '@/features/activeTransportation/components/pedatMap/PedatMapWrapper'
import { Box, Paper } from '@mui/material'

const PedatMapContainer = ({ data, timeUnit }: PedatChartsContainerProps) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: '77vh',
      }}
    >
      <Paper
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          overflow: 'hidden',
          height: 'auto',
        }}
      >
        <PedatMapWrapper data={data} timeUnit={timeUnit} />
      </Paper>
    </Box>
  )
}

export default PedatMapContainer
