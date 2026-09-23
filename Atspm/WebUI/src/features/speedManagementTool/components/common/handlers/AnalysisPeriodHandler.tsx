import { TimePeriodFilter } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { useState } from 'react'

export interface AnalysisPeriodHandler {
  analysisPeriod: TimePeriodFilter | undefined
  updateAnalysisPeriod(analysisPeriod: string): void
  getAnalysisPeriodLabel(): string
}

export const useAnalysisPeriodHandler = (): AnalysisPeriodHandler => {
  const [analysisPeriod, setAnalysisPeriod] = useState<
    TimePeriodFilter | undefined
  >(TimePeriodFilter.AllDay)
  const handleAnalysisPeriodChange = (newValue: string) => {
    setAnalysisPeriod(
      TimePeriodFilter[newValue as keyof typeof TimePeriodFilter]
    )
  }

  const getAnalysisLabel = () => {
    switch (analysisPeriod) {
      case TimePeriodFilter.AllDay:
        return 'All Day'
      case TimePeriodFilter.OffPeak:
        return 'Off-Peak (10 PM - 4 AM)'
      case TimePeriodFilter.AmPeak:
        return 'AM Peak (6 AM - 9 AM)'
      case TimePeriodFilter.PmPeak:
        return 'PM Peak (4 PM - 6 PM)'
      case TimePeriodFilter.MidDay:
        return 'Mid Day (9 AM - 4 PM)'
      case TimePeriodFilter.Evening:
        return 'Evening (6 PM - 10 PM)'
      case TimePeriodFilter.EarlyMorning:
        return 'Early Morning (4 AM - 6 AM)'
      default:
        return 'Analysis Period'
    }
  }

  const component: AnalysisPeriodHandler = {
    analysisPeriod,
    updateAnalysisPeriod: (analysisPeriod: string) => {
      handleAnalysisPeriodChange(analysisPeriod)
    },
    getAnalysisPeriodLabel: () => {
      return getAnalysisLabel()
    },
  }

  return component
}
