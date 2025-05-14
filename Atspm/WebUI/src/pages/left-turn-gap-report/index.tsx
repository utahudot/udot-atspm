// LeftTurnGapReport.tsx
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useLeftTurnApproaches } from '@/features/leftTurnGapReport/api/getLeftTurnApproaches'
import { useLeftTurnGapReportDataCheck } from '@/features/leftTurnGapReport/api/getLeftTurnGapReportDataCheck'
import { useLeftTurnGapReport } from '@/features/leftTurnGapReport/api/getLTGRData'
import LeftTurnGapReportForm from '@/features/leftTurnGapReport/components/LeftTurnGapReportForm'
import LTGRReportView from '@/features/leftTurnGapReport/components/LTGRReportView'
import RunCheckGrid from '@/features/leftTurnGapReport/components/RunCheckGrid'
import { Location } from '@/features/locations/types'
import Authorization from '@/lib/Authorization'
import { LoadingButton } from '@mui/lab'
import { Alert, Box } from '@mui/material'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'

export interface LeftTurnGapReportParams {
  location: Location | null
  startDateTime: Date
  endDateTime: Date
  cyclesWithPedCalls: number
  cyclesWithGapOuts: number
  leftTurnVolume: number
  finalGapAnalysisReport: boolean
  vehiclesPercentageAcceptableGaps: number
  acceptableSplitFailPercentage: number
  splitFailAnalysis: boolean
  pedestrianCallAnalysis: boolean
  conflictingVolumesAnalysis: boolean
  timeOptions: string
  startHour: number
  endHour: number
  startMinute: number
  endMinute: number
  getAMPMPeakHour: boolean
  get24HourPeriod: boolean
  getAMPMPeakPeriod: boolean
  approachIds: string[]
  selectedDays: number[]
}

