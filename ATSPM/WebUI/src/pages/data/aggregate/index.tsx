import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { RouteSelect } from '@/components/RouteSelect/RouteSelect'
import { StyledPaper } from '@/components/StyledPaper'
import { AggregateDataOptions } from '@/features/data/aggregate/components/aggregateDataOptions'
import AggregateResults from '@/features/data/aggregate/components/aggregateResults'
import { useAggregateOptionsHandler } from '@/features/data/aggregate/handlers/aggregateDataHandler'
import transformAggregateData from '@/features/data/aggregate/transformers/aggregateTransformer'
import { TransformedAggregateData } from '@/features/data/aggregate/types/aggregateData'
import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import { RefObject, createRef, useEffect, useRef, useState } from 'react'

const AggregateCharts = () => {
  const handler = useAggregateOptionsHandler()
  const [areRefsReady, setAreRefsReady] = useState(false)
  const [chartData, setChartData] = useState<TransformedAggregateData>()
  const chartRefs = useRef<RefObject<HTMLDivElement>[]>([])

  useEffect(() => {
    setAreRefsReady(false)
    if (!chartData) {
      return
    }
    const dataLength = chartData.data.charts.length
    chartRefs.current = new Array(dataLength).fill(null).map(() => createRef())

    setAreRefsReady(chartRefs.current.length === dataLength)
  }, [chartData])

  useEffect(() => {
    if (handler.aggregatedData.length) {
      const data = transformAggregateData(handler)
      setChartData(data)
    }
  }, [handler.aggregatedData])

  return (
    <ResponsivePageLayout title={'Aggregate Charts'}>
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          flexWrap: 'wrap',
        }}
      >
        <StyledPaper sx={{ padding: 3, flexGrow: 1, maxWidth: '950px' }}>
          <RouteSelect handler={handler} hasLocationMap />
        </StyledPaper>
        <AggregateDataOptions handler={handler} />
      </Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flexStart',
          margin: '20px 0',
        }}
      >
        <LoadingButton
          variant="contained"
          onClick={handler.handleRunAnalysis}
          sx={{ margin: '20px 0', padding: '10px' }}
          disabled={!handler.updatedLocations.length}
        >
          Run Analysis
        </LoadingButton>
      </Box>
      <Box>
        {areRefsReady && (
          <AggregateResults
            refs={chartRefs.current}
            chartData={chartData as TransformedAggregateData}
          />
        )}
      </Box>
    </ResponsivePageLayout>
  )
}

export default AggregateCharts
