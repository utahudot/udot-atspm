import {
  DetectionHardwareTypes,
  DetectionTypes,
  DirectionTypes,
  LaneTypes,
  MovementTypes,
} from '@/api/config'
import { Color } from '@/features/charts/utils'
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
import { dateToTimestamp } from '@/utils/dateTime'
import { removeAuditFields } from '@/utils/removeAuditFields'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  Box,
  Button,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  Paper,
} from '@mui/material'
import React, { useCallback, useMemo, useState } from 'react'

interface ApproachAdminProps {
  approach: ConfigApproach
}

function EditApproach({ approach }: ApproachAdminProps) {
  const locationIdentifier = useLocationStore(
    (s) => s.location?.locationIdentifier
  )
  const deleteDetector = useLocationStore((s) => s.deleteDetector)
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
  const updateSavedApproaches = useLocationStore((s) => s.updateSavedApproach)

  const [open, setOpen] = useState(false)
  const [openModal, setOpenModal] = useState(false)
  const [deleteMode, setDeleteMode] = useState(false)
  const [selectedDetectorIds, setSelectedDetectorIds] = useState<number[]>([])
  const [confirmDelete, setConfirmDelete] = useState(false)

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
    // 1) check for duplicate detectorChannel errors
    const { isValid, errors: channelErrors } =
      hasUniqueDetectorChannels(channelMap)

    // collect all our errors here
    let newErrors: Record<string, { error: string; id: string }> = {}
    if (!isValid) {
      newErrors = { ...newErrors, ...channelErrors }
    }

    // ── parse phase inputs into number | null ──
    const rawProt = approach.protectedPhaseNumber
    const rawPerm = approach.permissivePhaseNumber
    const rawPed = approach.pedestrianPhaseNumber

    const protectedPhaseNumber =
      rawProt === '' || rawProt == null ? null : Number(rawProt)
    const permissivePhaseNumber =
      rawPerm === '' || rawPerm == null ? null : Number(rawPerm)
    const pedestrianPhaseNumber =
      rawPed === '' || rawPed == null ? null : Number(rawPed)

    // 2) protectedPhaseNumber is always required (even if it’s zero)
    if (protectedPhaseNumber == null) {
      newErrors[approach.id] = {
        error: 'A Phase Number is required',
        id: String(approach.id),
      }
    }

    // 3) every detector must have a channel
    approach.detectors.forEach((det) => {
      if (!det.detectorChannel) {
        newErrors[String(det.id)] = {
          error: 'Detector Channel is required',
          id: String(det.id),
        }
      }
    })

    // if any errors, stop here and render them
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors)
      return
    }
    setErrors(null)

    // Create a deep clone so we can safely mutate
    let modifiedApproach = JSON.parse(
      JSON.stringify(approach)
    ) as ConfigApproach

    // overwrite with our parsed phase values
    modifiedApproach.protectedPhaseNumber = protectedPhaseNumber
    modifiedApproach.permissivePhaseNumber = permissivePhaseNumber
    modifiedApproach.pedestrianPhaseNumber = pedestrianPhaseNumber

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

      det.dateAdded = dateToTimestamp(det.dateAdded)

      det.detectionHardware = findDetectionHardware(
        det.detectionHardware
      )?.value
      det.movementType = findMovementType(det.movementType)?.value
      det.laneType = findLaneType(det.laneType)?.value
    })

    // remove audit fields
    modifiedApproach = removeAuditFields(modifiedApproach)
    modifiedApproach.detectors =
      modifiedApproach.detectors.map(removeAuditFields)

    editApproach(modifiedApproach, {
      onSuccess: (saved) => {
        try {
          const detectorsArray = saved.detectors || []
          detectorsArray.forEach((detector) => {
            detector.detectionTypes = detector.detectionTypes || []
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
            detectors: detectorsArray.sort(
              (a, b) => a.detectorChannel - b.detectorChannel
            ),
          }

          if (approach.isNew) {
            deleteApproachInStore(approach)
          }

          updateApproachInStore(normalizedSaved)
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
    updateSavedApproaches,
    updateApproachInStore,
    deleteApproachInStore,
    addNotification,
  ])

  const confirmDeleteSelected = useCallback(() => {
    selectedDetectorIds.forEach((id) => deleteDetector(id))
    setConfirmDelete(false)
    setDeleteMode(false)
    setSelectedDetectorIds([])
    addNotification({ title: 'Selected detectors deleted', type: 'success' })
  }, [selectedDetectorIds, deleteDetector, addNotification])

  const handleDeleteApproach = useCallback(() => {
    try {
      deleteApproachInStore(approach)
      addNotification({ title: 'Approach deleted', type: 'success' })
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

  const leftBorderColor = useMemo(() => {
    if (approach.directionTypeId === DirectionTypes.NA) {
      return 'lightgrey'
    }
    const dir = approach.directionTypeId?.charAt(0).toUpperCase()
    switch (dir) {
      case 'N':
        return Color.Blue
      case 'S':
        return Color.BrightRed
      case 'E':
        return Color.Yellow
      case 'W':
        return Color.Orange
      default:
        return 'lightgrey'
    }
  }, [approach.directionTypeId])

  return (
    <>
      <Paper
        variant="outlined"
        sx={{ mb: '6px', borderLeft: `7px solid ${leftBorderColor}` }}
      >
        <ApproachEditorRowHeader
          open={open}
          approach={approach}
          handleApproachClick={handleApproachClick}
          handleCopyApproach={() => copyApproachInStore(approach)}
          handleSaveApproach={handleSaveApproach}
          openDeleteApproachModal={openDeleteApproachModal}
        />
        <Divider />
        <Collapse in={open} unmountOnExit>
          <>
            <EditApproachGrid approach={approach} />

            <Box display="flex" justifyContent="flex-end" mb={1}>
              {!deleteMode && (
                <>
                  <Button
                    variant="contained"
                    color="success"
                    size="small"
                    onClick={() => addDetectorInStore(approach.id)}
                    sx={{ m: 1, textTransform: 'none' }}
                    startIcon={<AddIcon />}
                  >
                    Add Detector
                  </Button>
                  <Button
                    variant="contained"
                    color="error"
                    size="small"
                    onClick={() => setDeleteMode(true)}
                    sx={{ m: 1, textTransform: 'none' }}
                    startIcon={<DeleteIcon />}
                  >
                    Delete Detectors
                  </Button>
                </>
              )}
              {deleteMode && (
                <>
                  <Button
                    size="small"
                    variant="outlined"
                    onClick={() => {
                      setDeleteMode(false)
                      setSelectedDetectorIds([])
                    }}
                    sx={{ m: 1, textTransform: 'none' }}
                  >
                    Cancel
                  </Button>
                  <Button
                    size="small"
                    variant="contained"
                    color="error"
                    disabled={selectedDetectorIds.length === 0}
                    onClick={() => setConfirmDelete(true)}
                    sx={{ m: 1, textTransform: 'none' }}
                  >
                    Delete Selected Detectors
                  </Button>
                </>
              )}
            </Box>

            <Box sx={{ mt: 1, ml: '1px' }}>
              <EditDetectors
                approach={approach}
                deleteMode={deleteMode}
                onSelectionChange={(ids) => setSelectedDetectorIds(ids)}
              />
            </Box>
          </>
        </Collapse>
      </Paper>

      <Dialog open={confirmDelete} onClose={() => setConfirmDelete(false)}>
        <DialogTitle>Confirm Delete Selected Detectors</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the selected detectors?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDelete(false)}>Cancel</Button>
          <Button color="error" onClick={confirmDeleteSelected}>
            Yes
          </Button>
        </DialogActions>
      </Dialog>

      <DeleteApproachModal
        openModal={openModal}
        setOpenModal={setOpenModal}
        confirmDeleteApproach={handleDeleteApproach}
      />
    </>
  )
}

export default React.memo(EditApproach)
