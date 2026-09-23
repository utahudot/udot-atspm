import {
  Entity,
  Segment,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { DataSource } from '@/features/speedManagementTool/enums'
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
} from '@mui/material'

interface HoveredEntityProps {
  hoveredEntity: Segment | Entity | null
}

const HoveredEntity = ({ hoveredEntity }: HoveredEntityProps) => {
  if (!hoveredEntity) return null

  const isSegment =
    hoveredEntity && hoveredEntity?.geometry?.type === 'LineString'

  return isSegment ? (
    <Paper
      elevation={3}
      sx={{
        position: 'absolute',
        bottom: '20px',
        left: '20px',
        padding: '8px 12px',
        backgroundColor: 'rgba(255, 255, 255, 0.9)',
        zIndex: 1000,
        pointerEvents: 'none',
      }}
    >
      <TableContainer>
        <Table size="small">
          <TableBody>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Segment Name:</TableCell>
              <TableCell align="right">
                {hoveredEntity.properties.name}
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Speed Limit:</TableCell>
              <TableCell align="right">
                {hoveredEntity.properties.speedLimit} mph
              </TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  ) : (
    <Paper
      elevation={3}
      sx={{
        position: 'absolute',
        bottom: '20px',
        left: '20px',
        padding: '8px 12px',
        backgroundColor: 'rgba(255, 255, 255, 0.9)',
        zIndex: 1000,
        pointerEvents: 'none',
      }}
    >
      <TableContainer>
        <Table size="small">
          <TableBody>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Entity Source:</TableCell>
              <TableCell align="right">
                {DataSource[hoveredEntity.sourceId]}
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Entity Id:</TableCell>
              <TableCell align="right">{hoveredEntity.entityId}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Version:</TableCell>
              <TableCell align="right">{hoveredEntity.version}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Entity Type:</TableCell>
              <TableCell align="right">{hoveredEntity.entityType}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Direction:</TableCell>
              <TableCell align="right">{hoveredEntity.direction}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Start Date:</TableCell>
              <TableCell align="right">{hoveredEntity.startDate}</TableCell>
            </TableRow>
            {hoveredEntity.sourceId === DataSource.ClearGuide && (
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>Active:</TableCell>
                <TableCell align="right">{hoveredEntity.active}</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  )
}
export default HoveredEntity
