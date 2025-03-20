import { DirectionTypes } from '@/api/config/aTSPMConfigurationApi.schemas'
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
  console.log('EditApproach:', approach)

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

  const openDeleteApproachModal = useCallback(() => {
    setOpenModal(true)
  }, [])

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
      delete detector.isNew
      delete detector.approach
      delete detector.detectorComments

      detector.latencyCorrection =
        detector.latencyCorrection === null ||
        detector.latencyCorrection === undefined ||
        detector.latencyCorrection === ''
          ? 0
          : Number(detector.latencyCorrection)
      detector.dectectorIdentifier =
        handler.expandedLocation?.locationIdentifier + detector.detectorChannel
      detector.detectionTypes.forEach((detectionType) => {
        detectionType.id = findDetectionType(detectionType.abbreviation)?.value
      })

      det.detectionHardware = findDetectionHardware(
        det.detectionHardware
      )?.value
      det.movementType = findMovementType(det.movementType)?.value
      det.laneType = findLaneType(det.laneType)?.value
    })

    editApproach(modifiedApproach, {
      onSuccess: (data: ApproachForConfig) => {
        data.directionTypeId = findDirectionType(data.directionTypeId)?.name
        data.detectors.forEach((detector) => {
          detector.detectionTypes.forEach((detectionType) => {
            detectionType.abbreviation = findDetectionType(
              detectionType.abbreviation
            )?.name
          })
          detector.detectionHardware = findDetectionHardware(
            detector.detectionHardware
          )?.name
          detector.movementType = findMovementType(detector.movementType)?.name
          detector.laneType = findLaneType(detector.laneType)?.name
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
      id: crypto.randomUUID(),
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
      detectors: [newDetector as Detector, ...approach.detectors],
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
