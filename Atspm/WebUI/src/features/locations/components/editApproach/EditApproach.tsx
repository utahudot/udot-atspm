import {
  DetectionHardwareTypes,
  DetectionTypes,
  DirectionTypes,
  LaneTypes,
  MovementTypes,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { AddButton } from '@/components/addButton'
import { useEditApproach } from '@/features/locations/api/approach'
import ApproachEditorRowHeader from '@/features/locations/components/editApproach/ApproachEditorRow'
import DeleteApproachModal from '@/features/locations/components/editApproach/DeleteApproachModal'
import { hasUniqueDetectorChannels } from '@/features/locations/components/editApproach/utils/checkDetectors'
import EditApproachGrid from '@/features/locations/components/EditApproachGrid'
import EditDetectors from '@/features/locations/components/editDetector/EditDetectors'
import {
  ConfigApproach,
  useLocationStore,
} from '@/features/locations/components/editLocation/locationStore'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { Box, Collapse, Paper } from '@mui/material'
import React, { useEffect, useState } from 'react'

interface ApproachAdminProps {
  approach: ConfigApproach
}

function EditApproach({ approach }: ApproachAdminProps) {
  const {
    location,
    updateApproaches,
    copyApproach,
    deleteApproach,
    addDetector,
  } = useLocationStore()
  const approaches = location?.approaches || []

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

  useEffect(() => {
    if (!location?.approaches) return
    const { isValid, errors } = hasUniqueDetectorChannels(location.approaches)

    if (isValid) {
      setWarnings(null)
      setErrors(null)
    } else {
      // Convert the errors object to warnings
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
  }, [location?.approaches])

  const handleApproachClick = () => {
    setOpen((prev) => !prev)
  }

  const openDeleteApproachModal = () => {
    setOpenModal(true)
  }

  const handleSaveApproach = () => {
    // Clear previous errors
    let newErrors: Record<string, { error: string; id: string }> = {}

    // Check detector channel uniqueness across all approaches
    const { isValid, errors: channelErrors } =
      hasUniqueDetectorChannels(approaches)
    if (!isValid) {
      newErrors = { ...newErrors, ...channelErrors }
    }

    // Protected phase required
    if (
      !approach.protectedPhaseNumber ||
      isNaN(approach.protectedPhaseNumber)
    ) {
      newErrors = {
        ...newErrors,
        protectedPhaseNumber: {
          error: 'Protected Phase Number is required',
          id: String(approach.id),
        },
      }
    }

    // Each detector must have a channel
    approach.detectors.forEach((detector) => {
      if (!detector.detectorChannel) {
        newErrors = {
          ...newErrors,
          [String(detector.id)]: {
            error: 'Detector Channel is required',
            id: String(detector.id),
          },
        }
      }
    })

    if (!isEmptyObject(newErrors)) {
      setErrors(newErrors)
      return
    }

    setErrors(null)

    // Prepare approach payload
    const modifiedApproach = JSON.parse(
      JSON.stringify(approach)
    ) as ConfigApproach

    // Clean up isNew, directionType, etc.
    if (modifiedApproach.isNew) {
      delete modifiedApproach.id
      modifiedApproach.detectors?.forEach((d) => delete d.approachId)
    }
    delete modifiedApproach.index
    delete modifiedApproach.open
    delete modifiedApproach.isNew

    // Convert direction type from name -> numeric enum
    modifiedApproach.directionTypeId =
      findDirectionType(modifiedApproach.directionTypeId)?.value ||
      DirectionTypes.NA

    // Detectors
    modifiedApproach.detectors.forEach((detector) => {
      if (detector.isNew) {
        delete detector.id
      }
      delete detector.isNew
      delete detector.approach
      delete detector.detectorComments

      // Build an identifier
      detector.dectectorIdentifier =
        (location?.locationIdentifier || '') + (detector.detectorChannel || '')

      // Convert detection type abbreviations -> numeric enum
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
      onSuccess: (saved) => {
        // Convert the numeric enums we got back from the API back to strings:
        saved.directionTypeId =
          findDirectionType(saved.directionTypeId)?.name || DirectionTypes.NA

        saved.detectors.forEach((det) => {
          det.detectionTypes.forEach((dType) => {
            dType.abbreviation =
              findDetectionType(dType.abbreviation)?.name || DetectionTypes.NA
          })
          det.detectionHardware =
            findDetectionHardware(det.detectionHardware)?.name ||
            DetectionHardwareTypes.NA
          det.movementType =
            findMovementType(det.movementType)?.name || MovementTypes.NA
          det.laneType = findLaneType(det.laneType)?.name || LaneTypes.NA
        })

        updateApproaches(
          approaches.map((item) => (item.id === approach.id ? saved : item))
        )
      },
    })
  }

  const handleDeleteApproach = () => {
    deleteApproach(approach)
    setOpenModal(false)
  }

  return (
    <>
      <Paper sx={{ mt: 1 }}>
        <ApproachEditorRowHeader
          open={open}
          approach={approach}
          handleApproachClick={handleApproachClick}
          handleCopyApproach={() => copyApproach(approach)}
          handleSaveApproach={handleSaveApproach}
          openDeleteApproachModal={openDeleteApproachModal}
        />
      </Paper>
      <Collapse in={open} unmountOnExit>
        <Box minHeight="600px">
          <EditApproachGrid approach={approach} />
          <br />
          <Box display="flex" justifyContent="flex-end" mb={1}>
            <AddButton
              label="New Detector"
              onClick={() => addDetector(approach)}
              sx={{ m: 1 }}
            />
          </Box>
          <EditDetectors approach={approach} />
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

function isEmptyObject(obj: Record<string, any>): boolean {
  return Object.keys(obj).length === 0
}
