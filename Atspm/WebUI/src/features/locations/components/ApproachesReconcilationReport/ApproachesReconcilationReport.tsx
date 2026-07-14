import { dk } from '@/features/locations/components/ApproachesReconcilationReport/discrepancyKeys'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import { Box, Collapse, Divider, Paper, Typography } from '@mui/material'
import { useMemo, useState } from 'react'
import type { DiscrepancyItem } from './DiscrepancyRow'
import FoundDetectorChannelsSection from './FoundDetectorChannelsSection'
import FoundPhasesSection from './FoundPhasesSection'
import NotFoundSection from './NotFoundSection'
import useDiscrepancyStatuses, {
  LocationDiscrepancyReport,
} from './useDiscrepancyStatuses'

interface ApproachesReconcilationReportProps {
  categories: LocationDiscrepancyReport
}

export default function ApproachesReconcilationReport({
  categories,
}: ApproachesReconcilationReportProps) {
  const {
    approaches,
    deleteApproach,
    deleteDetector,
    addApproach,
    addDetector,
  } = useLocationStore()

  const { itemStatuses, updateStatus } = useDiscrepancyStatuses(
    categories,
    approaches
  )

  const [open, setOpen] = useState(true)
  const toggle = () => setOpen(!open)

  const [selectedNotFound, setSelectedNotFound] = useState<
    Record<string, boolean>
  >({})
  const [selectedFoundPhases, setSelectedFoundPhases] = useState<
    Record<string, boolean>
  >({})
  const [selectedFoundDetectors, setSelectedFoundDetectors] = useState<
    Record<string, boolean>
  >({})

  const foundPhases: DiscrepancyItem[] = useMemo(
    () =>
      (categories.foundPhaseNumbers ?? []).map((phase) => ({
        kind: 'FOUND_PHASE',
        id: dk('FOUND_PHASE', phase),
        label: phase,
      })),
    [categories.foundPhaseNumbers]
  )

  const foundDetectors: DiscrepancyItem[] = useMemo(
    () =>
      (categories.foundDetectorChannels ?? []).map((det) => ({
        kind: 'FOUND_DET',
        id: dk('FOUND_DET', det),
        label: det,
      })),
    [categories.foundDetectorChannels]
  )

  const notFoundApproaches: DiscrepancyItem[] = useMemo(
    () =>
      (categories.notFoundApproaches ?? []).map((a) => ({
        kind: 'NOT_FOUND_APP',
        id: dk('NOT_FOUND_APP', a.id),
        label: a.description || `${a.id}`,
      })),
    [categories.notFoundApproaches]
  )

  const notFoundDetectors: DiscrepancyItem[] = useMemo(
    () =>
      (categories.notFoundDetectorChannels ?? []).map((det) => ({
        kind: 'NOT_FOUND_DET',
        id: dk('NOT_FOUND_DET', det),
        label: det,
      })),
    [categories.notFoundDetectorChannels]
  )

  return (
    <Paper sx={{ p: 2, my: 2 }}>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        sx={{ cursor: 'pointer' }}
        onClick={toggle}
      >
        <Typography variant="h4">Approaches Reconciliation Report</Typography>
        {open ? <ExpandLessIcon /> : <ExpandMoreIcon />}
      </Box>

      <Collapse in={open}>
        <Divider sx={{ my: 2 }} />

        <FoundPhasesSection
          items={foundPhases}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedFoundPhases}
          setSelected={setSelectedFoundPhases}
          addApproach={addApproach}
        />

        <FoundDetectorChannelsSection
          key={foundDetectors.map((d) => d.id).join('-')}
          items={foundDetectors}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedFoundDetectors}
          setSelected={setSelectedFoundDetectors}
          approaches={approaches}
          addDetector={addDetector}
        />

        <Divider sx={{ my: 2 }} />

        <NotFoundSection
          approaches={approaches}
          notFoundApproaches={notFoundApproaches}
          notFoundDetectors={notFoundDetectors}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedNotFound}
          setSelected={setSelectedNotFound}
          deleteApproach={deleteApproach}
          deleteDetector={deleteDetector}
        />
      </Collapse>
    </Paper>
  )
}
