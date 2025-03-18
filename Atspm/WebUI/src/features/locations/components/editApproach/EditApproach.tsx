import {
  DetectionHardwareTypes,
  DetectionTypes,
  DirectionTypes,
  LaneTypes,
  MovementTypes,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { AddButton } from '@/components/addButton'
import { useEditApproach } from '@/features/locations/api/approach'
// import { useUpsertApproachApproach } from '@/api/config/aTSPMConfigurationApi'
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
import { useNotificationStore } from '@/stores/notifications'
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
    setErrors,
    setWarnings,
    channelMap,
    clearErrorsAndWarnings,
  } = useLocationStore()
  const approaches = location?.approaches || []

  const [open, setOpen] = useState(false)
  const [openModal, setOpenModal] = useState(false)
  const { addNotification } = useNotificationStore()

  const { mutate: editApproach } = useEditApproach()
  // const { mutate: editApproach } = useUpsertApproachApproach() //giving me 415 errors

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
    console.log('useEffect is called')
    const { isValid, errors } = hasUniqueDetectorChannels(channelMap)
    if (isValid) {
      clearErrorsAndWarnings()
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
  }, [location?.approaches, setWarnings, clearErrorsAndWarnings])

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
      hasUniqueDetectorChannels(channelMap)
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

      detector.latencyCorrection =
        detector.latencyCorrection === null ||
        detector.latencyCorrection === undefined ||
        detector.latencyCorrection === ''
          ? 0
          : Number(detector.latencyCorrection)

      detector.detectorIdentifier = // Fixed typo
        (location?.locationIdentifier || '') + (detector.detectorChannel || '')

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
        try {
          // Normalize the response
          const detectorsArray = saved.detectors?.$values || []
          detectorsArray.forEach((detector) => {
            detector.detectionTypes = detector.detectionTypes?.$values || []
            detector.detectionTypes.forEach((dType) => {
              dType.abbreviation =
                findDetectionType(dType.abbreviation)?.name || DetectionTypes.NA
            })
            detector.detectionHardware =
              findDetectionHardware(detector.detectionHardware)?.name ||
              DetectionHardwareTypes.NA
            detector.movementType =
              findMovementType(detector.movementType)?.name || MovementTypes.NA
            detector.laneType =
              findLaneType(detector.laneType)?.name || LaneTypes.NA
          })

          const normalizedSaved: ConfigApproach = {
            ...saved,
            directionTypeId:
              findDirectionType(saved.directionTypeId)?.name ||
              DirectionTypes.NA,
            detectors: detectorsArray,
          }

          updateApproaches(
            approaches.map((item) =>
              item.id === approach.id ? normalizedSaved : item
            )
          )

          addNotification({
            title: 'Approach saved successfully',
            type: 'success',
          })
        } catch (error) {
          console.error('Error processing saved approach:', error)
          addNotification({
            title: 'Failed to process saved approach',
            type: 'error',
            message:
              error instanceof Error
                ? error.message
                : 'An unexpected error occurred',
          })
        }
      },
      onError: (error) => {
        console.error('Failed to save approach:', error)
        addNotification({
          title: 'Failed to save approach',
          type: 'error',
          message:
            error instanceof Error
              ? error.message
              : 'An unexpected error occurred',
        })
      },
    })
  }

  const handleDeleteApproach = () => {
    try {
      deleteApproach(approach)
      addNotification({
        title: 'Approach deleted',
        type: 'success',
      })
    } catch (error) {
      console.error('Failed to delete:', error)
      addNotification({
        title: 'Failed to delete approach',
        type: 'error',
        message:
          error instanceof Error
            ? error.message
            : 'An unexpected error occurred',
      })
      setOpenModal(false)
    }
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
