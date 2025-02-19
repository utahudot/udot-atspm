import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { Box, Paper, useMediaQuery, useTheme } from '@mui/material'
import React, { useEffect, useState } from 'react'
import transformWatchdogIssueTypeData from '../watchdogIssueType.transformer'
import SunburstLegend from './SunburstLegend'

interface IssueTypeChartProps {
  data: any // Replace 'any' with the actual type of your issue type data
  isLoading: boolean
}

const IssueTypeChart: React.FC<IssueTypeChartProps> = ({ data, isLoading }) => {
  const [issueTypeData, setIssueTypeData] = useState<any>(null)
  const [issueTypeLegend, setIssueTypeLegend] = useState<
    { name: string; color: string; selected: boolean }[]
  >([])
  const [deselectedIssueTypes, setDeselectedIssueTypes] = useState<string[]>([
    'UnconfiguredDetector',
    'UnconfiguredApproach',
  ])

  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))

  useEffect(() => {
    if (data) {
      const { sunburst, legendData } = transformWatchdogIssueTypeData(
        data,
        deselectedIssueTypes
      )
      setIssueTypeData(sunburst)
      setIssueTypeLegend(legendData)
    }
  }, [data, deselectedIssueTypes])

  const toggleIssueTypeLegend = (name: string) => {
    setDeselectedIssueTypes((prev) => {
      if (prev.includes(name)) {
        return prev.filter((item) => item !== name)
      } else {
        return [...prev, name]
      }
    })
  }

  return (
    <Paper sx={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
      <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Issue Type Breakdown</h2>
      </Box>

      <Box
        sx={{
          display: 'flex',
          flexDirection: isMobile ? 'column' : 'row',
          width: '100%',
          justifyContent: 'space-between',
        }}
      >
        <Box
          sx={{
            flex: isMobile ? 'none' : '0 0 80%',
            width: isMobile ? '100%' : '80%',
            paddingLeft: isMobile ? 0 : '15rem',
          }}
        >
          {!isLoading && issueTypeData && issueTypeData.series && (
            <ApacheEChart
              id="watchdog-issue-type-chart"
              option={issueTypeData}
              loading={isLoading}
              style={{ width: '100%', height: '700px' }}
              hideInteractionMessage
            />
          )}
        </Box>
        <Box
          sx={{
            flex: isMobile ? 'none' : '0 0 20%',
            width: isMobile ? '100%' : '18%',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            paddingLeft: isMobile ? 0 : '23px',
            marginTop: isMobile ? '20px' : 0,
            marginBottom: '1rem',
          }}
        >
          <SunburstLegend
            legendData={issueTypeLegend}
            onToggle={toggleIssueTypeLegend}
            isMobile={isMobile}
          />
        </Box>
      </Box>
    </Paper>
  )
}

export default IssueTypeChart
