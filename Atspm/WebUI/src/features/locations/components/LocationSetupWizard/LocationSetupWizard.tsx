import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { useNotificationStore } from '@/stores/notifications'
import { Box, Button, Paper, Step, StepLabel, Stepper } from '@mui/material'
import { useEffect, useState } from 'react'

const steps = [
  'Create Location through Template',
  'Configure Devices',
  'Reconcile Approaches and Detectors',
]

export default function LocationSetupWizard() {
  const { addNotification } = useNotificationStore()
  const { activeStep, setActiveStep } = useLocationWizardStore()

  const [isExiting, setIsExiting] = useState(false)
  const [isMounted, setIsMounted] = useState(false)
  const [isVisible, setIsVisible] = useState(true)

  useEffect(() => {
    requestAnimationFrame(() => {
      setIsMounted(true)
    })
  }, [])

  const handleNext = () => {
    setActiveStep(Math.min(activeStep + 1, steps.length - 1))
  }

  const handleBack = () => {
    setActiveStep(Math.max(activeStep - 1, 0))
  }

  const handleFinish = () => {
    addNotification({
      title: 'Location setup completed! 🎉',
      type: 'success',
    })
    setIsExiting(true)
  }

  const handleTransitionEnd = (e: React.TransitionEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget && isExiting) {
      setIsVisible(false)
    }
  }

  if (!isVisible) return null

  return (
    <Paper
      variant="outlined"
      onTransitionEnd={handleTransitionEnd}
      sx={{
        position: 'fixed',
        bottom: 10,
        left: '50%',
        transform: isExiting
          ? 'translate(-50%, 100%)'
          : isMounted
            ? 'translate(-50%, 0)'
            : 'translate(-50%, 100%)',
        transition: 'transform 0.3s ease-in-out',
        p: 2,
        zIndex: 1000,
        // minWidth: '1200px',
        bgcolor: 'background.default',
      }}
    >
      {/* Single row for buttons + stepper */}
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Button onClick={handleBack} disabled={activeStep === 0}>
          Back
        </Button>

        <Stepper
          activeStep={activeStep}
          alternativeLabel
          sx={{ flex: 1, mx: 2 }}
        >
          {steps.map((label, index) => (
            <Step key={label} completed={activeStep > index}>
              <StepLabel>{label}</StepLabel>
            </Step>
          ))}
        </Stepper>

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
