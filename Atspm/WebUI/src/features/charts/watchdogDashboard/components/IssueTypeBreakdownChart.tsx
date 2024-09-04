import SunburstLegend from '@/features/charts/watchdogDashboard/components/SunburstLegend'
import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import React, { useEffect, useState } from 'react'
import transformWatchdogIssueTypeData from '../watchdogIssueType.transformer'

interface IssueTypeBreakdownChartProps {
  data: any // Replace 'any' with the actual type of your issue type data
  isLoading: boolean
}

const IssueTypeBreakdownChart: React.FC<IssueTypeBreakdownChartProps> = ({
  data,
  isLoading,
}) => {
  const [issueTypeData, setIssueTypeData] = useState<any>(null)
  const [issueTypeLegend, setIssueTypeLegend] = useState<
    { name: string; color: string; selected: boolean }[]
  >([])
  const [deselectedIssueTypes, setDeselectedIssueTypes] = useState<string[]>([
    'UnconfiguredDetector',
    'UnconfiguredApproach'
  ])

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
    <>
      <div style={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Issue Type Breakdown</h2>
      </div>
      <SunburstLegend
        legendData={issueTypeLegend}
        onToggle={toggleIssueTypeLegend}
      />
      {!isLoading && issueTypeData && issueTypeData.series && (
        <ApacheEChart
          id="watchdog-issue-type-chart"
          option={issueTypeData}
          loading={isLoading}
          style={{ width: '100%', height: '100%' }}
          hideInteractionMessage
        />
      )}
    </>
  )
}

export default IssueTypeBreakdownChart