import { dk } from '@/features/locations/components/ApproachesReconcilationReport/discrepancyKeys'
import { type LocationStore } from '@/features/locations/components/editLocation/locationStore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Alert,
  Box,
  ButtonBase,
  Collapse,
  Divider,
  Paper,
  Typography,
} from '@mui/material'
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

/** Shared report UI; the mock editor supplies local actions instead of persistence. */
export function ApproachesReconciliationView({
  categories,
  approaches,
  deleteApproach,
  updateApproach,
  deleteDetector,
  addApproach,
  addDetector,
  onIgnore,
  detectionTypes = [],
  locationTypeName,
  pedsAre1to1 = false,
  errors,
}: ApproachesReconcilationReportProps &
  Pick<
    LocationStore,
    | 'approaches'
    | 'deleteApproach'
    | 'updateApproach'
    | 'deleteDetector'
    | 'addApproach'
    | 'addDetector'
  > & {
    errors?: LocationStore['errors']
    onIgnore?: () => void
    detectionTypes?: import('@/features/locations/types').DetectionType[]
    locationTypeName?: string
    pedsAre1to1?: boolean
  }) {
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
  const [selectedFoundOverlaps, setSelectedFoundOverlaps] = useState<
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

  const foundOverlaps: DiscrepancyItem[] = useMemo(
    () =>
      (categories.foundOverlapNumbers ?? []).map((phase) => ({
        kind: 'FOUND_OVERLAP',
        id: `FOUND_OVERLAP:${phase}`,
        label: phase,
      })),
    [categories.foundOverlapNumbers]
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
      <ButtonBase
        aria-expanded={open}
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          width: '100%',
          textAlign: 'left',
        }}
        onClick={toggle}
      >
        <Typography variant="h4">Approaches Reconciliation Report</Typography>
        {open ? <ExpandLessIcon /> : <ExpandMoreIcon />}
      </ButtonBase>

      <Collapse in={open}>
        <Divider sx={{ my: 2 }} />
        {categories.warnings?.map((warning) => (
          <Alert severity="warning" key={warning} sx={{ mb: 2 }}>
            {warning}
          </Alert>
        ))}

        <Box sx={{ mb: 2 }}>
          <Typography variant="h4" fontWeight="bold" sx={{ mb: 1 }}>
            Found
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Data was found for the following unconfigured items
          </Typography>
        </Box>

        <FoundPhasesSection
          items={foundPhases}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedFoundPhases}
          setSelected={setSelectedFoundPhases}
          pedsAre1to1={pedsAre1to1}
          addApproach={(phase, properties) =>
            addApproach(phase, false, properties)
          }
        />

        <FoundPhasesSection
          title="Overlaps"
          approaches={approaches}
          updateApproach={updateApproach}
          items={foundOverlaps}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedFoundOverlaps}
          setSelected={setSelectedFoundOverlaps}
          isOverlap
          pedsAre1to1={pedsAre1to1}
          addApproach={(phase, properties) =>
            addApproach(phase, true, properties)
          }
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
          detectionTypes={detectionTypes}
          locationTypeName={locationTypeName}
        />

        <Divider sx={{ my: 2 }} />

        <NotFoundSection
          errors={errors}
          pedsAre1to1={pedsAre1to1}
          approaches={approaches}
          notFoundApproaches={notFoundApproaches}
          missingAssignments={categories.missingPhaseAssignments ?? []}
          updateApproach={updateApproach}
          notFoundDetectors={notFoundDetectors}
          itemStatuses={itemStatuses}
          updateStatus={updateStatus}
          selected={selectedNotFound}
          setSelected={setSelectedNotFound}
          deleteApproach={deleteApproach}
          deleteDetector={deleteDetector}
          onIgnore={onIgnore}
        />
      </Collapse>
    </Paper>
  )
}
