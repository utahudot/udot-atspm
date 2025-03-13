import { Detector } from '@/api/config/aTSPMConfigurationApi.schemas'
import { modalButtonLocation } from '@/components/GenericAdminChart'
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
import EditableTableCell from '@/features/locations/components/editableTableCell/EditableTableCell'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  Box,
  Button,
  Divider,
  IconButton,
  Modal,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { useState } from 'react'

export const modalStyle = {
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: 'none',
  borderRadius: '10px',
  boxShadow: 24,
}

export default function EditDetectors({
  approach,
}: {
  approach: ConfigApproach
}) {
  const { errors, warnings, updateDetector, deleteDetector } =
    useLocationStore()
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedDetectorId, setSelectedDetectorId] = useState<number>()

  return (
    <>
      <TableContainer component={Paper}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
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
            </TableRow>
            <TableRow>
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
            {approach.detectors.map((detector) => (
              <TableRow
                key={detector.id}
                sx={{
                  backgroundColor: detector.isNew
                    ? 'rgba(100, 210, 100, 0.3)'
                    : 'white',
                }}
              >
                <EditableTableCell
                  value={detector.detectorChannel}
                  onUpdate={(newVal) =>
                    updateDetector(
                      detector.id,
                      'detectorChannel',
                      parseInt(newVal as string)
                    )
                  }
                  error={errors?.[String(detector.id)]?.error}
                  warning={warnings?.[String(detector.id)]?.warning}
                />
                <DetectionTypesCell
                  detector={detector as Detector}
                  onUpdate={(newSelection) =>
                    updateDetector(detector.id, 'detectionTypes', newSelection)
                  }
                />
                <HardwareTypeCell
                  value={detector.detectionHardware}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'detectionHardware', newVal)
                  }
                />
                <EditableTableCell
                  value={detector.latencyCorrection}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'latencyCorrection', newVal)
                  }
                />
                <EditableTableCell
                  value={detector.laneNumber}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'laneNumber', newVal)
                  }
                />
                <MovementTypeCell
                  value={detector.movementType}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'movementType', newVal)
                  }
                />
                <LaneTypeCell
                  value={detector.laneType}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'laneType', newVal)
                  }
                />
                <DateAddedCell
                  value={detector.dateAdded}
                  onUpdate={(newVal) =>
                    updateDetector(
                      detector.id,
                      'dateAdded',
                      newVal.toISOString()
                    )
                  }
                />
                <CommentCell detector={detector as Detector} />
                <EditableTableCell
                  value={detector.distanceFromStopBar}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'distanceFromStopBar', newVal)
                  }
                />
                <EditableTableCell
                  value={detector.decisionPoint}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'decisionPoint', newVal)
                  }
                />
                <EditableTableCell
                  value={detector.minSpeedFilter}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'minSpeedFilter', newVal)
                  }
                />
                <EditableTableCell
                  value={detector.movementDelay}
                  onUpdate={(newVal) =>
                    updateDetector(detector.id, 'movementDelay', newVal)
                  }
                />
                <TableCell align="center">
                  <IconButton
                    color="error"
                    onClick={() => {
                      setSelectedDetectorId(detector.id)
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

      {selectedDetectorId && (
        <Modal open={modalOpen} onClose={() => setModalOpen(false)}>
          <Box sx={modalStyle}>
            <Typography fontWeight="bold">Delete Detector</Typography>
            <Divider sx={{ my: 2, backgroundColor: 'gray' }} />
            <Typography>
              Are you sure you want to delete this detector?
            </Typography>
            <Box sx={modalButtonLocation}>
              <Button onClick={() => setModalOpen(false)}>Cancel</Button>
              <Button
                sx={{ color: 'red' }}
                onClick={() => {
                  deleteDetector(selectedDetectorId)
                  setModalOpen(false)
                }}
              >
                Delete Detector
              </Button>
            </Box>
          </Box>
        </Modal>
      )}
    </>
  )
}
