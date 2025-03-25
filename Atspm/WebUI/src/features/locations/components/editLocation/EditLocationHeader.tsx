import { useGetLocationAllVersionsOfLocationFromIdentifier } from '@/api/config/aTSPMConfigurationApi'
import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import CustomSelect from '@/components/customSelect'
import { useLocationTypes } from '@/features/locations/api'
import {
  useCopyLocationToNewVersion,
  useDeleteVersion,
  useSetLocationToBeDeleted,
} from '@/features/locations/api/location'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { getLocationTypeConfig } from '@/features/locations/utils'
import { getLocation } from '@/pages/admin/locations'
import { useNotificationStore } from '@/stores/notifications'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Avatar,
  Box,
  Button,
  Icon,
  InputLabel,
  Menu,
  MenuItem,
  Modal,
  Paper,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'
import { useQueryClient } from 'react-query'

const modalStyle = {
  position: 'absolute' as const,
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

export default function EditLocationHeader() {
  const { location, setLocation } = useLocationStore()
  const { addNotification } = useNotificationStore()

  const queryClient = useQueryClient()
  const { data: locationTypeData } = useLocationTypes()
  const { mutate: copyVersion } = useCopyLocationToNewVersion(location.id)
  const { mutate: deleteVersion } = useDeleteVersion()
  const { mutate: deleteLocation } = useSetLocationToBeDeleted(location.id)

  const { data: versionsData, refetch: fetchLocationVersions } =
    useGetLocationAllVersionsOfLocationFromIdentifier(
      `'${location.locationIdentifier}'`,
      { enabled: false }
    )

  const locationVersions = versionsData?.value

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const open = Boolean(anchorEl)

  const [openModal, setOpenModal] = useState(false)
  const [modalAction, setModalAction] = useState('')

  if (!location) return null

  const updateLocationVersion = async (newLoc: Location | null) => {
    if (!newLoc) return

    setLocation(await getLocation(newLoc.id))
  }

  function buildDisplayName(loc: Location) {
    const { locationIdentifier, primaryName, secondaryName } = loc
    let displayName = locationIdentifier || ''
    if (primaryName) {
      displayName += ` - ${primaryName}`
    }
    if (primaryName && secondaryName) {
      displayName += ` & ${secondaryName}`
    }
    return displayName
  }

  const locationsVersions = locationVersions?.map((ver: Location) => ({
    id: ver.id,
    note: formatVersionNote(ver.note, ver.start),
    startDate: ver.start,
  }))

  function formatVersionNote(note: string, date: string) {
    const [year, month, day] = date.split('T')[0].split('-')
    return `${+month}/${+day}/${year} - ${note}`
  }

  const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(e.currentTarget)
  }
  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleModalPopup = (action: string) => {
    setModalAction(action)
    setOpenModal(true)
    handleClose()
  }

  const handleVersionChange = (e: SelectChangeEvent<unknown>) => {
    const selectedId = e.target.value
    const newLocation = locationVersions?.find((ver) => ver.id === selectedId)
    updateLocationVersion(newLocation || null)
  }

  const handleAddNewVersionConfirm = () => {
    copyVersion(location.id, {
      onSuccess: (newLoc) => {
        updateLocationVersion(newLoc)
        fetchLocationVersions()
      },
    })
    setOpenModal(false)
  }

  const handleDeleteCurrentVersionConfirm = () => {
    deleteVersion(location.id, {
      onSuccess: () => {
        fetchLocationVersions()
        updateLocationVersion(
          locationVersions?.find((ver) => ver.id !== location.id) || null
        )
      },
    })
    setOpenModal(false)
  }

  const handleDeleteLocationConfirm = () => {
    deleteLocation(location.id, {
      onSuccess: async () => {
        updateLocationVersion(null)
        addNotification({
          type: 'error',
          title: 'Location Deleted',
        })
        await queryClient.invalidateQueries()
      },
    })
    setOpenModal(false)
  }

  // Decide which text/action to use based on the chosen menu item
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
  }

  const locationType = locationTypeData?.value.find(
    (type) => type.id === location.locationTypeId
  )
  const locationTypeConfig = getLocationTypeConfig(locationType?.id)

  return (
    <Paper sx={{ mt: 2, p: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        {/* Left side: Location Type + Avatar */}
        <Box sx={{ display: 'flex', alignItems: 'flex-end', mb: '10px' }}>
          <Avatar
            sx={{
              mr: '10px',
              width: 30,
              height: 30,
              bgcolor: locationTypeConfig?.color,
            }}
            variant="rounded"
          >
            <Icon
              component={locationTypeConfig?.MuiIcon}
              sx={{ ml: locationType?.id === 2 ? '2px' : '0px' }}
            />
          </Avatar>
          <Typography variant="h4" component={'p'}>
            {locationType?.name}
          </Typography>
        </Box>

        {/* Right side: Actions button */}
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

      {/* Main Title */}
      <Typography variant="h2" marginBottom={'10px'}>
        {buildDisplayName(location)}
      </Typography>

      {/* Version Dropdown */}
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

      {/* Modal Confirmation */}
      <Modal open={openModal} onClose={() => setOpenModal(false)}>
        <Box sx={modalStyle}>
          <Typography sx={{ fontWeight: 'bold' }}>Confirm Action</Typography>
          <Typography sx={{ mt: 2 }}>{modalText}</Typography>
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
