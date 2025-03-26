import { Detector } from '@/api/config/aTSPMConfigurationApi.schemas'
import EditableTableCell from '@/features/locations/components/editableTableCell/EditableTableCell'
import CommentCell from '@/features/locations/components/editDetector/CommentCell'
import DateAddedCell from '@/features/locations/components/editDetector/DateAddedCell'
import DetectionTypesCell from '@/features/locations/components/editDetector/DetectionTypesCell'
import HardwareTypeCell from '@/features/locations/components/editDetector/HardwareTypeCell'
import LaneTypeCell from '@/features/locations/components/editDetector/LaneTypeCell'
import MovementTypeCell from '@/features/locations/components/editDetector/MovementTypeCell'
import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { memo, useCallback, useState } from 'react'

function EditDetectors({ approach }: { approach: ConfigApproach }) {
  const approachErrors = useLocationStore((state) => {
    if (!state.errors) return {}
    const obj: Record<string, { error: string; id: string }> = {}
    approach.detectors.forEach((d) => {
      if (state.errors![String(d.id)]) {
        obj[String(d.id)] = state.errors![String(d.id)]
      }
    })
    return obj
  })

  const approachWarnings = useLocationStore((state) => {
    if (!state.warnings) return {}
    const obj: Record<string, { warning: string; id: string }> = {}
    approach.detectors.forEach((d) => {
      if (state.warnings![String(d.id)]) {
        obj[String(d.id)] = state.warnings![String(d.id)]
      }
    })
    return obj
  })

  const updateDetector = useLocationStore((s) => s.updateDetector)
  const deleteDetector = useLocationStore((s) => s.deleteDetector)

  const [modalOpen, setModalOpen] = useState(false)
  const [selectedDetectorId, setSelectedDetectorId] = useState<number>()

  const handleConfirmDelete = useCallback(() => {
    if (!selectedDetectorId) return
    deleteDetector(selectedDetectorId)
    setModalOpen(false)
  }, [selectedDetectorId, deleteDetector])

  return (
    <>
      <TableContainer component={Paper}>
        <Table stickyHeader>
          <TableHead>
            <TableRow key="header-info">
              <TableCell colSpan={9} />
              <TableCell colSpan={2} align="center">
                <Typography variant="caption" fontStyle="italic">
                  Advanced Count Only
                </Typography>
              </TableCell>
              <TableCell colSpan={2} align="center">
                <Typography variant="caption" fontStyle="italic">
                  Advanced Speed Only
                </Typography>
              </TableCell>
              <TableCell />
            </TableRow>
            <TableRow key="header-columns">
              <TableCell>Channel</TableCell>
              <TableCell>Detection Types</TableCell>
              <TableCell>Hardware</TableCell>
              <TableCell>Latency Correction</TableCell>
              <TableCell>Lane Number</TableCell>
              <TableCell>Movement Type</TableCell>
              <TableCell>Lane Type</TableCell>
              <TableCell>Date Added</TableCell>
              <TableCell>Comments</TableCell>
              <TableCell>Distance to Stop Bar</TableCell>
              <TableCell>Decision Point</TableCell>
              <TableCell>Minimum Speed Filter</TableCell>
              <TableCell>Movement Delay</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {approach.detectors.length === 0 && (
              <TableRow>
                <TableCell colSpan={15}>
                  <Typography variant="h6" align="center">
                    No detectors added
                  </Typography>
                </TableCell>
              </TableRow>
            )}
            {approach.detectors.map((det) => (
              <TableRow
                key={det.id}
                sx={{
                  backgroundColor: det.isNew
                    ? 'rgba(100, 210, 100, 0.3)'
                    : 'white',
                }}
              >
                <EditableTableCell
                  value={det.detectorChannel}
                  onUpdate={(val) =>
                    updateDetector(
                      det.id,
                      'detectorChannel',
                      parseInt(val as string)
                    )
                  }
                  error={approachErrors[String(det.id)]?.error}
                  warning={approachWarnings[String(det.id)]?.warning}
                />
                <DetectionTypesCell
                  detector={det as Detector}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'detectionTypes', val)
                  }
                />
                <HardwareTypeCell
                  value={det.detectionHardware}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'detectionHardware', val)
                  }
                />
                <EditableTableCell
                  value={det.latencyCorrection}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'latencyCorrection', val)
                  }
                />
                <EditableTableCell
                  value={det.laneNumber}
                  onUpdate={(val) => updateDetector(det.id, 'laneNumber', val)}
                />
                <MovementTypeCell
                  value={det.movementType}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'movementType', val)
                  }
                />
                <LaneTypeCell
                  value={det.laneType}
                  onUpdate={(val) => updateDetector(det.id, 'laneType', val)}
                />
                <DateAddedCell
                  value={det.dateAdded}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'dateAdded', val.toISOString())
                  }
                />
                <CommentCell detector={det as Detector} />
                <EditableTableCell
                  value={det.distanceFromStopBar}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'distanceFromStopBar', val)
                  }
                />
                <EditableTableCell
                  value={det.decisionPoint}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'decisionPoint', val)
                  }
                />
                <EditableTableCell
                  value={det.minSpeedFilter}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'minSpeedFilter', val)
                  }
                />
                <EditableTableCell
                  value={det.movementDelay}
                  onUpdate={(val) =>
                    updateDetector(det.id, 'movementDelay', val)
                  }
                />
                <TableCell align="center">
                  <IconButton
                    color="error"
                    onClick={() => {
                      setSelectedDetectorId(det.id)
                      setModalOpen(true)
                    }}
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={modalOpen} onClose={() => setModalOpen(false)}>
        <DialogTitle sx={{ fontWeight: 'bold' }}>Delete Detector</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this detector?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setModalOpen(false)} variant="outlined">
            Cancel
          </Button>
          <Button
            onClick={handleConfirmDelete}
            variant="contained"
            color="error"
          >
            Delete Detector
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default memo(EditDetectors)
