import { AddButton } from '@/components/addButton'
import {
  useDeleteApproach,
  useEditApproach,
} from '@/features/locations/api/approach'
import EditApproachGrid from '@/features/locations/components/EditApproachGrid'
import EditDetectors from '@/features/locations/components/editDetector/EditDetectors'
import {
  Approach,
  Detector,
  LocationExpanded,
} from '@/features/locations/types'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import DeleteIcon from '@mui/icons-material/Delete'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import SaveIcon from '@mui/icons-material/Save'
import {
  Box,
  Button,
  ButtonBase,
  Collapse,
  IconButton,
  Modal,
  Paper,
  Tooltip,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'
import {
  ApproachForConfig,
  DetectorForConfig,
  LocationConfigHandler,
} from '../editLocation/editLocationConfigHandler'

interface ApproachAdminProps {
  approach: ApproachForConfig
  handler: LocationConfigHandler
}

const modalStyle = {
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
}

function EditApproach({ approach, handler }: ApproachAdminProps) {
  const [open, setOpen] = useState(
    handler.approaches.find((val) => val.index === approach.index)?.open ||
      false
  )
  const [openModal, setOpenModal] = useState(false)

  const { mutate: editApproach } = useEditApproach()
  const { mutate: deleteApproach } = useDeleteApproach()
  const detectionHardwareData = useConfigEnums(
    ConfigEnum.DetectionHardwareTypes
  )
  const detectionTypesData = useConfigEnums(ConfigEnum.DetectionTypes)
  const directionTypesData = useConfigEnums(ConfigEnum.DirectionTypes)
  const laneTypesData = useConfigEnums(ConfigEnum.LaneTypes)
  const movementTypesData = useConfigEnums(ConfigEnum.MovementTypes)

  // const deviceConfigurations = deviceConfigurationsData?.value
  // const products = productsData?.value
  // const deviceTypes = deviceTypesData?.data?.members.map(
  //   (member) => member.name
  // )
  // const deviceStatus = deviceStatusData?.data?.members.map(
  //   (member) => member.name
  // )

  const handleApproachClick = () => {
    handler.updateApproach({ ...approach, open: !approach.open })
    setOpen(!open)
  }

  const handleCopyApproach = () => {
    const { id, detectors, ...restOfApproach } = approach
    const newApproach = {
      ...restOfApproach,
      index: handler.approaches.length,
      open: false,
      description: `${approach.description} (copy)`,
      detectors: detectors.map(({ id, ...restDetector }) => ({
        ...restDetector,
      })),
    }

    handler.updateApproaches([
      ...handler.approaches,
      newApproach as ApproachForConfig,
    ])
  }

  const handleSaveApproach = () => {
    const modifiedApproach = JSON.parse(JSON.stringify(approach)) as Approach
    modifiedApproach.directionTypeId = parseInt(
      directionTypesData?.data?.members.find(
        (member) => member.name === modifiedApproach.directionTypeId
      )?.value
    )
    modifiedApproach.detectors.forEach((detector) => {
      if (detector.isNew) {
        delete detector.id
      }
      delete detector.isNew
      delete detector.approach
      delete detector.detectorComments
      detector.dectectorIdentifier =
        handler.expandedLocation?.locationIdentifier + detector.detectorChannel
      detector.detectionTypes.forEach((detectionType) => {
        detectionType.id = parseInt(
          detectionTypesData?.data?.members.find(
            (member) => member.name === detectionType.abbreviation
          )?.value
        )
      })
      detector.detectionHardware = parseInt(
        detectionHardwareData?.data?.members.find(
          (member) => member.name === detector.detectionHardware
        )?.value
      )
      detector.movementType = parseInt(
        movementTypesData?.data?.members.find(
          (member) => member.name === detector.movementType
        )?.value
      )
      detector.laneType = parseInt(
        laneTypesData?.data?.members.find(
          (member) => member.name === detector.laneType
        )?.value
      )
    })

    delete modifiedApproach.directionType
    delete modifiedApproach.index
    delete modifiedApproach.open
    delete modifiedApproach.isNew

    editApproach(modifiedApproach, {
      onSuccess: (data) => {
        console.log('Approach saved', data)
      },
    })
  }

  const handleDeleteApproach = () => {
    setOpenModal(true)
  }

  const confirmDeleteApproach = () => {
    const filteredApproaches = handler.approaches.filter(
      (val) => val.index !== approach.index
    )
    if (approach.isNew) {
      handler.updateApproaches(filteredApproaches)
    } else {
      deleteApproach(approach.id, {
        onSuccess: () => {
          handler.updateApproaches(filteredApproaches)
        },
      })
    }

    setOpenModal(false)
  }

  const handleAddNewDetectorClick = () => {
    const newDetector: Partial<DetectorForConfig> = {
      isNew: true,
      id: crypto.randomUUID(),
      approachId: approach.id,
      dateDisabled: null,
      decisionPoint: null,
      dectectorIdentifier: '',
      distanceFromStopBar: null,
      laneNumber: null,
      latencyCorrection: 0,
      movementDelay: null,
      movementType: 'NA',
      laneType: 'NA',
      detectionHardware: 'NA',
      detectionTypes: [],
      dateAdded: new Date().toISOString(),
      detectorComments: [],
    }

    const updatedApproach = {
      ...approach,
      detectors: [newDetector as Detector, ...approach.detectors],
    }

    handler.updateApproach(updatedApproach)
  }

  return (
    <>
      <Paper sx={{ mt: 1 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: 1,
            backgroundColor: approach.isNew
              ? 'rgba(100, 210, 100, 0.3)'
              : 'white',
          }}
        >
          <Tooltip title="Approach Details">
            <ButtonBase
              onClick={handleApproachClick}
              sx={{
                cursor: 'pointer',
                textTransform: 'none',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                width: '100%',
              }}
            >
              <Box display="flex" alignItems="center">
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    transition: 'transform 0.2s ease-in-out',
                    transform: open ? 'rotateZ(-180deg)' : 'rotateZ(0deg)',
                  }}
                >
                  <ExpandMoreIcon />
                </Box>
                <Typography
                  variant="h4"
                  component={'h3'}
                  sx={{ padding: 1, marginRight: 2 }}
                >
                  {approach.description}
                </Typography>
                <Typography variant="h5" component="p">
                  {approach.detectors.length}{' '}
                  {approach.detectors.length === 1 ? 'Detector' : 'Detectors'}
                </Typography>
              </Box>
            </ButtonBase>
          </Tooltip>
          <Box display="flex" alignItems="center">
            <Tooltip title="Copy Approach">
              <IconButton
                aria-label="copy approach"
                onClick={handleCopyApproach}
              >
                <ContentCopyIcon />
              </IconButton>
            </Tooltip>
            <Tooltip title="Save Approach">
              <IconButton
                aria-label="save approach"
                color="success"
                onClick={handleSaveApproach}
              >
                <SaveIcon />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete Approach">
              <IconButton
                aria-label="delete approach"
                color="error"
                onClick={handleDeleteApproach}
              >
                <DeleteIcon />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
      </Paper>
      <Collapse in={open} unmountOnExit>
        <>
          <EditApproachGrid
            approach={approach}
            approaches={handler.approaches}
            location={handler.expandedLocation as LocationExpanded}
            updateApproach={handler.updateApproach}
            updateApproaches={handler.updateApproaches}
          />
          <br />
          <Box display="flex" justifyContent="flex-end" mb={1}>
            <AddButton
              label="New Detector"
              onClick={handleAddNewDetectorClick}
              sx={{ m: 1 }}
            />
          </Box>
          <EditDetectors
            detectors={approach.detectors}
            approach={approach}
            updateApproach={handler.updateApproach}
          />
        </>
      </Collapse>
      <Modal
        open={openModal}
        onClose={() => setOpenModal(false)}
        aria-labelledby="delete-confirmation"
        aria-describedby="confirm-delete-approach"
      >
        <Box sx={modalStyle}>
          <Typography id="delete-confirmation" sx={{ fontWeight: 'bold' }}>
            Confirm Delete
          </Typography>
          <Typography id="confirm-delete-approach" sx={{ mt: 2 }}>
            Are you sure you want to delete this approach?
          </Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenModal(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={confirmDeleteApproach}
              color="error"
              variant="contained"
            >
              Delete Approach
            </Button>
          </Box>
        </Box>
      </Modal>
    </>
  )
}

export default React.memo(EditApproach)
