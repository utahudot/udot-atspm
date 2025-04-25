import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CloseIcon from '@mui/icons-material/Close'
import DoneIcon from '@mui/icons-material/Done'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Box, Chip, Collapse, Divider, Paper, Typography } from '@mui/material'
import { useState } from 'react'

interface Categories {
  foundPhases: string[]
  foundDetectors: string[]
  notFoundApproaches: string[]
  notFoundDetectors: string[]
}

interface ApproachesReconcilationReportProps {
  // Whether or not we've actually synced yet
  // If false, the component won't render anything
  synced: boolean

  categories: Categories
  syncedPhases: string[]
  syncedDetectors: string[]
}

export default function ApproachesReconcilationReport({
  synced,
  categories,
  syncedPhases,
  syncedDetectors,
}: ApproachesReconcilationReportProps) {
  const [open, setOpen] = useState(true)

  // If not synced, show nothing
  if (!synced) {
    return null
  }

  const isPhaseSynced = (phase: string) => syncedPhases.includes(phase)
  const isDetectorSynced = (detector: string) =>
    syncedDetectors.includes(detector)

  const handleToggle = () => {
    setOpen((prev) => !prev)
  }

  const renderCategory = (
    title: string,
    subtext: string,
    phases: string[],
    detectors: string[]
  ) => (
    <Box sx={{ mb: 2 }}>
      <Typography variant="h4" sx={{ mb: 1 }} fontWeight="bold">
        {title}
      </Typography>
      <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
        {subtext}:
      </Typography>

      <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
        Phases/Approaches
      </Typography>
      {phases.length === 0 && (
        <Typography
          variant="body2"
          color="success.main"
          sx={{
            mb: 1,
            display: 'flex',
            alignItems: 'center',
            fontSize: '0.875rem',
          }}
        >
          <CheckCircleIcon sx={{ fontSize: '1rem', mr: 0.5 }} />
          In Sync
        </Typography>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
        {phases.map((phase, index) => (
          <Chip
            key={index}
            label={phase}
            onDelete={() => null}
            deleteIcon={isPhaseSynced(phase) ? <DoneIcon /> : <CloseIcon />}
            sx={{
              fontSize: '0.875rem',
              bgcolor: isPhaseSynced(phase) ? 'success.light' : undefined,
            }}
          />
        ))}
      </Box>

      <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
        Detector Channels
      </Typography>
      {detectors.length === 0 && (
        <Typography
          variant="body2"
          color="success.main"
          sx={{
            mb: 1,
            display: 'flex',
            alignItems: 'center',
            fontSize: '0.875rem',
          }}
        >
          <CheckCircleIcon sx={{ fontSize: '1rem', mr: 0.5 }} />
          In Sync
        </Typography>
      )}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
        {detectors.map((detector, index) => (
          <Chip
            key={index}
            label={detector}
            onDelete={() => null}
            deleteIcon={
              isDetectorSynced(detector) ? <DoneIcon /> : <CloseIcon />
            }
            sx={{
              fontSize: '0.875rem',
              bgcolor: isDetectorSynced(detector) ? 'success.light' : undefined,
            }}
          />
        ))}
      </Box>
    </Box>
  )

  return (
    <Paper sx={{ p: 2, bgColor: 'primary.main.light' }}>
      <Box
        onClick={handleToggle}
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          cursor: 'pointer',
          mb: 1,
        }}
      >
        <Typography variant="h4" fontWeight="bold">
          Approaches Reconciliation Report
        </Typography>
        {open ? <ExpandLessIcon /> : <ExpandMoreIcon />}
      </Box>

      <Collapse in={open} timeout="auto" unmountOnExit>
        <Divider sx={{ my: 1 }} />
        {renderCategory(
          'Found',
          'Found data for the following phases/detector channels with no associated approach/detector',
          categories.foundPhases,
          categories.foundDetectors
        )}
        <Divider sx={{ my: 2 }} />
        {renderCategory(
          'Not Found',
          'No data was found for the following approaches/detectors',
          categories.notFoundApproaches,
          categories.notFoundDetectors
        )}
      </Collapse>
    </Paper>
  )
}
