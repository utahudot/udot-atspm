import { useGetV1LoggingLog } from '@/api/data/aTSPMLogDataApi'
import DevicesWizardPanel from '@/features/locations/components/LocationSetupWizard/DevicesWizardPanel.tsx/DevicesWizardPanel'
import { useLocationStore } from '@/features/locations/locationStore'
import {
  Box,
  Button,
  CircularProgress,
  Paper,
  Step,
  StepLabel,
  Stepper,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

const steps = [
  'Create Location through Template',
  'Configure Devices',
  'Configure Approaches and Detectors',
]

const LocationSetupWizard = () => {
  const { activeStep, setActiveStep, devices } = useLocationStore()
  const [shouldFetch, setShouldFetch] = useState(false)
  const [isExiting, setIsExiting] = useState(false)
  const [isVisible, setIsVisible] = useState(true)

  const {
    data: deviceStatuses,
    isLoading,
    refetch,
    isRefetching,
  } = useGetV1LoggingLog(
    {
      deviceIds: devices.map((device) => device.id).join(','),
    },
    { query: { enabled: shouldFetch } }
  )

  useEffect(() => {
    if (activeStep === 1 && !shouldFetch) {
      setShouldFetch(true)
      refetch() // Trigger refetch the first time step 2 is entered
    }
  }, [activeStep, shouldFetch, refetch])

  const handleNext = () => {
    setActiveStep(Math.min(activeStep + 1, steps.length - 1))
  }

  const handleBack = () => {
    setActiveStep(Math.max(activeStep - 1, 0))
  }

  const handleFinish = () => {
    setIsExiting(true)
  }

  const handleTransitionEnd = (e: React.TransitionEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget && isExiting) {
      setIsVisible(false)
    }
  }

  const handleSync = () => {
    refetch()
  }

  const devicesWithInfo = devices.map((device) => {
    if (!deviceStatuses) return device

    const status = deviceStatuses.find((s) => s.deviceId === device.id)

    return {
      ...device,
      ...status,
      ipModified: status?.ipaddress
        ? device.ipaddress !== status.ipaddress
        : false,
    }
  })

  if (!isVisible) return null

  return (
    <Paper
      sx={{
        position: 'fixed',
        bottom: 10,
        left: '50%',
        transform: isExiting ? 'translate(-50%, 100%)' : 'translate(-50%, 0)',
        transition: 'transform 0.2s ease-in-out',
        p: 2,
        zIndex: 1000,
        minWidth: '900px',
        bgcolor: 'background.default',
      }}
      variant="outlined"
      onTransitionEnd={handleTransitionEnd}
    >
      {activeStep === 1 && (
        <Box sx={{ mb: 2 }}>
          {isLoading ? (
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: '100px',
              }}
            >
              <CircularProgress size={30} />
              <Typography variant="body1" sx={{ ml: 2 }}>
                ... Downloading data from devices
              </Typography>
            </Box>
          ) : (
            <DevicesWizardPanel
              devices={devicesWithInfo || []}
              onResync={handleSync}
              isResyncing={isRefetching}
            />
          )}
        </Box>
      )}

      <Stepper activeStep={activeStep} alternativeLabel>
        {steps.map((label, index) => (
          <Step key={label} completed={activeStep > index}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
        <Button onClick={handleBack} disabled={activeStep === 0}>
          Back
        </Button>
        {activeStep === steps.length - 1 ? (
          <Button onClick={handleFinish} variant="contained" color="primary">
            Finish
          </Button>
        ) : (
          <Button onClick={handleNext} variant="contained" color="primary">
            Next
          </Button>
        )}
      </Box>
    </Paper>
  )
}

export default LocationSetupWizard
