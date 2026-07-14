import { useLocationWizardStore } from '@/features/locations/components/LocationSetupWizard/locationSetupWizardStore'
import { useNotificationStore } from '@/stores/notifications'
import CloseIcon from '@mui/icons-material/Close'
import CropSquareIcon from '@mui/icons-material/CropSquare'
import MinimizeIcon from '@mui/icons-material/Remove'
import {
  Box,
  Button,
  IconButton,
  Step,
  StepContent,
  StepLabel,
  Stepper,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

interface LocationSetupWizardProps {
  open?: boolean
  onClose?: () => void
}

const steps = [
  {
    label: 'Verify Devices',
    description:
      'Test device IPs and ensure data is being downloaded successfully.',
  },
  {
    label: 'Reconcile Detectors & Approaches',
    description:
      'Compare discovered data with your current configuration to ensure no missing or extra detectors/approaches.',
  },
]

export default function LocationSetupWizard({
  open = true,
  onClose,
}: LocationSetupWizardProps) {
  const { addNotification } = useNotificationStore()
  const {
    activeStep,
    setActiveStep,
    setDeviceVerificationStatus,
    setApproachVerificationStatus,
    resetStore,
    setUseWizard,
  } = useLocationWizardStore()

  const [isMinimized, setIsMinimized] = useState(false)
  const [isClosed, setIsClosed] = useState(!open)

  useEffect(() => {
    setIsClosed(!open)
  }, [open])

  const closeWizard = () => {
    setIsClosed(true)
    setUseWizard(false)
    onClose?.()
  }

  const handleFinish = () => {
    addNotification({
      title: 'Location setup completed',
      type: 'success',
    })
    resetStore()
    closeWizard()
  }

  const handleNextStep = () => {
    setActiveStep(activeStep + 1)
  }

  const handlePrevStep = () => {
    if (activeStep > 0) {
      setActiveStep(activeStep - 1)
    }
  }

  const handleVerifyDevices = () => {
    setActiveStep(0)
    setDeviceVerificationStatus('READY_TO_RUN')
  }

  const handleReconcileApproaches = () => {
    setActiveStep(1)
    setApproachVerificationStatus('READY_TO_RUN')
  }

  if (isClosed) return null

  return (
    <Box
      sx={{
        position: 'fixed',
        bottom: 20,
        right: 20,
        width: 370,
        zIndex: 1300,
        maxHeight: '80vh',
        overflowY: 'auto',
        bgcolor: 'background.paper',
        p: 2,
        borderRadius: 2,
        boxShadow: 3,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Typography variant="h6">Guided Setup</Typography>
        <Box>
          <IconButton
            onClick={() => setIsMinimized((prev) => !prev)}
            size="small"
            aria-label="minimize"
          >
            {isMinimized ? (
              <CropSquareIcon fontSize="small" />
            ) : (
              <MinimizeIcon fontSize="small" />
            )}
          </IconButton>
          <IconButton onClick={closeWizard} size="small" aria-label="close">
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>
      </Box>

      {!isMinimized && (
        <Stepper activeStep={activeStep} orientation="vertical">
          {steps.map((step, index) => (
            <Step key={step.label} completed={activeStep > index}>
              <StepLabel>{step.label}</StepLabel>
              <StepContent>
                <Typography
                  variant="body2"
                  sx={{ fontSize: '.85rem' }}
                  color="text.secondary"
                >
                  {step.description}
                </Typography>

                <Box sx={{ mt: 2, display: 'flex', gap: 1 }}>
                  {index === 0 && (
                    <>
                      <Button
                        variant="contained"
                        onClick={handleVerifyDevices}
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.8rem',
                        }}
                      >
                        Run Verification
                      </Button>
                      <Button
                        variant="outlined"
                        onClick={handleNextStep}
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.8rem',
                        }}
                      >
                        Next
                      </Button>
                    </>
                  )}

                  {index === 1 && (
                    <>
                      <Button
                        variant="outlined"
                        onClick={handlePrevStep}
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.8rem',
                        }}
                      >
                        Back
                      </Button>
                      <Button
                        variant="contained"
                        onClick={handleReconcileApproaches}
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.8rem',
                        }}
                      >
                        Run Reconciliation
                      </Button>
                      <Button
                        variant="outlined"
                        onClick={handleFinish}
                        sx={{
                          textTransform: 'none',
                          fontSize: '0.8rem',
                        }}
                      >
                        Finish
                      </Button>
                    </>
                  )}
                </Box>
              </StepContent>
            </Step>
          ))}
        </Stepper>
      )}
    </Box>
  )
}
