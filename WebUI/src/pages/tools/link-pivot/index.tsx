import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { ToolType } from '@/features/charts/common/types'
import { useLinkPivotAdjustment } from '@/features/tools/link-pivot/api/getLinkPivotAdjustments'
import { LinkPivotAdjustmentComponent } from '@/features/tools/link-pivot/components/LinkPivotAdjustmentComponent'
import { LinkPivotApproachLinkComponent } from '@/features/tools/link-pivot/components/LinkPivotApproachLinkComponent'
import { LinkPivotOptionsComponent } from '@/features/tools/link-pivot/components/LinkPivotOptionsComponent'
import { useLinkPivotOptionsHandler } from '@/features/tools/link-pivot/handlers/linkPivotHandlers'
import {
  LinkPivotAdjustmentOptions,
  RawLinkPivotData,
} from '@/features/tools/link-pivot/types'
import { toUTCDateWithTimeStamp } from '@/utils/dateTime'
import { LoadingButton } from '@mui/lab'
import { Alert, Box, Paper, Typography } from '@mui/material'
import { startOfDay } from 'date-fns'
import { useEffect, useState } from 'react'

const getDefaultChartOptions = (): LinkPivotAdjustmentOptions => {
  const date = new Date()
  const year = date.getUTCFullYear()
  const month = String(date.getUTCMonth() + 1).padStart(2, '0')
  const day = String(date.getUTCDate()).padStart(2, '0')

  const startTime = toUTCDateWithTimeStamp(startOfDay(date))
  const endTime = toUTCDateWithTimeStamp(startOfDay(date))
  const formattedDate = `${year}-${month}-${day}`
  return {
    startDate: formattedDate,
    endDate: formattedDate,
    startTime,
    endTime,
    routeId: '',
    cycleLength: 90,
    daysOfWeek: [1, 2, 3, 4, 5],
    direction: 'Downstream',
    bias: 0,
    biasDirection: 'Downstream',
  }
}

const LinkPivot = () => {
  const [toolOptions, setToolOptions] = useState<LinkPivotAdjustmentOptions>(
    getDefaultChartOptions()
  )
  const [data, setData] = useState<RawLinkPivotData>()
  const [alert, setAlert] = useState('')

  const {
    refetch,
    data: chartData,
    isLoading,
    isError,
    error,
  } = useLinkPivotAdjustment({
    toolType: ToolType.LinkPivot,
    toolOptions,
  })

  useEffect(() => {
    if (chartData !== null || chartData !== undefined) {
      setData(chartData)
    }
  }, [chartData])

  const handleToolOptions = (value: Partial<LinkPivotAdjustmentOptions>) => {
    const [key, val] = Object.entries(value)[0]
    setToolOptions((prev) => ({ ...prev, [key]: val }))
  }

  const handler = useLinkPivotOptionsHandler({ handleToolOptions })

  const handleRunAnalysis = () => {
    if (toolOptions.routeId === '') {
      setAlert('Please select a route')
      return
    }
    setAlert('')
    refetch()
  }

  return (
    <ResponsivePageLayout title={'Link Pivot'}>
      <LinkPivotOptionsComponent handler={handler} />
      <Box display={'flex'} alignItems={'center'} height={'50px'} mt={2}>
        <LoadingButton
          loading={isLoading}
          variant="contained"
          onClick={handleRunAnalysis}
          sx={{ margin: '20px 0', padding: '10px' }}
        >
          Run Analysis
        </LoadingButton>
        {isError && (
          <Alert severity="error" sx={{ marginLeft: 1 }}>
            {(error as Error).message}
          </Alert>
        )}
        {alert && (
          <Alert severity="error" sx={{ marginLeft: 1 }}>
            {alert}
          </Alert>
        )}
      </Box>
      <Box sx={{ height: 'auto', width: '100%' }}>
        {data !== undefined && (
          <Box>
            <Typography variant="h3" component="h2" fontWeight="bold" sx={{ mb: 2 }}>
              Adjustments
            </Typography>
            <Paper>
              <LinkPivotAdjustmentComponent
                data={data.adjustments}
                cycleLength={toolOptions.cycleLength}
              />
            </Paper>
            <Typography variant="h3" fontWeight="bold" sx={{ mb: 2, mt: 3 }}>
              Approach Link Comparison
            </Typography>
            <Paper>
              <LinkPivotApproachLinkComponent
                data={data.approachLinks}
                corridorSummary={data}
                lpHandler={handler}
              />
            </Paper>
          </Box>
        )}
      </Box>
    </ResponsivePageLayout>
  )
}

export default LinkPivot
