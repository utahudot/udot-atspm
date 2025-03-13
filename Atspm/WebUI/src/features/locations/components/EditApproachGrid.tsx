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
import React from 'react'

const StyledTC = ({
  children,
  width = '100px',
}: {
  children: React.ReactNode
  width?: string
}) => <TableCell sx={{ minWidth: width }}>{children}</TableCell>

export default function EditApproachGrid({
  approach,
}: {
  approach: ConfigApproach
}) {
  const { location, errors, updateApproach } = useLocationStore()
  const pedsAre1to1 = location?.pedsAre1to1 ?? false

  const handleUpdate = (
    field: keyof ConfigApproach,
    value: string | number | boolean
  ) => {
    updateApproach({ ...approach, [field]: value })
  }

  return (
    <TableContainer component={Paper}>
      <Table aria-label="edit approach table">
        <TableHead
          sx={{
            '& .MuiTableCell-head': {
              fontSize: '0.8rem',
              bgcolor: 'white',
              lineHeight: '1rem',
              padding: '0.5rem',
            },
          }}
        >
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
            <StyledTC>Direction</StyledTC>
            <StyledTC>Description</StyledTC>
            <StyledTC>Protected</StyledTC>
            <StyledTC>Permissive</StyledTC>
            <StyledTC>Pedestrian</StyledTC>
            <StyledTC>Pedestrian Detectors</StyledTC>
            <StyledTC>Approach Speed (mph)</StyledTC>
            <StyledTC>Protected</StyledTC>
            <StyledTC>Permissive</StyledTC>
            <StyledTC>Pedestrian</StyledTC>
          </TableRow>
        </TableHead>
        <TableBody>
          <TableRow
            sx={{
              '& .MuiTableCell-body': {
                fontSize: '0.9rem',
                borderRight: '1px solid #e0e0e0',
              },
            }}
          >
            <DirectionTypeCell
              value={approach.directionTypeId}
              onUpdate={(v) => handleUpdate('directionTypeId', v)}
            />
            <EditableTableCell
              value={approach.description}
              onUpdate={(v) => handleUpdate('description', v)}
            />
            <EditableTableCell
              value={approach.protectedPhaseNumber}
              onUpdate={(v) => handleUpdate('protectedPhaseNumber', v)}
              error={errors?.protectedPhaseNumber?.error}
            />
            <EditableTableCell
              value={approach.permissivePhaseNumber}
              onUpdate={(v) => handleUpdate('permissivePhaseNumber', v)}
            />
            <EditableTableCell
              value={approach.pedestrianPhaseNumber}
              onUpdate={(v) => handleUpdate('pedestrianPhaseNumber', v)}
              lockable
              isLocked={pedsAre1to1}
            />
            <EditableTableCell
              value={approach.pedestrianDetectors?.toString() || ''}
              onUpdate={(v) =>
                handleUpdate('pedestrianDetectors', v?.toString())
              }
              lockable
              isLocked={pedsAre1to1}
            />
            <EditableTableCell
              value={approach.mph}
              onUpdate={(v) => handleUpdate('mph', v)}
            />
            <EditableTableCell
              value={approach.isProtectedPhaseOverlap}
              onUpdate={(v) => handleUpdate('isProtectedPhaseOverlap', v)}
            />
            <EditableTableCell
              value={approach.isPermissivePhaseOverlap}
              onUpdate={(v) => handleUpdate('isPermissivePhaseOverlap', v)}
            />
            <EditableTableCell
              value={approach.isPedestrianPhaseOverlap}
              onUpdate={(v) => handleUpdate('isPedestrianPhaseOverlap', v)}
            />
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  )
}
