import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material'
import React from 'react'

type RunCheckProps = {
  data: {
    leftTurnVolumeOk: boolean
    gapOutOk: boolean
    pedCycleOk: boolean
    insufficientDetectorEventCount: boolean
    insufficientCycleAggregation: boolean
    insufficientPhaseTermination: boolean
    insufficientPedAggregations: boolean
    insufficientSplitFailAggregations: boolean
    insufficientLeftTurnGapAggregations: boolean
    approachId: number
    approachDescription: string
    locationIdentifier: string
    locationDescription: string
    start: string
    end: string
  }
}

const RunCheckGrid: React.FC<RunCheckProps> = ({ data }) => {
  if (!data || !Array.isArray(data)) {
    return null
  }
  const approaches = data.map((row) => ({
    approachDescription: row.approachDescription,
    leftTurnVolumeOk: row.leftTurnVolumeOk ? 'Check Detector' : 'OK',
    gapOutOk: row.gapOutOk ? 'Check Detector' : 'OK',
    pedCycleOk: row.pedCycleOk ? 'Review Crash History' : 'OK',
    insufficientDetectorEventCount: row.insufficientDetectorEventCount
      ? 'Check Detector'
      : 'OK',
    insufficientCycleAggregation: row.insufficientCycleAggregation
      ? 'Check Detector'
      : 'OK',
    insufficientPhaseTermination: row.insufficientPhaseTermination
      ? 'Check Detector'
      : 'OK',
    insufficientPedAggregations: row.insufficientPedAggregations
      ? 'Check Detector'
      : 'OK',
    insufficientSplitFailAggregations: row.insufficientSplitFailAggregations
      ? 'Check Detector'
      : 'OK',
    insufficientLeftTurnGapAggregations: row.insufficientLeftTurnGapAggregations
      ? 'Check Detector'
      : 'OK',
  }))

  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Approach Description</TableCell>
            <TableCell>Left Turn Volume OK</TableCell>
            <TableCell>Gap Out OK</TableCell>
            <TableCell>Ped Cycle OK</TableCell>
            <TableCell>Insufficient Detector Event Count</TableCell>
            <TableCell>Insufficient Cycle Aggregation</TableCell>
            <TableCell>Insufficient Phase Termination</TableCell>
            <TableCell>Insufficient Ped Aggregations</TableCell>
            <TableCell>Insufficient Split Fail Aggregations</TableCell>
            <TableCell>Insufficient Left Turn Gap Aggregations</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {approaches.map((approach, index) => (
            <TableRow key={index}>
              <TableCell>{approach.approachDescription}</TableCell>
              <TableCell>{approach.leftTurnVolumeOk}</TableCell>
              <TableCell>{approach.gapOutOk}</TableCell>
              <TableCell>{approach.pedCycleOk}</TableCell>
              <TableCell>{approach.insufficientDetectorEventCount}</TableCell>
              <TableCell>{approach.insufficientCycleAggregation}</TableCell>
              <TableCell>{approach.insufficientPhaseTermination}</TableCell>
              <TableCell>{approach.insufficientPedAggregations}</TableCell>
              <TableCell>
                {approach.insufficientSplitFailAggregations}
              </TableCell>
              <TableCell>
                {approach.insufficientLeftTurnGapAggregations}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default RunCheckGrid
