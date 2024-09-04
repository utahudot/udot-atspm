import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { useGetDetectionTypeGroup } from '@/features/watchdog/api/getDetectionTypeGroup'
import { useGetIssueTypeGroup } from '@/features/watchdog/api/getIssueTypeGroup'
import { format, subDays } from 'date-fns'
import React, { useEffect, useMemo, useState } from 'react'
import transformDetectionTypeData from '../watchDogDetectionType.transformer'
import transformWatchdogIssueTypeData from '../watchdogIssueType.transformer'
import transformWatchdogControllerTypeData from './watchdogControllerType.transformer'
import ControllerTypeBreakdownChart from './ControllerTypeBreakDownChart'
import IssueTypeBreakdownChart from './IssueTypeBreakdownChart'

const detectionTypeDummyData = [
  {
    detectionType: 2,
    hardware: [
      {
        name: 'Unknown',
        counts: 151,
      },
      {
        name: 'Wavetronix Advance',
        counts: 949,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 49,
      },
      {
        name: 'Inductive Loops',
        counts: 8,
      },
    ],
    name: 'AC',
  },
  {
    detectionType: 3,
    hardware: [
      {
        name: 'Unknown',
        counts: 75,
      },
      {
        name: 'Wavetronix Advance',
        counts: 400,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 100,
      },
      {
        name: 'Inductive Loops',
        counts: 20,
      },
    ],
    name: 'BC',
  },
  {
    detectionType: 1,
    hardware: [
      {
        name: 'Unknown',
        counts: 30,
      },
      {
        name: 'Wavetronix Advance',
        counts: 200,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 50,
      },
      {
        name: 'Inductive Loops',
        counts: 10,
      },
    ],
    name: 'CC',
  },
  {
    detectionType: 4,
    hardware: [
      {
        name: 'Unknown',
        counts: 42,
      },
      {
        name: 'Wavetronix Advance',
        counts: 300,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 75,
      },
      {
        name: 'Inductive Loops',
        counts: 15,
      },
    ],
    name: 'DC',
  },
  {
    detectionType: 5,
    hardware: [
      {
        name: 'Unknown',
        counts: 90,
      },
      {
        name: 'Wavetronix Advance',
        counts: 500,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 125,
      },
      {
        name: 'Inductive Loops',
        counts: 25,
      },
    ],
    name: 'EC',
  },
  {
    detectionType: 6,
    hardware: [
      {
        name: 'Unknown',
        counts: 60,
      },
      {
        name: 'Wavetronix Advance',
        counts: 350,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 90,
      },
      {
        name: 'Inductive Loops',
        counts: 18,
      },
    ],
    name: 'FC',
  },
  {
    detectionType: 7,
    hardware: [
      {
        name: 'Unknown',
        counts: 24,
      },
      {
        name: 'Wavetronix Advance',
        counts: 250,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 60,
      },
      {
        name: 'Inductive Loops',
        counts: 12,
      },
    ],
    name: 'GC',
  },
  {
    detectionType: 8,
    hardware: [
      {
        name: 'Unknown',
        counts: 96,
      },
      {
        name: 'Wavetronix Advance',
        counts: 450,
      },
      {
        name: 'Wavetronix Matrix',
        counts: 110,
      },
      {
        name: 'Inductive Loops',
        counts: 22,
      },
    ],
    name: 'HC',
  },
]

const WatchDogIssueTypeContainer: React.FC = () => {
    const endDate = useMemo(() => new Date(), [])
    const startDate = useMemo(() => subDays(endDate, 7), [endDate])
    const start = format(startDate, "yyyy-MM-dd'T'HH:mm:ss")
    const end = format(endDate, "yyyy-MM-dd'T'HH:mm:ss")
  
    const [issueTypeData, setIssueTypeData] = useState<any[]>([])
  
    const { data, isLoading, error } = useGetIssueTypeGroup({
      start: '2024-08-01T15:00:00Z',
      end: '2024-09-01T00:00:00Z',
    })
  
    useEffect(() => {
      if (!isLoading && data) {
        const issueTypeGroup = transformWatchdogIssueTypeData(data.issueTypeGroup)
        setIssueTypeData(issueTypeGroup.sunburst)
      }
    }, [data, isLoading])

  //! delelete this after testing
  const {
    data: detectionTypeGroupData,
    isLoading: isDetectionTypeGroupLoading,
  } = useGetDetectionTypeGroup({
    start: '2024-08-01T15:00:00Z',
    end: '2024-09-01T00:00:00Z',
  })

  const detectionTypeChartOption = useMemo(() => {
    if (detectionTypeGroupData) {
      return transformDetectionTypeData(detectionTypeDummyData)
    }
    return null
  }, [detectionTypeGroupData])
  //! delelete this after testing

  if (error) {
    return <div>Error loading data: {error.message}</div>
  }

  return (
    <div style={{ width: '100%', height: '600px' }}>
      <ControllerTypeBreakdownChart 
        data={data?.controllerTypeGroup} 
        isLoading={isLoading} 
      />

<IssueTypeBreakdownChart 
        data={data?.issueTypeGroup} 
        isLoading={isLoading} 
      />

      <div style={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Detection Type Breakdown</h2>
      </div>

      {!isDetectionTypeGroupLoading && detectionTypeChartOption && (
        <ApacheEChart
          id="watchdog-detection-type-chart"
          option={detectionTypeChartOption}
          loading={isDetectionTypeGroupLoading}
          style={{ width: '100%', height: '100%' }}
          hideInteractionMessage
        />
      )}
    </div>
  )
}

export default WatchDogIssueTypeContainer
