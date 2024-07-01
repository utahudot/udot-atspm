import { ToolType } from '@/features/charts/common/types'
import { toUTCDateStamp, toUTCDateWithTimeStamp } from '@/utils/dateTime'
import { LoadingButton } from '@mui/lab'
import { Box, Paper, Typography, useTheme } from '@mui/material'
import { useEffect, useState } from 'react'
import { useLinkPivotPcdCharts } from '../api/getLinkPivotPcdCharts'
import { LinkPivotHandler } from '../handlers/linkPivotHandlers'
import {
  LinkPivotPcdHandler,
  useLinkPivotPcdOptionsHandler,
} from '../handlers/linkPivotPcdHandlers'
import {
  LinkPivotComponentDto,
  LinkPivotPcdOptions,
  RawExistingPcdData,
  RawLinkPivotPcdResponse,
  RawPredictedPcdData,
} from '../types'
import { LinkPivotPcdCharts } from './LinkPivotPcdCharts'
import { LinkPivotPcdOptionsComponent } from './LinkPivotPcdOptionsComponent'

interface props {
  pcdDto: LinkPivotComponentDto
  lpHandler: LinkPivotHandler
}

const createBasePcdOptions = (
  handler: LinkPivotPcdHandler,
  pcdDto: LinkPivotComponentDto
): LinkPivotPcdOptions => {
  return {
    startDate: toUTCDateStamp(handler.startDateTime),
    endDate: toUTCDateStamp(handler.endDateTime),
    startTime: toUTCDateWithTimeStamp(handler.startTime),
    endTime: toUTCDateWithTimeStamp(handler.endTime),
    locationIdentifier: pcdDto.locationIdentifier,
    downstreamLocationIdentifier: pcdDto.downstreamLocationIdentifier,
    downstreamApproachDirection: pcdDto.downstreamApproachDirection,
    upstreamApproachDirection: pcdDto.upstreamApproachDirection,
    delta: pcdDto.delta,
  }
}

export const LinkPivotPcdComponent = ({ pcdDto, lpHandler }: props) => {
  const theme = useTheme()
  const [data, setData] = useState<RawLinkPivotPcdResponse>()
  const [existingPcd, setExistingPcd] = useState<RawExistingPcdData>()
  const [predictedPcd, setPredictedPcd] = useState<RawPredictedPcdData>()

  const handleToolOptions = (value: Partial<LinkPivotPcdOptions>) => {
    const [key, val] = Object.entries(value)[0]
    setToolOptions((prev) => ({ ...prev, [key]: val }))
  }

  const handler = useLinkPivotPcdOptionsHandler({
    handleToolOptions,
    pcdDto,
    lpHandler,
  })

  const [toolOptions, setToolOptions] = useState(
    createBasePcdOptions(handler, pcdDto)
  )
  const {
    refetch,
    data: chartData,
    isLoading,
  } = useLinkPivotPcdCharts({
    toolType: ToolType.LpPcd,
    toolOptions,
  })

  const runPcdCharts = () => {
    refetch()
  }

  useEffect(() => {
    if (chartData !== undefined || chartData !== null) {
      setData(chartData)
      setExistingPcd(chartData?.data)
      setPredictedPcd(chartData?.data)
    }
  }, [chartData])

  return (
    <Box padding={theme.spacing(1)}>
      <Box
        component={Paper}
        width="fit-content"
        padding={theme.spacing(2)}
        marginBottom={theme.spacing(2)}
      >
        <Typography
          paddingLeft={theme.spacing(1)}
          textAlign="left"
          fontWeight="bold"
        >
          PCD Options for {pcdDto.locationIdentifier}
        </Typography>
        <Box display="flex" alignItems="center">
          <LinkPivotPcdOptionsComponent handler={handler} />
          <LoadingButton
            size="small"
            loading={isLoading}
            variant="contained"
            onClick={runPcdCharts}
            sx={{ margin: '20px 0', padding: '10px' }}
          >
            Run Analysis
          </LoadingButton>
        </Box>
      </Box>
      {data && (
        <LinkPivotPcdCharts
          existingPcd={existingPcd as RawExistingPcdData}
          predictedPcd={predictedPcd as RawPredictedPcdData}
        />
      )}
    </Box>
  )
}
