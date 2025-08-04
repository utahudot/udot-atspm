import {
  Location,
  useDeleteLocationAllVersionsFromKey,
  useDeleteLocationSetLocationTodFromKey,
  useGetLocationAllVersionsOfLocationFromIdentifier,
  useGetLocationCopyLocationToNewVersionFromKey,
  useGetLocationType,
} from '@/api/config'
import CustomSelect from '@/components/customSelect'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { getLocationTypeConfig } from '@/features/locations/utils'
import { getLocation } from '@/pages/admin/locations/[[...id]]'
import { useNotificationStore } from '@/stores/notifications'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Avatar,
  Box,
  Button,
  Divider,
  Icon,
  InputLabel,
  Menu,
  MenuItem,
  Modal,
  Paper,
  SelectChangeEvent,
  TextField,
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

  const { data: locationTypeData } = useGetLocationType()
  const { mutate: copyVersion } =
    useGetLocationCopyLocationToNewVersionFromKey()
  const { mutate: deleteVersion } = useDeleteLocationSetLocationTodFromKey()
  const { mutate: deleteLocation } = useDeleteLocationAllVersionsFromKey()

  const { data: versionsData, refetch: fetchLocationVersions } =
    useGetLocationAllVersionsOfLocationFromIdentifier(
      `'${location.locationIdentifier}'`,
      { enabled: false }
    )

  const locationVersions = versionsData?.value

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const open = Boolean(anchorEl)

  const [openDeleteModal, setOpenDeleteModal] = useState(false)
  const [deleteAction, setDeleteAction] = useState('')

  const [openAddDialog, setOpenAddDialog] = useState(false)
  const [newVersionLabel, setNewVersionLabel] = useState('')

  if (!location) return null

  const updateLocationVersion = async (newLoc: Location | null) => {
    if (!newLoc?.id) return
    const newLocation = await getLocation(newLoc.id)
    setLocation(newLocation)
  }

  function buildDisplayName(loc: Location) {
    const { locationIdentifier, primaryName, secondaryName } = loc
    let displayName = locationIdentifier || ''
    if (primaryName) displayName += ` - ${primaryName}`
    if (primaryName && secondaryName) displayName += ` & ${secondaryName}`
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

  const handleClick = (e: React.MouseEvent<HTMLButtonElement>) =>
    setAnchorEl(e.currentTarget)
  const handleClose = () => setAnchorEl(null)

  const handleAddNewVersion = () => {
    setOpenAddDialog(true)
    handleClose()
  }

  const handleDeletePopup = (action: string) => {
    setDeleteAction(action)
    setOpenDeleteModal(true)
    handleClose()
  }

  const handleVersionChange = (e: SelectChangeEvent<unknown>) => {
    const selectedId = e.target.value
    const newLocation = locationVersions?.find((ver) => ver.id === selectedId)
    updateLocationVersion(newLocation || null)
  }

  const handleAddNewVersionConfirm = () => {
    copyVersion(
      { key: location.id, params: { newVersionLabel } },
      {
        onSuccess: (newLoc) => {
          updateLocationVersion(newLoc)
          fetchLocationVersions()
          addNotification({ type: 'success', title: 'New Version Added' })
        },
      }
    )
    setOpenAddDialog(false)
    setNewVersionLabel('')
  }

  const handleDeleteCurrentVersionConfirm = () => {
    deleteVersion(
      { key: location.id },
      {
        onSuccess: () => {
          fetchLocationVersions()
          updateLocationVersion(
            locationVersions?.find((ver) => ver.id !== location.id) || null
          )
          addNotification({ type: 'success', title: 'Version Deleted' })
        },
      }
    )
    setOpenDeleteModal(false)
  }

  const handleDeleteLocationConfirm = () => {
    if (!location?.locationIdentifier) return
    deleteLocation(
      { key: location.locationIdentifier },
      {
        onSuccess: async () => {
          updateLocationVersion(null)
          setLocation(null)
          addNotification({ type: 'success', title: 'Location Deleted' })
          await queryClient.invalidateQueries()
        },
      }
    )
    setOpenDeleteModal(false)
  }

  const deleteModalText =
    deleteAction === 'deleteVersion'
      ? 'Are you sure you want to delete this version?'
      : 'Are you sure you want to delete this location?'

  const deleteButtonText =
    deleteAction === 'deleteVersion' ? 'Delete Version' : 'Delete Location'

  const handleDeleteAction = () => {
    if (deleteAction === 'deleteVersion') handleDeleteCurrentVersionConfirm()
    if (deleteAction === 'deleteLocation') handleDeleteLocationConfirm()
  }

  const locationType = locationTypeData?.value.find(
    (type) => type.id === location.locationTypeId
  )
  const locationTypeConfig = getLocationTypeConfig(locationType?.id)

  return (
    <Paper sx={{ mt: 2, p: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
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
            <MenuItem onClick={handleAddNewVersion}>Add New Version</MenuItem>
            <MenuItem onClick={() => handleDeletePopup('deleteVersion')}>
              Delete This Version
            </MenuItem>
            <Divider />
            <MenuItem onClick={() => handleDeletePopup('deleteLocation')}>
              Delete This Location
            </MenuItem>
          </Menu>
        </Box>
      </Box>

      <Typography variant="h2" marginBottom={'10px'}>
        {buildDisplayName(location)}
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', mt: '10px' }}>
        <InputLabel sx={{ marginRight: '10px' }}>
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

      {/* Add Version Dialog */}
      <Modal open={openAddDialog} onClose={() => setOpenAddDialog(false)}>
        <Box sx={modalStyle}>
          <Typography sx={{ fontWeight: 'bold' }}>Add New Version</Typography>
          <TextField
            label="Version Label"
            fullWidth
            sx={{ mt: 2 }}
            value={newVersionLabel}
            onChange={(e) => setNewVersionLabel(e.target.value)}
          />
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenAddDialog(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={handleAddNewVersionConfirm}
              variant="contained"
              color="success"
              sx={{ ml: 2 }}
              disabled={!newVersionLabel.trim()}
            >
              Add New Version
            </Button>
          </Box>
        </Box>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal open={openDeleteModal} onClose={() => setOpenDeleteModal(false)}>
        <Box sx={modalStyle}>
          <Typography sx={{ fontWeight: 'bold' }}>Confirm Action</Typography>
          <Typography sx={{ mt: 2 }}>{deleteModalText}</Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenDeleteModal(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={handleDeleteAction}
              variant="contained"
              color="error"
              sx={{ ml: 2 }}
            >
              {deleteButtonText}
            </Button>
          </Box>
        </Box>
      </Modal>
    </Paper>
  )
}
