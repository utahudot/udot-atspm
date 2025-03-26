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
import { useNotificationStore } from '@/stores/notifications'
import { Box, Collapse, Paper } from '@mui/material'
import React, { useCallback, useState } from 'react'

interface ApproachAdminProps {
  approach: ConfigApproach
}

function EditApproach({ approach }: ApproachAdminProps) {
  const locationIdentifier = useLocationStore(
    (s) => s.location?.locationIdentifier
  )
  const channelMap = useLocationStore((s) => s.channelMap)
  const errors = useLocationStore((s) => s.errors)
  const warnings = useLocationStore((s) => s.warnings)
  const setErrors = useLocationStore((s) => s.setErrors)
  const setWarnings = useLocationStore((s) => s.setWarnings)
  const clearErrorsAndWarnings = useLocationStore(
    (s) => s.clearErrorsAndWarnings
  )
  const updateApproachInStore = useLocationStore((s) => s.updateApproach)
  const copyApproachInStore = useLocationStore((s) => s.copyApproach)
  const deleteApproachInStore = useLocationStore((s) => s.deleteApproach)
  const addDetectorInStore = useLocationStore((s) => s.addDetector)
  const updateSavedApproaches = useLocationStore((s) => s.updateSavedApproaches)

  const [open, setOpen] = useState(false)
  const [openModal, setOpenModal] = useState(false)

  const { addNotification } = useNotificationStore()
  const { mutate: editApproach } = useEditApproach()

  // Lookups
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

  const handleApproachClick = useCallback(() => {
    setOpen((prev) => !prev)
  }, [])

  const openDeleteApproachModal = useCallback(() => {
    setOpenModal(true)
  }, [])

  const handleSaveApproach = useCallback(() => {
    const { isValid, errors: channelErrors } =
      hasUniqueDetectorChannels(channelMap)
    let newErrors: Record<string, { error: string; id: string }> = {}

    if (!isValid) {
      newErrors = { ...newErrors, ...channelErrors }
    }
    if (
      !approach.protectedPhaseNumber ||
      isNaN(approach.protectedPhaseNumber)
    ) {
      newErrors.protectedPhaseNumber = {
        error: 'Protected Phase Number is required',
        id: String(approach.id),
      }
    }
    approach.detectors.forEach((det) => {
      if (!det.detectorChannel) {
        newErrors[String(det.id)] = {
          error: 'Detector Channel is required',
          id: String(det.id),
        }
      }
    })

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors)
      return
    }
    setErrors(null)

    // Create a deep clone so we can safely mutate
    const modifiedApproach = JSON.parse(
      JSON.stringify(approach)
    ) as ConfigApproach

    // If the approach is new, remove the local ID so the server will create one
    if (modifiedApproach.isNew) {
      delete modifiedApproach.id
      modifiedApproach.detectors.forEach((d) => delete d.approachId)
    }
    delete modifiedApproach.index
    delete modifiedApproach.open
    delete modifiedApproach.isNew

    // Convert direction type from name -> numeric enum
    modifiedApproach.directionTypeId =
      findDirectionType(modifiedApproach.directionTypeId)?.value ||
      DirectionTypes.NA

    // Detectors
    modifiedApproach.detectors.forEach((det) => {
      if (det.isNew) {
        delete det.id
      }
      delete det.isNew
      delete det.approach
      delete det.detectorComments

      det.latencyCorrection =
        det.latencyCorrection == null || det.latencyCorrection === ''
          ? 0
          : Number(det.latencyCorrection)

      det.dectectorIdentifier =
        (locationIdentifier || '') + (det.detectorChannel || '')

      det.detectionTypes.forEach((dType) => {
        dType.id = findDetectionType(dType.abbreviation)?.value
      })

      det.detectionHardware = findDetectionHardware(
        det.detectionHardware
      )?.value
      det.movementType = findMovementType(det.movementType)?.value
      det.laneType = findLaneType(det.laneType)?.value
    })

    editApproach(modifiedApproach, {
      onSuccess: (saved) => {
        try {
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

          // Build final approach object for the store
          const normalizedSaved: ConfigApproach = {
            ...saved,
            isNew: false,
            directionTypeId:
              findDirectionType(saved.directionTypeId)?.name ||
              DirectionTypes.NA,
            detectors: detectorsArray,
          }

          /**
           * If the approach was new, we must remove the "local" approach
           * (with its random ID) from the store, then add this saved approach
           * (with the real server ID) so we don't end up with duplicates.
           */
          if (approach.isNew) {
            // 1) remove the old approach from store (no server API call for new approaches)
            deleteApproachInStore(approach)

            // 2) updateApproachInStore() with the newly created approach
            updateApproachInStore(normalizedSaved)
          } else {
            // If it wasn't new, we can just update existing approach
            updateApproachInStore(normalizedSaved)
          }

          // Update savedApproaches to reflect the saved state
          updateSavedApproaches(normalizedSaved)

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
  }, [
    approach,
    channelMap,
    locationIdentifier,
    editApproach,
    setErrors,
    findDirectionType,
    findMovementType,
    findLaneType,
    findDetectionHardware,
    findDetectionType,
    updateApproachInStore,
    deleteApproachInStore,
    updateSavedApproaches,
    addNotification,
  ])

  const handleDeleteApproach = useCallback(() => {
    try {
      deleteApproachInStore(approach)
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
  }, [approach, deleteApproachInStore, addNotification])

  return (
    <>
      <Paper sx={{ mt: 1 }}>
        <ApproachEditorRowHeader
          open={open}
          approach={approach}
          handleApproachClick={handleApproachClick}
          handleCopyApproach={() => copyApproachInStore(approach)}
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
              onClick={() => addDetectorInStore(approach.id)}
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
