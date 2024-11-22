// components/LeftTurnGapReportForm.tsx
import SelectDateTime from '@/components/selectTimeSpan'
import { StyledPaper } from '@/components/StyledPaper'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { LeftTurnGapOptions } from '@/features/leftTurnGapReport/components/leftTurnGapOptions'
import { LocationDataCheck } from '@/features/leftTurnGapReport/components/locationDataCheck'
import { ReportInformation } from '@/features/leftTurnGapReport/components/reportInformation'
import { TimeOptions } from '@/features/leftTurnGapReport/components/timeOptions'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'
import { LeftTurnGapReportParams } from '@/pages/left-turn-gap-report'
import { Box } from '@mui/material'
import React, { useCallback } from 'react'

const daysOfWeekList = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
]

interface LeftTurnGapReportFormProps {
  params: LeftTurnGapReportParams
  setParams: React.Dispatch<React.SetStateAction<LeftTurnGapReportParams>>
  approachesData: any
}

const LeftTurnGapReportForm: React.FC<LeftTurnGapReportFormProps> = ({
  params,
  setParams,
  approachesData,
}) => {
  const setApproachIds = useCallback(
    (ids: number[]) => {
      setParams((prev: LeftTurnGapReportParams) => ({
        ...prev,
        approachIds: ids,
      }))
    },
    [setParams]
  )

  const handleLocationChange = (location: Location) => {
    setParams((prev: LeftTurnGapReportParams) => ({
      ...prev,
      location,
    }))
  }

  return (
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
          flexGrow: 1,
          padding: 3,
          minWidth: '450px',
        }}
      >
        <SelectLocation
          location={params.location}
          setLocation={handleLocationChange}
        />
      </StyledPaper>

      <Box>
        <Box
          sx={{
            display: 'flex',
            marginBottom: 2,
            marginLeft: 2,
            gap: 2,
            flexWrap: 'wrap',
          }}
        >
          <LeftTurnGapOptions
            approaches={approachesData}
            approachIds={params.approachIds}
            setApproachIds={setApproachIds}
          />
          <LocationDataCheck
            cyclesWithPedCalls={params.cyclesWithPedCalls}
            setCyclesWithPedCalls={(value) =>
              setParams((prev: any) => ({ ...prev, cyclesWithPedCalls: value }))
            }
            cyclesWithGapOuts={params.cyclesWithGapOuts}
            setCyclesWithGapOuts={(value: number) =>
              setParams((prev: any) => ({ ...prev, cyclesWithGapOuts: value }))
            }
            leftTurnVolume={params.leftTurnVolume}
            setLeftTurnVolume={(value: number) =>
              setParams((prev: any) => ({ ...prev, leftTurnVolume: value }))
            }
          />
          <ReportInformation
            finalGapAnalysisReport={params.finalGapAnalysisReport}
            setFinalGapAnalysisReport={(value: boolean) =>
              setParams((prev: any) => ({
                ...prev,
                finalGapAnalysisReport: value,
              }))
            }
            splitFailAnalysis={params.splitFailAnalysis}
            setSplitFailAnalysis={(value: boolean) =>
              setParams((prev: any) => ({ ...prev, splitFailAnalysis: value }))
            }
            pedestrianCallAnalysis={params.pedestrianCallAnalysis}
            setPedestrianCallAnalysis={(value: boolean) =>
              setParams((prev: any) => ({
                ...prev,
                pedestrianCallAnalysis: value,
              }))
            }
            conflictingVolumesAnalysis={params.conflictingVolumesAnalysis}
            setConflictingVolumesAnalysis={(value: boolean) =>
              setParams((prev: any) => ({
                ...prev,
                conflictingVolumesAnalysis: value,
              }))
            }
            vehiclesPercentageAcceptableGaps={
              params.vehiclesPercentageAcceptableGaps
            }
            setVehiclesPercentageAcceptableGaps={(value: number) =>
              setParams((prev: any) => ({
                ...prev,
                vehiclesPercentageAcceptableGaps: value,
              }))
            }
            acceptableSplitFailPercentage={params.acceptableSplitFailPercentage}
            setAcceptableSplitFailPercentage={(value: number) =>
              setParams((prev: any) => ({
                ...prev,
                acceptableSplitFailPercentage: value,
              }))
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
                startDateTime={params.startDateTime}
                endDateTime={params.endDateTime}
                changeStartDate={(date: Date) =>
                  setParams((prev: any) => ({ ...prev, startDateTime: date }))
                }
                changeEndDate={(date: Date) =>
                  setParams((prev: any) => ({ ...prev, endDateTime: date }))
                }
              />
            </StyledPaper>
          </Box>
          <TimeOptions
            timeOptions={params.timeOptions}
            setTimeOptions={(value: string) =>
              setParams((prev: any) => ({ ...prev, timeOptions: value }))
            }
            startHour={params.startHour}
            setStartHour={(value: number) =>
              setParams((prev: any) => ({ ...prev, startHour: value }))
            }
            endHour={params.endHour}
            setEndHour={(value: number) =>
              setParams((prev: any) => ({ ...prev, endHour: value }))
            }
            startMinute={params.startMinute}
            setStartMinute={(value: number) =>
              setParams((prev: any) => ({ ...prev, startMinute: value }))
            }
            endMinute={params.endMinute}
            setEndMinute={(value: number) =>
              setParams((prev: any) => ({ ...prev, endMinute: value }))
            }
            setGetAMPMPeakHour={(value: boolean) =>
              setParams((prev: any) => ({ ...prev, getAMPMPeakHour: value }))
            }
            setGet24HourPeriod={(value: boolean) =>
              setParams((prev: any) => ({ ...prev, get24HourPeriod: value }))
            }
            setGetAMPMPeakPeriod={(value: boolean) =>
              setParams((prev: any) => ({ ...prev, getAMPMPeakPeriod: value }))
            }
          />
          <MultiSelectCheckbox
            itemList={daysOfWeekList}
            selectedItems={params.selectedDays}
            setSelectedItems={(days: number[]) =>
              setParams((prev: any) => ({ ...prev, selectedDays: days }))
            }
            header="Days"
          />
        </Box>
      </Box>
    </Box>
  )
}

export default LeftTurnGapReportForm
