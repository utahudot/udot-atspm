import CustomSelect from '@/components/customSelect'
import {
  useAllVersionsOfLocation,
  useLocationTypes,
} from '@/features/locations/api'
import {
  useCopyLocationToNewVersion,
  useDeleteVersion,
  useSetLocationToBeDeleted,
} from '@/features/locations/api/location'
import { Location, LocationExpanded } from '@/features/locations/types'
import { useNotificationStore } from '@/stores/notifications'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Box,
  Button,
  InputLabel,
  Menu,
  MenuItem,
  Modal,
  Paper,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import React, { useEffect, useState } from 'react'
import { useQueryClient } from 'react-query'

interface LocationAdminHeaderProps {
  location: LocationExpanded
  updateLocationVersion: (location: Location | null) => void
  refetchLocation: () => void
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

const EditLocationHeader = ({
  location,
  updateLocationVersion,
  refetchLocation,
}: LocationAdminHeaderProps) => {
  const { addNotification } = useNotificationStore()

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [openModal, setOpenModal] = useState(false)
  const [modalAction, setModalAction] = useState('')

  const queryClient = useQueryClient()
  const { mutate: copyVersion } = useCopyLocationToNewVersion(location.id)
  const { mutate: deleteLocation } = useSetLocationToBeDeleted(location.id)
  const { mutate: deleteVersion } = useDeleteVersion()
  const { data: versionData, refetch: refetchAllVersionsOfLocation } =
    useAllVersionsOfLocation(location?.locationIdentifier, { enabled: false })

  const open = Boolean(anchorEl)

  const { data: locationTypeData } = useLocationTypes()

  const locationType = locationTypeData?.value.find(
    (type) => type.id === location?.locationTypeId
  )

  const locationsVersions = versionData?.value.map((version: Location) => ({
    id: version.id,
    note: version.note,
  }))

  useEffect(() => {
    if (location.locationIdentifier) {
      refetchAllVersionsOfLocation()
    }
  }, [location.locationIdentifier, refetchAllVersionsOfLocation])

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleVersionChange = (e: SelectChangeEvent<unknown>) => {
    const { value } = e.target
    const newLocation = versionData?.value.find(
      (version) => version.id === value
    )
    updateLocationVersion(newLocation as Location)
  }

  const handleAddNewVersionConfirm = () => {
    copyVersion(location.id, {
      onSuccess: (location) => {
        updateLocationVersion(location)
        refetchLocation()
        refetchAllVersionsOfLocation()
      },
    })
    setOpenModal(false)
  }

  const handleDeleteCurrentVersionConfirm = () => {
    deleteVersion(location.id, {
      onSuccess: () => {
        refetchLocation()
        refetchAllVersionsOfLocation()
        updateLocationVersion(
          versionData?.value.find(
            (version) => version.id !== location.id
          ) as Location
        )
        addNotification({
          type: 'error',
          title: `Version Deleted`,
        })
      },
    })
  }

  const handleModalPopup = (action: string) => {
    setModalAction(action)
    setOpenModal(true)
  }

  const handleDeleteLocationConfirm = () => {
    deleteLocation(location.id, {
      onSuccess: async () => {
        updateLocationVersion(null)
        addNotification({
          type: 'error',
          title: `Location Deleted`,
        })
        await queryClient.invalidateQueries()
      },
    })
  }

  const handleModalAction = () => {
    switch (modalAction) {
      case 'addVersion':
        handleAddNewVersionConfirm()
        break
      case 'deleteVersion':
        handleDeleteCurrentVersionConfirm()
        break
      case 'deleteLocation':
        handleDeleteLocationConfirm()
        break
    }
    setOpenModal(false)
  }

  const modalText =
    modalAction === 'addVersion'
      ? 'Are you sure you want to add a new version of this location?'
      : modalAction === 'deleteVersion'
        ? 'Are you sure you want to delete this version?'
        : 'Are you sure you want to delete this location?'

  const actionButtonText =
    modalAction === 'addVersion'
      ? 'Add New Version'
      : modalAction === 'deleteVersion'
        ? 'Delete Version'
        : 'Delete Location'

  return (
    <Paper sx={{ mt: 2, p: 2 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Typography variant="h4" marginBottom={'5px'} component={'p'}>
          {locationType?.name}
        </Typography>
        <Box>
          <Button
            variant="contained"
            startIcon={<ExpandMoreIcon />}
            sx={{ paddingX: '10px', marginLeft: '10px' }}
            onClick={handleClick}
          >
            Actions
          </Button>
          <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
            <MenuItem onClick={() => handleModalPopup('addVersion')}>
              Add New Version
            </MenuItem>
            <MenuItem onClick={() => handleModalPopup('deleteVersion')}>
              Delete This Version
            </MenuItem>
            <MenuItem onClick={() => handleModalPopup('deleteLocation')}>
              Delete This Location
            </MenuItem>
          </Menu>
        </Box>
      </Box>
      <Typography variant="h2" marginBottom={'10px'}>
        {location?.locationIdentifier} - {location?.primaryName} &{' '}
        {location?.secondaryName}
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'center', mt: '10px' }}>
        <InputLabel sx={{ marginRight: '10px' }} htmlFor="version-select-label">
          <Typography variant="h4" marginRight={'10px'} component={'p'}>
            Version
          </Typography>
        </InputLabel>
        <CustomSelect
          hideLabel
          id="version-select"
          label="version"
          name="version"
          data={locationsVersions}
          displayProperty="note"
          value={location.id}
          onChange={handleVersionChange}
          size="small"
          sx={{ color: 'primary.main', minWidth: '200px' }}
        />
      </Box>
      <Modal
        open={openModal}
        onClose={() => setOpenModal(false)}
        aria-labelledby="modal-modal-title"
        aria-describedby="modal-modal-description"
      >
        <Box sx={modalStyle}>
          <Typography id="modal-modal-title" sx={{ fontWeight: 'bold' }}>
            Confirm Action
          </Typography>
          <Typography id="modal-modal-description" sx={{ mt: 2 }}>
            {modalText}
          </Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenModal(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={handleModalAction}
              variant="contained"
              color={modalAction === 'addVersion' ? 'success' : 'error'}
              sx={{ ml: 2 }}
            >
              {actionButtonText}
            </Button>
          </Box>
        </Box>
      </Modal>
    </Paper>
  )
}

export default EditLocationHeader
