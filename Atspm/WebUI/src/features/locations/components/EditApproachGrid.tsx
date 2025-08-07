import BooleanCell from '@/features/locations/components/Cell/BooleanCell'
import SelectCell from '@/features/locations/components/Cell/SelectCell'
import { TextCell } from '@/features/locations/components/Cell/TextCell'
import { directionTypeOptions } from '@/features/locations/components/editDetector/selectOptions'
import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
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

  const rowCount = approach.detectors?.length + 1 || 1
  const colCount = pedsAre1to1 ? 8 : 10

  let columnCount = 0
  const getColumnCount = () => columnCount++

  return (
    <TableContainer component={Paper}>
      <Table stickyHeader aria-label="edit approach table">
        <TableHead>
          <TableRow>
            <TableCell
              colSpan={2}
              sx={{ borderBottom: 'none', pb: 0, pt: 1 }}
            />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0, pt: 1 }}
            >
              <Divider>
                <Typography
                  variant="caption"
                  fontStyle="italic"
                  fontSize="12px"
                >
                  Phase/Overlap
                </Typography>
              </Divider>
            </TableCell>
            <TableCell
              colSpan={2}
              sx={{ borderBottom: 'none', pb: 0, pt: 1 }}
            />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0, pt: 1 }}
            >
              <Divider>
                <Typography variant="caption" fontStyle="italic" fontSize={12}>
                  Overlap
                </Typography>
              </Divider>
            </TableCell>
          </TableRow>
          <TableRow>
            {[
              'Direction',
              'Description',
              'Protected',
              'Permissive',
              'Pedestrian',
              'Pedestrian Detectors',
              'Approach Speed (mph)',
              'Protected',
              'Permissive',
              'Pedestrian',
            ].map((header, i) => (
              <TableCell
                key={i}
                sx={{
                  py: 1,
                  fontSize: '12px',
                  minWidth: '100px',
                  maxWidth:
                    header === 'Protected' || header === 'Permissive'
                      ? '40px'
                      : '100px',
                }}
              >
                {header}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>

        <TableBody>
          <TableRow>
            <SelectCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              options={directionTypeOptions}
              value={approach.directionTypeId}
              onUpdate={(v) => handleUpdate('directionTypeId', v)}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.description}
              onUpdate={(v) => handleUpdate('description', v)}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.protectedPhaseNumber}
              onUpdate={(v) => handleUpdate('protectedPhaseNumber', v)}
              error={errors?.[approach.id]}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={3}
              rowCount={rowCount}
              colCount={getColumnCount()}
              value={approach.permissivePhaseNumber}
              onUpdate={(v) => handleUpdate('permissivePhaseNumber', v)}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={4}
              rowCount={rowCount}
              colCount={pedsAre1to1 ? null : getColumnCount()}
              value={
                (approach.pedestrianPhaseNumber ?? pedsAre1to1)
                  ? approach.protectedPhaseNumber
                  : ''
              }
              onUpdate={(v) => handleUpdate('pedestrianPhaseNumber', v)}
              disabled={pedsAre1to1}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={5}
              rowCount={rowCount}
              colCount={pedsAre1to1 ? null : getColumnCount()}
              value={
                approach.pedestrianDetectors
                  ? approach.pedestrianDetectors.toString()
                  : pedsAre1to1
                    ? approach.protectedPhaseNumber
                    : ''
              }
              onUpdate={(v) =>
                handleUpdate('pedestrianDetectors', v?.toString())
              }
              disabled={pedsAre1to1}
            />

            <TextCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.mph}
              onUpdate={(v) => handleUpdate('mph', v)}
            />

            <BooleanCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.isProtectedPhaseOverlap}
              onUpdate={(v) => handleUpdate('isProtectedPhaseOverlap', v)}
            />

            <BooleanCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.isPermissivePhaseOverlap}
              onUpdate={(v) => handleUpdate('isPermissivePhaseOverlap', v)}
            />

            <BooleanCell
              approachId={approach.id}
              row={0}
              col={getColumnCount()}
              rowCount={rowCount}
              colCount={colCount}
              value={approach.isPedestrianPhaseOverlap}
              onUpdate={(v) => handleUpdate('isPedestrianPhaseOverlap', v)}
            />
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default memo(EditApproachGrid)
