import { useGetDirectionTypes } from '@/features/locations/api'
import DirectionTypeCell from '@/features/locations/components/editApproach/DirectionTypeCell'
import EditableTableCell from '@/features/locations/components/editableTableCell'
import { LocationExpanded } from '@/features/locations/types'
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
import { ApproachForConfig } from './editLocation/editLocationConfigHandler'

const StyledTC = ({
  children,
  width = '100px',
}: {
  children: React.ReactNode
  width?: string
}) => <TableCell sx={{ minWidth: width }}>{children}</TableCell>

interface EditApproachGridProps {
  approach: ApproachForConfig
  approaches: ApproachForConfig[]
  location: LocationExpanded
  updateApproach: (approach: ApproachForConfig) => void
  updateApproaches: (approaches: ApproachForConfig[]) => void
}

const EditApproachGrid = ({
  approach,
  location,
  updateApproach,
}: EditApproachGridProps) => {
  const { data: directionTypesData } = useGetDirectionTypes()

  const { pedsAre1to1 } = location
  const directionTypes = directionTypesData?.value

  if (!directionTypes) return null

  const handleUpdate = (field: string, value: any) => {
    const updatedApproach = { ...approach, [field]: value }
    updateApproach(updatedApproach)
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
            <TableCell
              colSpan={2}
              sx={{ borderBottom: 'none', pb: 0 }}
              component={'td'}
            />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0 }}
            >
              <Divider>
                <Typography variant="caption" fontStyle={'italic'}>
                  Phase/Overlap
                </Typography>
              </Divider>
            </TableCell>
            <TableCell
              colSpan={2}
              sx={{ borderBottom: 'none', pb: 0 }}
              component={'td'}
            />
            <TableCell
              colSpan={3}
              align="center"
              sx={{ borderBottom: 'none', pb: 0 }}
            >
              <Divider>
                <Typography variant="caption" fontStyle={'italic'}>
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
              value={approach?.directionTypeId}
              onUpdate={(value) => handleUpdate('directionTypeId', value)}
            />
            <EditableTableCell
              value={approach.description}
              onUpdate={(value) => handleUpdate('description', value)}
            />
            <EditableTableCell
              value={approach.protectedPhaseNumber}
              onUpdate={(value) => handleUpdate('protectedPhaseNumber', value)}
            />
            <EditableTableCell
              value={approach.permissivePhaseNumber}
              onUpdate={(value) => handleUpdate('permissivePhaseNumber', value)}
            />
            <EditableTableCell
              value={approach.pedestrianPhaseNumber}
              onUpdate={(value) => handleUpdate('pedestrianPhaseNumber', value)}
              lockable={true}
              isLocked={pedsAre1to1}
            />
            <EditableTableCell
              value={
                approach.pedestrianDetectors &&
                approach.pedestrianDetectors.toString()
              }
              onUpdate={(value) =>
                handleUpdate('pedestrianDetectors', value?.toString())
              }
              lockable={true}
              isLocked={pedsAre1to1}
            />
            <EditableTableCell
              value={approach.mph}
              onUpdate={(value) => handleUpdate('mph', value)}
            />
            <EditableTableCell
              value={approach.isProtectedPhaseOverlap}
              onUpdate={(value) =>
                handleUpdate('isProtectedPhaseOverlap', value)
              }
            />
            <EditableTableCell
              value={approach.isPermissivePhaseOverlap}
              onUpdate={(value) =>
                handleUpdate('isPermissivePhaseOverlap', value)
              }
            />
            <EditableTableCell
              value={approach.isPedestrianPhaseOverlap}
              onUpdate={(value) =>
                handleUpdate('isPedestrianPhaseOverlap', value)
              }
            />
          </TableRow>
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default EditApproachGrid
