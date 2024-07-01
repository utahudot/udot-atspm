import { modalButtonLocation } from '@/components/GenericAdminChart'
import { useDeleteDetector } from '@/features/locations/api/detector'
import CommentCell from '@/features/locations/components/editDetector/CommentCell'
import DateAddedCell from '@/features/locations/components/editDetector/DateAddedCell'
import DetectionTypesCell from '@/features/locations/components/editDetector/DetectionTypesCell'
import HardwareTypeCell from '@/features/locations/components/editDetector/HardwareTypeCell'
import LaneTypeCell from '@/features/locations/components/editDetector/LaneTypeCell'
import MovementTypeCell from '@/features/locations/components/editDetector/MovementTypeCell'
import EditableTableCell from '@/features/locations/components/editableTableCell/EditableTableCell'
import { Detector } from '@/features/locations/types'
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
import React, { useState } from 'react'
import { ApproachForConfig } from '../editLocation/editLocationConfigHandler'

export const modalStyle = {
  position: 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: 'none',
  borderRadius: '10px',
  boxShadow: 24,
  p: 4,
  '@media (max-width: 400px)': {
    width: '100%',
  },
}

interface EditDetectorsProps {
  detectors: Detector[]
  approach: ApproachForConfig
  updateApproach: (approach: ApproachForConfig) => void
}

function EditDetectors({
  detectors,
  approach,
  updateApproach,
}: EditDetectorsProps) {
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedDetectorId, setSelectedDetectorId] = useState<number | null>(
    null
  )
  const { mutate: deleteDetector } = useDeleteDetector()

  const updateDetector = (id: number, name: string, val: unknown) => {
    val = val === '' ? null : val
    updateApproach({
      ...approach,
      detectors: approach.detectors.map((detector) =>
        detector.id === id ? { ...detector, [name]: val } : detector
      ),
    })
  }

  const handleDeleteClick = (id: number) => {
    updateApproach({
      ...approach,
      detectors: approach.detectors.filter((detector) => detector.id !== id),
    })
    deleteDetector(id)
    setModalOpen(false)
  }

  const deleteModal = (id: number) => (
    <Modal open={modalOpen} onClose={() => setModalOpen(false)}>
      <Box sx={modalStyle}>
        <Typography sx={{ fontWeight: 'bold' }}>Delete Detector</Typography>
        <Divider sx={{ margin: '10px 0', backgroundColor: 'gray' }} />
        <Typography>Are you sure you want to delete this detector?</Typography>
        <Box sx={modalButtonLocation}>
          <Button onClick={() => setModalOpen(false)}>Cancel</Button>
          <Button onClick={() => handleDeleteClick(id)} sx={{ color: 'red' }}>
            Delete Detector
          </Button>
        </Box>
      </Box>
    </Modal>
  )

  return (
    <>
      <TableContainer component={Paper}>
        <Table stickyHeader aria-label="detectors table">
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
                colSpan={9}
                sx={{ borderBottom: 'none', pb: 0 }}
                component={'td'}
              />
              <TableCell
                colSpan={2}
                align="center"
                sx={{ borderBottom: 'none', pb: 0 }}
              >
                <Typography variant="caption" fontStyle={'italic'}>
                  Advanced Count Only
                </Typography>
              </TableCell>
              <TableCell
                colSpan={2}
                align="center"
                sx={{ borderBottom: 'none', pb: 0 }}
              >
                <Typography variant="caption" fontStyle={'italic'}>
                  Advanced Speed Only
                </Typography>
              </TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Channel</TableCell>
              <TableCell sx={{ minWidth: '225px' }}>Detection Types</TableCell>
              <TableCell>Hardware</TableCell>
              <TableCell>Latency Correction</TableCell>
              <TableCell>Lane Number</TableCell>
              <TableCell sx={{ maxWidth: '100px' }}>Movement Type</TableCell>
              <TableCell>Lane Type</TableCell>
              <TableCell>Date Added</TableCell>
              <TableCell>Comments</TableCell>
              <TableCell sx={{ minWidth: '100px' }}>
                Distance to Stop Bar
              </TableCell>
              <TableCell>Decision Point</TableCell>
              <TableCell sx={{ minWidth: '100px' }}>
                Minimum Speed Filter
              </TableCell>
              <TableCell>Movement Delay</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {detectors.length === 0 && (
              <TableRow>
                <TableCell colSpan={15}>
                  <Typography variant="h6" align="center">
                    No detectors added
                  </Typography>
                </TableCell>
              </TableRow>
            )}
            {detectors.map((detector, i) => (
              <TableRow
                key={detector.id || i}
                sx={{
                  backgroundColor: detector.isNew
                    ? 'rgba(100, 210, 100, 0.3)'
                    : 'white',
                  '& .MuiTableCell-body': {
                    fontSize: '0.9rem',
                    borderRight: '1px solid #e0e0e0',
                    padding: '.5rem',
                  },
                }}
              >
                <EditableTableCell
                  value={detector.detectorChannel}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'detectorChannel', newValue)
                  }
                />
                <DetectionTypesCell
                  detector={detector}
                  onUpdate={(newSelection) => {
                    updateDetector(detector.id, 'detectionTypes', newSelection)
                  }}
                />
                <HardwareTypeCell
                  value={detector.detectionHardware}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'detectionHardware', newValue)
                  }
                />
                <EditableTableCell
                  value={detector.latencyCorrection}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'latencyCorrection', newValue)
                  }
                />
                <EditableTableCell
                  value={detector.laneNumber}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'laneNumber', newValue)
                  }
                />
                <MovementTypeCell
                  value={detector.movementType}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'movementType', newValue)
                  }
                />
                <LaneTypeCell
                  value={detector.laneType}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'laneType', newValue)
                  }
                />
                <DateAddedCell
                  value={detector.dateAdded}
                  onUpdate={(newValue) =>
                    updateDetector(
                      detector.id,
                      'dateAdded',
                      newValue.toISOString()
                    )
                  }
                />
                <CommentCell detector={detector} />
                <EditableTableCell
                  value={detector.distanceFromStopBar}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'distanceFromStopBar', newValue)
                  }
                />
                <EditableTableCell
                  value={detector.decisionPoint}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'decisionPoint', newValue)
                  }
                />
                <EditableTableCell
                  value={detector.minSpeedFilter}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'minSpeedFilter', newValue)
                  }
                />
                <EditableTableCell
                  value={detector.movementDelay}
                  onUpdate={(newValue) =>
                    updateDetector(detector.id, 'movementDelay', newValue)
                  }
                />
                <TableCell align="center">
                  <IconButton
                    aria-label="delete detector"
                    onClick={() => {
                      setSelectedDetectorId(detector?.id)
                      setModalOpen(true)
                    }}
                    color="error"
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      {selectedDetectorId && deleteModal(selectedDetectorId)}
    </>
  )
}

export default React.memo(EditDetectors)
