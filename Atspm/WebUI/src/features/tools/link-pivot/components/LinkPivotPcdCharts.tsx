import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedToolResponse } from '@/features/charts/types'
import { Box, Paper, useTheme } from '@mui/material'
import { RefObject, useRef } from 'react'
import transformlpPcdData from '../tranformers/lpPcdTransformer'
import { RawExistingPcdData, RawLpPcdData, RawPredictedPcdData } from '../types'

interface props {
  existingPcd: RawExistingPcdData
  predictedPcd: RawPredictedPcdData
}

export const LinkPivotPcdCharts = ({ existingPcd, predictedPcd }: props) => {
  const theme = useTheme()
  const existingRefs = useRef<RefObject<HTMLDivElement>[]>([])
  const predictedRefs = useRef<RefObject<HTMLDivElement>[]>([])

  if (existingPcd === undefined || predictedPcd === undefined) {
    return
  }

  const transformedExistingData: RawLpPcdData = {
    totalAog: existingPcd.existingTotalAOG,
    totalPAog: existingPcd.existingTotalPAOG,
    volume: existingPcd.existingVolume,
    pcd: existingPcd.pcdExisting,
  }
  const transformedPredictedData: RawLpPcdData = {
    totalAog: predictedPcd.predictedTotalAOG,
    totalPAog: predictedPcd.predictedTotalPAOG,
    volume: predictedPcd.predictedVolume,
    pcd: predictedPcd.pcdPredicted,
  }
  const existingData: TransformedToolResponse = transformlpPcdData(
    transformedExistingData,
    'Existing'
  )
  const predictedData: TransformedToolResponse = transformlpPcdData(
    transformedPredictedData,
    'Predicted'
  )
  return (
    <Box display="flex">
      <Box height="fit-content" width="100%" flex={1}>
        {existingData.data.charts.map((chartWrapper, index) => (
          <Box
            component={Paper}
            padding={theme.spacing(1)}
            key={index}
            ref={existingRefs.current[index]}
            sx={{
              overflow: 'hidden',
              minWidth: '300px',
              marginBottom: theme.spacing(2),
              marginRight: theme.spacing(2),
            }}
          >
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              theme={theme.palette.mode}
              style={{
                width: '100%',
                position: 'relative',
                height: '500px',
              }}
            />
          </Box>
        ))}
      </Box>
      <Box height="fit-content" width="100%" flex={1}>
        {predictedData.data.charts.map((chartWrapper, index) => (
          <Box
            component={Paper}
            padding={theme.spacing(1)}
            key={index}
            ref={predictedRefs.current[index]}
            sx={{
              overflow: 'hidden',
              minWidth: '300px',
              marginBottom: theme.spacing(2),
            }}
          >
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              theme={theme.palette.mode}
              style={{
                width: '100%',
                position: 'relative',
                height: '500px',
              }}
            />
          </Box>
        ))}
      </Box>
    </Box>
  )
}
