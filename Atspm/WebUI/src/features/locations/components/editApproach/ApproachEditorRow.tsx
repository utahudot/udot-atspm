import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import DeleteIcon from '@mui/icons-material/Delete'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import SaveIcon from '@mui/icons-material/Save'
import {
  Alert,
  Box,
  ButtonBase,
  IconButton,
  Tooltip,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

interface ApproachEditorRowProps {
  approach: ConfigApproach
  open: boolean
  handleApproachClick: () => void
  handleCopyApproach: () => void
  handleSaveApproach: () => void
  openDeleteApproachModal: () => void
}

const ApproachEditorRowHeader = ({
  open,
  approach,
  handleApproachClick,
  handleCopyApproach,
  handleSaveApproach,
  openDeleteApproachModal,
}: ApproachEditorRowProps) => {
  const { errors, setErrors } = useLocationStore((s) => ({
    errors: s.errors,
    setErrors: s.setErrors,
  }))

  // track whether the user has tried saving
  const [saveAttempted, setSaveAttempted] = useState(false)

  // do we have any errors on this approach or its detectors?
  const hasApproachError = Boolean(errors?.[approach.id])
  const hasDetectorError = approach.detectors.some((det) =>
    Boolean(errors?.[det.id])
  )
  const hasAnyError = hasApproachError || hasDetectorError

  // if errors go away (because inputs auto-clear them), reset our "attempted" flag
  useEffect(() => {
    if (saveAttempted && !hasAnyError) {
      setSaveAttempted(false)
    }
  }, [hasAnyError, saveAttempted])

  // existing phase-number auto-clear logic stays as is
  useEffect(() => {
    if (!hasApproachError) return
    const { protectedPhaseNumber, permissivePhaseNumber, id } = approach
    const phaseValid =
      protectedPhaseNumber &&
      !(protectedPhaseNumber === 0 && permissivePhaseNumber)
    if (phaseValid) {
      const newErrors = { ...errors }
      delete newErrors[id]
      setErrors(Object.keys(newErrors).length ? newErrors : null)
    }
  }, [
    approach.protectedPhaseNumber,
    approach.permissivePhaseNumber,
    hasApproachError,
    errors,
    setErrors,
    approach,
  ])

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: 1,
          backgroundColor: approach.isNew
            ? 'rgba(100, 210, 100, 0.3)'
            : 'white',
        }}
      >
        <ButtonBase
          onClick={handleApproachClick}
          sx={{
            cursor: 'pointer',
            textTransform: 'none',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
          }}
        >
          <Box display="flex" alignItems="center">
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                transition: 'transform 0.2s ease-in-out',
                transform: open ? 'rotateZ(-180deg)' : 'rotateZ(0deg)',
              }}
            >
              <ExpandMoreIcon />
            </Box>
            <Typography
              variant="h4"
              component="h3"
              sx={{ padding: 1, marginRight: 2 }}
            >
              {approach.description}
            </Typography>
            <Typography variant="h5" component="p">
              {approach.detectors.length}{' '}
              {approach.detectors.length === 1 ? 'Detector' : 'Detectors'}
            </Typography>
          </Box>
        </ButtonBase>

        <Box display="flex" alignItems="center">
          <Tooltip title="Copy Approach">
            <IconButton aria-label="copy approach" onClick={handleCopyApproach}>
              <ContentCopyIcon />
            </IconButton>
          </Tooltip>

          <Tooltip title="Save Approach">
            <IconButton
              aria-label="save approach"
              color="success"
              onClick={() => {
                // mark that we tried to save, then run your handler
                setSaveAttempted(true)
                handleSaveApproach()
              }}
            >
              <SaveIcon />
            </IconButton>
          </Tooltip>

          <Tooltip title="Delete Approach">
            <IconButton
              aria-label="delete approach"
              color="error"
              onClick={openDeleteApproachModal}
            >
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {saveAttempted && hasAnyError && (
        <Box>
          <Alert severity="error">
            Please fix the highlighted errors before saving
          </Alert>
        </Box>
      )}
    </Box>
  )
}

export default ApproachEditorRowHeader
