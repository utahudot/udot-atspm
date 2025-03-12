import {
  DetectionHardwareTypes,
  DetectionTypes,
  DirectionTypes,
  LaneTypes,
  MovementTypes,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { AddButton } from '@/components/addButton'
import {
  useDeleteApproach,
  useEditApproach,
} from '@/features/locations/api/approach'
import EditApproachGrid from '@/features/locations/components/EditApproachGrid'
import ApproachEditorRowHeader from '@/features/locations/components/editApproach/ApproachEditorRow'
import DeleteApproachModal from '@/features/locations/components/editApproach/DeleteApproachModal'
import { hasUniqueDetectorChannels } from '@/features/locations/components/editApproach/utils/checkDetectors'
import EditDetectors from '@/features/locations/components/editDetector/EditDetectors'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { Box, Collapse, Paper } from '@mui/material'
import React, { useEffect, useState } from 'react'
import {
  ApproachForConfig,
  DetectorForConfig,
  LocationConfigHandler,
} from '../editLocation/editLocationConfigHandler'

interface ApproachAdminProps {
  approach: ApproachForConfig
  handler: LocationConfigHandler
}

function EditApproach({ approach, handler }: ApproachAdminProps) {
  const [open, setOpen] = useState(false)
  const [openModal, setOpenModal] = useState(false)
  const [errors, setErrors] = useState<Record<
    string,
    { error: string; id: string }
  > | null>(null)
  const [warnings, setWarnings] = useState<Record<
    string,
    { warning: string; id: string }
  > | null>(null)

  const { mutate: editApproach } = useEditApproach()
  const { mutate: deleteApproach } = useDeleteApproach()
  const { findEnumByNameOrAbbreviation: findDetectionType } = useConfigEnums(
    ConfigEnum.DetectionTypes
  )
  const { findEnumByNameOrAbbreviation: findDirectionType } = useConfigEnums(
    ConfigEnum.DirectionTypes
  )
  const { findEnumByNameOrAbbreviation: findLaneType } = useConfigEnums(
    ConfigEnum.LaneTypes
  )
  const { findEnumByNameOrAbbreviation: findMovementType } = useConfigEnums(
    ConfigEnum.MovementTypes
  )

  const { findEnumByNameOrAbbreviation: findDetectionHardware } =
    useConfigEnums(ConfigEnum.DetectionHardwareTypes)

  console.log('types', findDetectionHardware('NA'))

  useEffect(() => {
    const { isValid, errors } = hasUniqueDetectorChannels(handler.approaches)

    if (isValid) {
      setWarnings(null)
      setErrors(null)
    } else {
      setWarnings(
        Object.keys(errors).reduce(
          (acc, key) => {
            acc[key] = { warning: errors[key].error, id: errors[key].id }
            return acc
          },
          {} as Record<string, { warning: string; id: string }>
        )
      )
    }
  }, [handler.approaches])

  const handleApproachClick = () => {
    setOpen(!open)
  }

  const handleCopyApproach = () => {
    const { id, detectors, ...restOfApproach } = approach
    const newApproach = {
      ...restOfApproach,
      index: handler.approaches.length,
      open: false,
      description: `${approach.description} (copy)`,
      detectors: detectors?.map(({ id, ...restDetector }) => ({
        ...restDetector,
      })),
    }

    handler.updateApproaches([
      newApproach as ApproachForConfig,
      ...handler.approaches,
    ])
  }

  const handleSaveApproach = () => {
    // Clear previous errors
    let newErrors: Record<string, { error: string; id: string }> = {}

    const { isValid, errors: channelErrors } = hasUniqueDetectorChannels(
      handler.approaches
    )

    if (!isValid) {
      newErrors = { ...newErrors, ...channelErrors }
    }

    if (
      !approach?.protectedPhaseNumber ||
      isNaN(approach?.protectedPhaseNumber)
    ) {
      newErrors = {
        ...newErrors,
        protectedPhaseNumber: {
          error: 'Protected Phase Number is required',
          id: approach.id,
        },
      }
    }

    // Make sure all detectors have a channel
    approach.detectors.forEach((detector) => {
      if (!detector.detectorChannel) {
        newErrors = {
          ...newErrors,
          [detector.id]: {
            error: 'Detector Channel is required',
            id: detector.id,
          },
        }
      }
    })

    // Set errors and exit if any errors are found
    if (!isEmptyObject(newErrors)) {
      setErrors(newErrors)
      return
    }

    setErrors(null)

    const modifiedApproach = JSON.parse(
      JSON.stringify(approach)
    ) as ApproachForConfig
    if (modifiedApproach.isNew) {
      delete modifiedApproach.id
      modifiedApproach.detectors?.forEach((detector) => {
        delete detector.approachId
      })
    }
    delete modifiedApproach.directionType
    delete modifiedApproach.index
    delete modifiedApproach.open
    delete modifiedApproach.isNew

    modifiedApproach.directionTypeId =
      findDirectionType(modifiedApproach.directionTypeId)?.value ||
      DirectionTypes.NA
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
        detectionType.id = findDetectionType(detectionType.abbreviation)?.value
      })
      detector.detectionHardware = findDetectionHardware(
        detector.detectionHardware
      )?.value
      detector.movementType = findMovementType(detector.movementType)?.value
      detector.laneType = findLaneType(detector.laneType)?.value
    })

    editApproach(modifiedApproach, {
      onSuccess: (data: ApproachForConfig) => {
        data.directionTypeId =
          findDirectionType(data.directionTypeId)?.name || DirectionTypes.NA
        data.detectors.forEach((detector) => {
          detector.detectionTypes.forEach((detectionType) => {
            detectionType.abbreviation =
              findDetectionType(detectionType.abbreviation)?.name ||
              DetectionTypes.NA
          })
          detector.detectionHardware =
            findDetectionHardware(detector.detectionHardware)?.name ||
            DetectionHardwareTypes.NA
          detector.movementType =
            findMovementType(detector.movementType)?.name || MovementTypes.NA
          detector.laneType =
            findLaneType(detector.laneType)?.name || LaneTypes.NA
        })

        handler.updateApproaches(
          handler.approaches.map((item) =>
            item.id === approach.id ? data : item
          )
        )
      },
    })
  }

  const openDeleteApproachModal = () => {
    setOpenModal(true)
  }

  const handleDeleteApproach = () => {
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
      id: parseInt(crypto.randomUUID()),
      approachId: approach.id,
      dateDisabled: null,
      decisionPoint: null,
      dectectorIdentifier: '',
      distanceFromStopBar: null,
      laneNumber: null,
      latencyCorrection: 0,
      movementDelay: null,
      detectionTypes: [],
      dateAdded: new Date().toISOString(),
      detectorComments: [],
    }

    const updatedApproach = {
      ...approach,
      detectors: [newDetector, ...approach.detectors],
    }

    handler.updateApproaches(
      handler.approaches.map((item) =>
        item.id === updatedApproach.id ? updatedApproach : item
      )
    )
  }

  const updateApproach = (updatedApproach: ApproachForConfig) => {
    handler.updateApproaches(
      handler.approaches.map((item) =>
        item.id === updatedApproach.id ? updatedApproach : item
      )
    )
  }

  return (
    <>
      <Paper sx={{ mt: 1 }}>
        <ApproachEditorRowHeader
          open={open}
          approach={approach}
          handleApproachClick={handleApproachClick}
          handleCopyApproach={handleCopyApproach}
          handleSaveApproach={handleSaveApproach}
          openDeleteApproachModal={openDeleteApproachModal}
        />
      </Paper>
      <Collapse in={open} unmountOnExit>
        <Box minHeight={'600px'}>
          <EditApproachGrid
            errors={errors}
            approach={approach}
            approaches={handler.approaches}
            location={handler.expandedLocation}
            updateApproach={updateApproach}
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
            updateApproach={updateApproach}
            errors={errors}
            warnings={warnings}
          />
        </Box>
      </Collapse>
      <DeleteApproachModal
        openModal={openModal}
        setOpenModal={setOpenModal}
        confirmDeleteApproach={handleDeleteApproach}
      />
    </>
  )
}

export default React.memo(EditApproach)

const isEmptyObject = (obj: Record<string, any>): boolean => {
  return Object.keys(obj).length === 0
}
