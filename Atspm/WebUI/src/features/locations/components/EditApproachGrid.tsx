import DirectionTypeCell from '@/features/locations/components/editApproach/DirectionTypeCell'
import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
import EditableTableCell from '@/features/locations/components/editableTableCell'
import {
  Divider,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { memo, useCallback } from 'react'

interface EditApproachGridProps {
  approach: ConfigApproach
}

function EditApproachGrid({ approach }: EditApproachGridProps) {
  /**
   * Pull all needed state/functions from our single zustand store.
   * - `location` so we can read top-level flags (like `pedsAre1to1`)
   * - `errors` if we’re showing approach-level errors
   * - `updateApproach` to save changes to this approach
   */
  const { location, errors, updateApproach } = useLocationStore((state) => ({
    location: state.location,
    errors: state.errors,
    updateApproach: state.updateApproach,
  }))

  const pedsAre1to1 = location?.pedsAre1to1 ?? false

  const handleUpdate = useCallback(
    (field: keyof ConfigApproach, value: unknown) => {
      updateApproach({ ...approach, [field]: value })
    },
    [approach, updateApproach]
  )

  return (
    <TableContainer component={Paper}>
      <Table aria-label="edit approach table">
        <TableHead>
          {/* Row with "Phase/Overlap" and "Overlap" headings */}
          <TableRow>
            <TableCell colSpan={2} sx={{ borderBottom: 'none', pb: 0 }} />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0 }}
            >
              <Divider>
                <Typography variant="caption" fontStyle="italic">
                  Phase/Overlap
                </Typography>
              </Divider>
            </TableCell>
            <TableCell colSpan={2} sx={{ borderBottom: 'none', pb: 0 }} />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0 }}
            >
              <Divider>
                <Typography variant="caption" fontStyle="italic">
                  Overlap
                </Typography>
              </Divider>
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell sx={{ minWidth: '100px' }}>Direction</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Description</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Protected</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Permissive</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Pedestrian</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>
              Pedestrian Detectors
            </TableCell>
            <TableCell sx={{ minWidth: '100px' }}>
              Approach Speed (mph)
            </TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Protected</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Permissive</TableCell>
            <TableCell sx={{ minWidth: '100px' }}>Pedestrian</TableCell>
          </TableRow>
        </TableHead>

        <TableBody>
          <TableRow>
            {/* Direction Cell */}
            <DirectionTypeCell
              value={approach.directionTypeId}
              onUpdate={(val) => handleUpdate('directionTypeId', val)}
            />

            {/* Description */}
            <EditableTableCell
              value={approach.description}
              onUpdate={(val) => handleUpdate('description', val)}
            />

            {/* Protected Phase */}
            <EditableTableCell
              value={approach.protectedPhaseNumber}
              onUpdate={(val) => handleUpdate('protectedPhaseNumber', val)}
              error={errors?.protectedPhaseNumber?.error}
            />

            {/* Permissive Phase */}
            <EditableTableCell
              value={approach.permissivePhaseNumber}
              onUpdate={(val) => handleUpdate('permissivePhaseNumber', val)}
            />

            {/* Ped Phase */}
            <EditableTableCell
              value={approach.pedestrianPhaseNumber}
              onUpdate={(val) => handleUpdate('pedestrianPhaseNumber', val)}
              lockable
              isLocked={pedsAre1to1}
            />

            {/* Ped Detectors */}
            <EditableTableCell
              value={approach.pedestrianDetectors?.toString() || ''}
              onUpdate={(val) =>
                handleUpdate('pedestrianDetectors', val?.toString())
              }
              lockable
              isLocked={pedsAre1to1}
            />

            {/* Approach Speed */}
            <EditableTableCell
              value={approach.mph}
              onUpdate={(val) => handleUpdate('mph', val)}
            />

            {/* Overlaps */}
            <EditableTableCell
              value={approach.isProtectedPhaseOverlap}
              onUpdate={(val) => handleUpdate('isProtectedPhaseOverlap', val)}
            />
            <EditableTableCell
              value={approach.isPermissivePhaseOverlap}
              onUpdate={(val) => handleUpdate('isPermissivePhaseOverlap', val)}
            />
            <EditableTableCell
              value={approach.isPedestrianPhaseOverlap}
              onUpdate={(val) => handleUpdate('isPedestrianPhaseOverlap', val)}
            />
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default memo(EditApproachGrid)
