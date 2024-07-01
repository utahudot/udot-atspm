import { useGetDeviceConfigurations } from '@/features/devices/api'
import {
  useDeleteDevice,
  useGetDevicesForLocation,
} from '@/features/devices/api/devices'
import { Device } from '@/features/devices/types'
import DeviceCard from '@/features/locations/components/editLocation/DeviceCard'
import DeviceModal from '@/features/locations/components/editLocation/NewDeviceModal'
import AddIcon from '@mui/icons-material/Add'
import { Avatar, Box, Button, Modal, Typography, useTheme } from '@mui/material'
import { useState } from 'react'

interface EditDevicesProps {
  locationId: string
}

const EditDevices = ({ locationId }: EditDevicesProps) => {
  const theme = useTheme()
  const [isModalOpen, setModalOpen] = useState(false)
  const [currentDevice, setCurrentDevice] = useState<Device | null>(null)
  const [openDeleteModal, setOpenDeleteModal] = useState(false)
  const [deleteDeviceId, setDeleteDeviceId] = useState<string | null>(null)

  const { data: devicesData, refetch: refetchDevices } =
    useGetDevicesForLocation(locationId)
  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()
  const { mutate: deleteDevice } = useDeleteDevice()

  if (!deviceConfigurationsData?.value || !devicesData?.value) {
    return <Typography variant="h6">Loading...</Typography>
  }

  const handleAddClick = () => {
    setCurrentDevice(null)
    setModalOpen(true)
  }

  const handleEditClick = (device: Device) => {
    setCurrentDevice(device)
    setModalOpen(true)
  }

  const handleDelete = (deviceId: string) => {
    setDeleteDeviceId(deviceId)
    setOpenDeleteModal(true)
  }

  const confirmDeleteDevice = () => {
    if (deleteDeviceId) {
      deleteDevice(deleteDeviceId, { onSuccess: refetchDevices })
    }
    setOpenDeleteModal(false)
  }

  return (
    <Box
      sx={{
        display: 'flex',
        overflowX: 'auto',
        flexWrap: 'nowrap',
        gap: '30px',
        marginTop: '10px',
      }}
    >
      {devicesData.value.map((device) => (
        <DeviceCard
          key={device.id}
          device={device}
          onEdit={handleEditClick}
          onDelete={() => handleDelete(device.id)}
        />
      ))}
      <Button
        onClick={handleAddClick}
        sx={{
          padding: 2,
          minWidth: '400px',
          height: '400px',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          border: `4px dashed ${theme.palette.primary.main}`,
        }}
      >
        <Avatar sx={{ bgcolor: theme.palette.primary.main, mb: 1 }}>
          <AddIcon />
        </Avatar>
        <Typography variant="h6" sx={{ marginTop: 2 }} component={'p'}>
          Add New Device
        </Typography>
      </Button>

      {isModalOpen && (
        <DeviceModal
          onClose={() => setModalOpen(false)}
          device={currentDevice}
          locationId={locationId}
          refetchDevices={refetchDevices}
        />
      )}

      

      <Modal
        open={openDeleteModal}
        onClose={() => setOpenDeleteModal(false)}
        aria-labelledby="delete-confirmation"
        aria-describedby="confirm-delete-approach"
      >
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 400,
            bgcolor: 'background.paper',
            boxShadow: 24,
            p: 4,
          }}
        >
          <Typography id="delete-confirmation" sx={{ fontWeight: 'bold' }}>
            Confirm Delete
          </Typography>
          <Typography id="confirm-delete-approach" sx={{ mt: 2 }}>
            Are you sure you want to delete this device?
          </Typography>
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={() => setOpenDeleteModal(false)} color="inherit">
              Cancel
            </Button>
            <Button
              onClick={confirmDeleteDevice}
              color="error"
              variant="contained"
            >
              Delete Device
            </Button>
          </Box>
        </Box>
      </Modal>
    </Box>
  )
}

export default EditDevices
