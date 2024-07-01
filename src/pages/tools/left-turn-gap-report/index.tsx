import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { useLeftTurnGapReport } from '@/features/leftTurnGapReport/api/getLTGRData'
import LTGRReportView from '@/features/leftTurnGapReport/components/LTGRReportView'
import { LeftTurnGapOptions } from '@/features/leftTurnGapReport/components/leftTurnGapOptions'
import { LocationDataCheck } from '@/features/leftTurnGapReport/components/locationDataCheck'
import { ReportInformation } from '@/features/leftTurnGapReport/components/reportInformation'
import { TimeOptions } from '@/features/leftTurnGapReport/components/timeOptions'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'
import Authorization from '@/lib/Authorization'
import { LoadingButton } from '@mui/lab'
import { Alert, Box, CircularProgress, useTheme } from '@mui/material'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'
import { useLeftTurnApproaches } from '../../../features/leftTurnGapReport/api/getLeftTurnApproaches'
import { useLeftTurnGapReportDataCheck } from '../../../features/leftTurnGapReport/api/getLeftTurnGapReportDataCheck'
import RunCheckGrid from '../run-check-grid'

function LeftTurnGapReport() {
  const theme = useTheme()
  const [location, setLocation] = useState<Location | null>(null)
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())

  //Map states
  const [selectedLocations, setSelectedLocations] = useState<string[]>([])

  //location Data states
  const [cyclesWithPedCalls, setCyclesWithPedCalls] = useState<number>(30)
  const [cyclesWithGapOuts, setCyclesWithGapOuts] = useState<number>(50)
  const [leftTurnVolume, setLeftTurnVolume] = useState<number>(60)

  //Report Information states
  const [finalGapAnalysisReport, setFinalGapAnalysisReport] =
    useState<boolean>(true)
  const [
    vehiclesPercentageAcceptableGaps,
    setVehiclesPercentageAcceptableGaps,
  ] = useState<number>(70)
  const [acceptableSplitFailPercentage, setAcceptableSplitFailPercentage] =
    useState<number>(50)

  const [splitFailAnalysis, setSplitFailAnalysis] = useState<boolean>(true)
  const [pedestrianCallAnalysis, setPedestrianCallAnalysis] =
    useState<boolean>(true)
  const [conflictingVolumesAnalysis, setConflictingVolumesAnalysis] =
    useState<boolean>(true)

  //Time Options states
  const [timeOptions, setTimeOptions] = useState<string>('PeakHourRadiobutton')
  const [startHour, setStartHour] = useState<number>(0)
  const [endHour, setEndHour] = useState<number>(23)
  const [startMinute, setStartMinute] = useState<number>(0)
  const [endMinute, setEndMinute] = useState<number>(59)
  const [getAMPMPeakHour, setGetAMPMPeakHour] = useState<boolean>(true)
  const [get24HourPeriod, setGet24HourPeriod] = useState<boolean>(false)
  const [getAMPMPeakPeriod, setGetAMPMPeakPeriod] = useState<boolean>(false)
  //approaches
  const [approachIds, setApproachIds] = useState<number[]>([])
  //Days of the Week State
  const [selectedDays, setSelectedDays] = useState<number[]>([
    0, 1, 2, 3, 4, 5, 6,
  ])
  // const [boxWidth, setBoxWidth] = useState(0)
  // const boxRef = useRef(null)

  // const [selectedDays, setSelectedDays] = useState<number[]>([
  //   1, 2, 3, 4, 5, 6, 7,
  // ])

  const [runCheckSuccess, setRunCheckSuccess] = useState<boolean>(false)
  const [errorMessages, setErrorMessages] = useState<string[]>([])

  //used obj used to to pass into component props. didnt want to do the whole cyclesWithPedCalls={cyclesWithPedCalls} 6 times
  const LeftTurnGapOptionProps = {
    cyclesWithPedCalls,
    setCyclesWithPedCalls,
    cyclesWithGapOuts,
    setCyclesWithGapOuts,
    leftTurnVolume,
    setLeftTurnVolume,
  }

  const daysOfWeekList: string[] = [
    'Sunday',
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
  ]

  useEffect(() => {
    if (location && !selectedLocations.includes(location?.locationIdentifier)) {
      setSelectedLocations((prevArr) => [
        ...prevArr,
        location.locationIdentifier,
      ])
    }
  }, [selectedLocations, location])

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }

  // ! Metric type handler needed

  // Get Approaches from Location
  const { data: approachesData, refetch: refetchApproaches } =
    useLeftTurnApproaches({
      locationId: location?.id || '',
    })

  useEffect(() => {
    if (location?.id) {
      refetchApproaches()
    }
  }, [location, approachesData, refetchApproaches])

  const ReportDataCheckDataBody = {
    locationIdentifier: location?.locationIdentifier,
    start: startDateTime,
    end: endDateTime,
    volumePerHourThreshold: leftTurnVolume,
    gapOutThreshold: cyclesWithGapOuts,
    pedestrianThreshold: cyclesWithPedCalls,
    daysOfWeek: selectedDays,
  }

  const {
    data: ReportDataCheckData,
    error: leftTurnGapReportDataCheckError,
    isLoading: ReportDataCheckDataIsLoading,
    refetch: refetchLeftTurnGapReportDataCheck,
  } = useLeftTurnGapReportDataCheck({
    body: ReportDataCheckDataBody,
    approachIds: approachIds,
  })

  const lTGRData = {
    locationIdentifier: location?.locationIdentifier,
    start: startDateTime,
    end: endDateTime,
    // start: '2022-12-10T00:00:00.000Z',
    // end: '2022-12-11T00:00:00.000Z',
    approachIds: approachIds,
    daysOfWeek: selectedDays,
    startHour: startHour,
    startMinute: startMinute,
    endHour: endHour,
    endMinute: endMinute,
    getAMPMPeakPeriod: getAMPMPeakPeriod, //getAMPMPeakPeriod,
    getAMPMPeakHour: getAMPMPeakHour,
    get24HourPeriod: get24HourPeriod,
    getGapReport: finalGapAnalysisReport,
    acceptableGapPercentage: vehiclesPercentageAcceptableGaps / 100,
    getSplitFail: splitFailAnalysis,
    acceptableSplitFailPercentage: acceptableSplitFailPercentage / 100,
    getPedestrianCall: conflictingVolumesAnalysis,
    getConflictingVolume: conflictingVolumesAnalysis,
  }

  const {
    data: lTGRDataReport,
    isLoading: lTGRDataReportIsloading,
    refetch: refetchLTGRDataReport,
    error: lTGRDataReportError,
  } = useLeftTurnGapReport({
    params: {
      ...lTGRData,
      locationIdentifier: lTGRData.locationIdentifier || '',
    },
  })

  const runCheckOnSubmit = () => {
    if (approachIds.length === 0) {
      setRunCheckSuccess(false)
      return
    }
    refetchLeftTurnGapReportDataCheck().then((response) => {
      if (response && response.isSuccess) {
        setRunCheckSuccess(true)
      } else if (leftTurnGapReportDataCheckError) {
        setRunCheckSuccess(false)
      } else {
        setRunCheckSuccess(false)
      }
    })
  }

  const handleRunReport = () => {
    refetchLTGRDataReport()
  }

  useEffect(() => {
    if (lTGRDataReportError) {
      const errorMessage = lTGRDataReportError.response?.data || 'Unknown error'
      if (!errorMessages.includes(errorMessage)) {
        setErrorMessages((prevErrors) => [...prevErrors, errorMessage])
      }
    } else {
      setErrorMessages([])
    }
  }, [lTGRDataReportError, errorMessages])
  const requiredClaim = 'Report:View'

  // useEffect(() => {
  //   const resizeObserver = new ResizeObserver((entries) => {
  //     for (let entry of entries) {
  //       setBoxWidth(entry.contentRect.width)
  //     }
  //   })

  //   if (boxRef.current) {
  //     resizeObserver.observe(boxRef.current)
  //   }

  //   return () => {
  //     resizeObserver.disconnect()
  //   }
  // }, [])

  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title={'Left Turn Gap Analysis Report'}>
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 2,
            justifyContent: 'center',
          }}
        >
          <StyledPaper
            sx={{
              marginLeft: 2,
              // '@media (max-width: 1796px)': {
              //   maxWidth: boxWidth,
              // },
              flexGrow: 1,
              padding: theme.spacing(3),
              minWidth: '450px',
            }}
          >
            <SelectLocation location={location} setLocation={setLocation} />
          </StyledPaper>

          <Box>
            <Box
              // ref={boxRef}
              sx={{
                display: 'flex',
                marginBottom: 2,
                marginLeft: 2,
                gap: 2,
                flexWrap: 'wrap',
              }}
            >
              <LeftTurnGapOptions
                locationIdentifier={location?.id}
                approaches={approachesData}
                approachIds={approachIds}
                setApproachIds={setApproachIds}
              />
              <LocationDataCheck {...LeftTurnGapOptionProps} />
              <ReportInformation
                finalGapAnalysisReport={finalGapAnalysisReport}
                setFinalGapAnalysisReport={setFinalGapAnalysisReport}
                splitFailAnalysis={splitFailAnalysis}
                setSplitFailAnalysis={setSplitFailAnalysis}
                pedestrianCallAnalysis={pedestrianCallAnalysis}
                setPedestrianCallAnalysis={setPedestrianCallAnalysis}
                conflictingVolumesAnalysis={conflictingVolumesAnalysis}
                setConflictingVolumesAnalysis={setConflictingVolumesAnalysis}
                vehiclesPercentageAcceptableGaps={
                  vehiclesPercentageAcceptableGaps
                }
                setVehiclesPercentageAcceptableGaps={
                  setVehiclesPercentageAcceptableGaps
                }
                acceptableSplitFailPercentage={acceptableSplitFailPercentage}
                setAcceptableSplitFailPercentage={
                  setAcceptableSplitFailPercentage
                }
              />
            </Box>

            <Box
              sx={{
                display: 'flex',
                gap: 2,
                marginLeft: 2,
                flexWrap: 'wrap',
              }}
            >
              <Box>
                <StyledPaper sx={{ padding: 3 }}>
                  <SelectDateTime
                    dateFormat={'MMM dd, yyyy'}
                    startDateTime={startDateTime}
                    endDateTime={endDateTime}
                    changeStartDate={handleStartDateTimeChange}
                    changeEndDate={handleEndDateTimeChange}
                  />
                </StyledPaper>
              </Box>
              <TimeOptions
                timeOptions={timeOptions}
                setTimeOptions={setTimeOptions}
                startHour={startHour}
                setStartHour={setStartHour}
                endHour={endHour}
                setEndHour={setEndHour}
                startMinute={startMinute}
                setStartMinute={setStartMinute}
                endMinute={endMinute}
                setEndMinute={setEndMinute}
                setGetAMPMPeakHour={setGetAMPMPeakHour}
                setGet24HourPeriod={setGet24HourPeriod}
                setGetAMPMPeakPeriod={setGetAMPMPeakPeriod}
              />
              <MultiSelectCheckbox
                itemList={daysOfWeekList}
                selectedItems={selectedDays}
                setSelectedItems={setSelectedDays}
                header="Days"
              />
            </Box>
          </Box>
        </Box>

        <Box
          sx={{
            display: 'flex',
            gap: 2,
          }}
        >
          <Box>
            <LoadingButton
              loadingPosition="start"
              disabled={approachIds.length === 0}
              variant="contained"
              onClick={runCheckOnSubmit}
              sx={{ margin: '20px 0', padding: '10px' }}
            >
              Run Check
            </LoadingButton>
          </Box>

          <Box>
            <LoadingButton
              loadingPosition="start"
              variant="contained"
              onClick={handleRunReport}
              disabled={
                !runCheckSuccess ||
                errorMessages.length != 0 ||
                (!finalGapAnalysisReport &&
                  !splitFailAnalysis &&
                  !pedestrianCallAnalysis &&
                  !conflictingVolumesAnalysis)
              }
              sx={{ margin: '20px 0', padding: '10px' }}
            >
              Run Report
            </LoadingButton>
          </Box>
          {ReportDataCheckDataIsLoading ||
            (lTGRDataReportIsloading && (
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <CircularProgress size={24} />
              </Box>
            ))}
          {errorMessages.map((errorMessage, index) => (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
              key={index}
            >
              <Alert severity="error">{errorMessage}</Alert>
            </Box>
          ))}
        </Box>

        {ReportDataCheckData &&
          Array.isArray(ReportDataCheckData) &&
          ReportDataCheckData.length > 0 && (
            <RunCheckGrid data={ReportDataCheckData} />
          )}

        {lTGRDataReport && (
          <LTGRReportView
            lTGRDataReport={lTGRDataReport}
            approaches={ReportDataCheckData}
          />
        )}
      </ResponsivePageLayout>
    </Authorization>
  )
}

export default LeftTurnGapReport
