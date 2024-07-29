import {
  Box,
  Checkbox,
  FormControlLabel,
  Paper,
  TextField,
  Typography,
} from '@mui/material'

import {
  StyledComponentHeader,
  commonPaperStyle,
} from '@/components/HeaderStyling/StyledComponentHeader'

const textFieldStyle = {
  width: '40px', // Set the width to 60px
  height: '30px', // Set the height to 60px
  '& input': {
    height: '30px',
    padding: '0', // Adjust padding to center the text vertically
    textAlign: 'center', // Center the text horizontally
    fontSize: '.75rem',
  },
  '& input[type=number]': {
    //hides the up and down arrows in the TextField
    '-moz-appearance': 'textfield',
    '&::-webkit-outer-spin-button': {
      '-webkit-appearance': 'none',
      margin: 0,
    },
    '&::-webkit-inner-spin-button': {
      '-webkit-appearance': 'none',
      margin: 0,
    },
  },
}

interface ReportInformationProps {
  finalGapAnalysisReport: boolean
  setFinalGapAnalysisReport: React.Dispatch<React.SetStateAction<boolean>>
  splitFailAnalysis: boolean
  setSplitFailAnalysis: React.Dispatch<React.SetStateAction<boolean>>
  pedestrianCallAnalysis: boolean
  setPedestrianCallAnalysis: React.Dispatch<React.SetStateAction<boolean>>
  conflictingVolumesAnalysis: boolean
  setConflictingVolumesAnalysis: React.Dispatch<React.SetStateAction<boolean>>
  vehiclesPercentageAcceptableGaps: number
  setVehiclesPercentageAcceptableGaps: React.Dispatch<
    React.SetStateAction<number>
  >
  acceptableSplitFailPercentage: number
  setAcceptableSplitFailPercentage: React.Dispatch<React.SetStateAction<number>>
}

export const ReportInformation = ({
  finalGapAnalysisReport,
  setFinalGapAnalysisReport,
  splitFailAnalysis,
  setSplitFailAnalysis,
  pedestrianCallAnalysis,
  setPedestrianCallAnalysis,
  conflictingVolumesAnalysis,
  setConflictingVolumesAnalysis,
  vehiclesPercentageAcceptableGaps,
  setVehiclesPercentageAcceptableGaps,
  acceptableSplitFailPercentage,
  setAcceptableSplitFailPercentage,
}: ReportInformationProps) => {
  const handleCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = event.target
    switch (name) {
      case 'finalGapAnalysisReport':
        setFinalGapAnalysisReport(checked)
        break
      case 'splitFailAnalysis':
        setSplitFailAnalysis(checked)
        break
      case 'pedestrianCallAnalysis':
        setPedestrianCallAnalysis(checked)
        break
      case 'conflictingVolumesAnalysis':
        setConflictingVolumesAnalysis(checked)
        break
      default:
        // Handle unexpected cases
        break
    }
  }

  return (
    <Paper
      sx={{
        ...commonPaperStyle,
        '@media (max-width: 1200px)': {
          marginBottom: 1,
        },
      }}
    >
      <Box>
        <StyledComponentHeader header={'Location Data Check'} />
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          gap: '8px',
          margin: '0 25px',
        }}
      >
        <FormControlLabel
          control={
            <Checkbox
              checked={finalGapAnalysisReport}
              onChange={handleCheckboxChange}
              name="finalGapAnalysisReport"
            />
          }
          label="Final Gap Analysis Report"
        />

        {finalGapAnalysisReport && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <TextField
              id="accetable-gaps"
              type="number"
              value={vehiclesPercentageAcceptableGaps}
              onChange={(e) => {
                setVehiclesPercentageAcceptableGaps(
                  (e.target as HTMLInputElement).valueAsNumber
                )
              }}
              onBlur={(e) => {
                if (e.target.value === '') {
                  setVehiclesPercentageAcceptableGaps(0)
                }
              }}
              sx={{ ...textFieldStyle }}
            />
            <label htmlFor="accetable-gaps">
              LT Vehicles % Acceptable Gaps
            </label>
          </Box>
        )}

        <FormControlLabel
          control={
            <Checkbox
              checked={splitFailAnalysis}
              onChange={handleCheckboxChange}
              name="splitFailAnalysis"
            />
          }
          label="Split Fail Analysis"
        />
        {splitFailAnalysis && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <TextField
              id="accetable-split-fail"
              type="number"
              value={acceptableSplitFailPercentage}
              onChange={(e) => {
                setAcceptableSplitFailPercentage(
                  (e.target as HTMLInputElement).valueAsNumber
                )
              }}
              onBlur={(e) => {
                if (e.target.value === '') {
                  setAcceptableSplitFailPercentage(0)
                }
              }}
              sx={{ ...textFieldStyle }}
            />
            <label htmlFor="accetable-split-fail">Acceptable Split Fail %</label>
          </Box>
        )}

        <FormControlLabel
          control={
            <Checkbox
              checked={pedestrianCallAnalysis}
              onChange={handleCheckboxChange}
              name="pedestrianCallAnalysis"
            />
          }
          label="Pedestrian Call Analysis"
        />
        <FormControlLabel
          control={
            <Checkbox
              checked={conflictingVolumesAnalysis}
              onChange={handleCheckboxChange}
              name="conflictingVolumesAnalysis"
            />
          }
          label="Conflicting Volumes Analysis"
        />
      </Box>
    </Paper>
  )
}