const LeftTurnGapReport = () => {
  const requiredClaim = 'Report:View'

  const [params, setParams] = useState<LeftTurnGapReportParams>({
    location: null,
    startDateTime: startOfToday(),
    endDateTime: startOfTomorrow(),
    cyclesWithPedCalls: 30,
    cyclesWithGapOuts: 50,
    leftTurnVolume: 60,
    finalGapAnalysisReport: true,
    vehiclesPercentageAcceptableGaps: 70,
    acceptableSplitFailPercentage: 50,
    splitFailAnalysis: true,
    pedestrianCallAnalysis: true,
    conflictingVolumesAnalysis: true,
    timeOptions: 'PeakHourRadiobutton',
    startHour: 0,
    endHour: 23,
    startMinute: 0,
    endMinute: 59,
    getAMPMPeakHour: true,
    get24HourPeriod: false,
    getAMPMPeakPeriod: false,
    approachIds: [],
    selectedDays: [0, 1, 2, 3, 4, 5, 6],
  })

  const [runCheckSuccess, setRunCheckSuccess] = useState(false)
  const [errorMessages, setErrorMessages] = useState<string[]>([])

  // Fetch approaches
  const { data: approachesData, refetch: refetchApproaches } =
    useLeftTurnApproaches({
      locationId: params.location?.id || '',
    })

  useEffect(() => {
    if (params.location?.id) {
      refetchApproaches()
    }
  }, [params.location, refetchApproaches])

  // Reset runCheckSuccess when location, timeframe, or approachIds change
  useEffect(() => {
    setRunCheckSuccess(false)
  }, [
    params.location?.id,
    params.startDateTime,
    params.endDateTime,
    params.approachIds,
  ])

  const reportDataCheckBody = {
    locationIdentifier: params.location?.locationIdentifier,
    start: params.startDateTime,
    end: params.endDateTime,
    volumePerHourThreshold: params.leftTurnVolume,
    gapOutThreshold: params.cyclesWithGapOuts,
    pedestrianThreshold: params.cyclesWithPedCalls,
    daysOfWeek: params.selectedDays,
  }

  const reportParams = {
    locationIdentifier: params.location?.locationIdentifier || '',
    start: params.startDateTime,
    end: params.endDateTime,
    approachIds: params.approachIds,
    daysOfWeek: params.selectedDays,
    startHour: params.startHour,
    startMinute: params.startMinute,
    endHour: params.endHour,
    endMinute: params.endMinute,
    getAMPMPeakPeriod: params.getAMPMPeakPeriod,
    getAMPMPeakHour: params.getAMPMPeakHour,
    get24HourPeriod: params.get24HourPeriod,
    getGapReport: params.finalGapAnalysisReport,
    acceptableGapPercentage: params.vehiclesPercentageAcceptableGaps / 100,
    getSplitFail: params.splitFailAnalysis,
    acceptableSplitFailPercentage: params.acceptableSplitFailPercentage / 100,
    getPedestrianCall: params.conflictingVolumesAnalysis,
    getConflictingVolume: params.conflictingVolumesAnalysis,
  }

  const {
    data: reportDataCheckData,
    error: reportDataCheckError,
    isLoading: reportDataCheckIsLoading,
    refetch: refetchReportDataCheck,
  } = useLeftTurnGapReportDataCheck({
    body: reportDataCheckBody,
    approachIds: params.approachIds,
  })

  const {
    data: reportData,
    error: reportDataError,
    isLoading: reportDataIsLoading,
    refetch: refetchReportData,
  } = useLeftTurnGapReport({
    params: reportParams,
  })

  const runCheckOnSubmit = () => {
    if (params.approachIds.length === 0) {
      setRunCheckSuccess(false)
      return
    }
    refetchReportDataCheck().then((response) => {
      if (response && response.isSuccess) {
        setRunCheckSuccess(true)
      } else {
        setRunCheckSuccess(false)
        if (reportDataCheckError) {
          setErrorMessages([reportDataCheckError.message || 'Unknown error'])
        }
      }
    })
  }

  // Handle run report
  const handleRunReport = () => {
    refetchReportData()
  }

  // Handle errors
  useEffect(() => {
    if (reportDataError) {
      const errorMessage = reportDataError.response?.data || 'Unknown error'
      if (!errorMessages.includes(errorMessage)) {
        setErrorMessages((prevErrors) => [...prevErrors, errorMessage])
      }
    } else {
      setErrorMessages([])
    }
  }, [reportDataError])

  const disableRunReport =
    !runCheckSuccess ||
    errorMessages.length !== 0 ||
    (!params.finalGapAnalysisReport &&
      !params.splitFailAnalysis &&
      !params.pedestrianCallAnalysis &&
      !params.conflictingVolumesAnalysis)

  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title="Left Turn Gap Analysis Report">
        <LeftTurnGapReportForm
          params={params}
          setParams={setParams}
          approachesData={approachesData}
        />

        <Box display={'flex'} justifyContent={'space-between'}>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <LoadingButton
              loading={reportDataCheckIsLoading}
              disabled={params.approachIds.length === 0}
              variant="contained"
              onClick={runCheckOnSubmit}
              sx={{ margin: '20px 0', padding: '10px' }}
            >
              Run Check
            </LoadingButton>

            <LoadingButton
              loading={reportDataIsLoading}
              variant="contained"
              onClick={handleRunReport}
              disabled={disableRunReport}
              sx={{ margin: '20px 0', padding: '10px' }}
            >
              Run Report
            </LoadingButton>
          </Box>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Alert severity="info">
              It is always good practice to review the Split Pattern performance
              in conjunction with using this report
            </Alert>
          </Box>
        </Box>
        {reportDataCheckData &&
          Array.isArray(reportDataCheckData) &&
          reportDataCheckData.length > 0 && (
            <RunCheckGrid data={reportDataCheckData} />
          )}

        {reportData && (
          <LTGRReportView
            lTGRDataReport={reportData}
            approaches={reportDataCheckData}
          />
        )}
      </ResponsivePageLayout>
    </Authorization>
  )
}

export default LeftTurnGapReport
