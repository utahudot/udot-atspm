import IndividualChartControls from '@/features/charts/components/chartsToolbox/IndividualChartControls'
import TimingAndActuationLegendDisplay from '@/features/charts/timingAndActuation/components/timingAndActuationChartsToolbox/TimingAndActuationLegendDisplay'
import {
  Box,
  Button,
  Checkbox,
  Divider,
  FormControlLabel,
  Paper,
  Toolbar,
  Typography,
  useTheme,
} from '@mui/material'
import { useState } from 'react'

interface GeneralChartsControllerProps {
  chartRefs: React.RefObject<HTMLDivElement>[]
  chartData: any
  toggleConfigLabel: string
  toggleConfig: () => void
}

export default function TimingAndActuationChartsToolbox({
  chartRefs,
  chartData,
  toggleConfigLabel,
  toggleConfig,
}: GeneralChartsControllerProps) {
  const theme = useTheme()
  const [showPermissivePhases, setShowPermissivePhases] = useState(true)

  const handleCheckboxChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowPermissivePhases(event.target.checked)
    togglePermissivePhasesVisibility(event.target.checked)
  }

  const togglePermissivePhasesVisibility = (isVisible: boolean) => {
    chartRefs.forEach((ref, i) => {
      if (
        chartData.data.charts[i].chart.displayProps.phaseType === 'Permissive'
      ) {
        ref.current.style.maxHeight = isVisible ? '300px' : '0px'
      }
    })
  }

  return (
    <Paper
      sx={{
        position: 'sticky',
        top: -5,
        marginTop: '-50px',
        zIndex: 1000,
        backgroundColor: '#D3D3D3',
        width: 'fit-content',
        mx: 'auto',
      }}
    >
      <Toolbar variant="dense" sx={{ px: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', width: '100%' }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={showPermissivePhases}
                onChange={handleCheckboxChange}
                name="showPermissivePhases"
                color="default"
                size="small"
              />
            }
            label={
              <Typography
                fontWeight={400}
                fontSize={'.8rem'}
                sx={{ textTransform: 'none' }}
              >
                Show Permissive Phases
              </Typography>
            }
            sx={{ flexGrow: 1 }}
          />
          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
          <TimingAndActuationLegendDisplay chartData={chartData} />
          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
          <Button
            onClick={toggleConfig}
            sx={{
              marginRight: 1,
              mx: '2px',
              color: theme.palette.text.primary,
              textTransform: 'none',
            }}
          >
            <Typography
              fontWeight={400}
              fontSize={'.8rem'}
              sx={{ textTransform: 'none' }}
            >
              View {toggleConfigLabel}
            </Typography>
          </Button>
          <Divider orientation="vertical" flexItem sx={{ mx: 1 }} />
          <IndividualChartControls
            charts={chartData.data.charts}
            chartRefs={chartRefs}
          />
        </Box>
      </Toolbar>
    </Paper>
  )
}
