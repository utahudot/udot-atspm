import { useGetDeviceConfigurations } from '@/features/devices/api'
import { Device } from '@/features/locations/types'
import CircleIcon from '@mui/icons-material/Circle'
import MoreVertIcon from '@mui/icons-material/MoreVert'
import QuestionMarkIcon from '@mui/icons-material/QuestionMark'
import RampLeftIcon from '@mui/icons-material/RampLeft'
import SensorsIcon from '@mui/icons-material/Sensors'
import SettingsRemoteIcon from '@mui/icons-material/SettingsRemote'
import TrafficIcon from '@mui/icons-material/Traffic'
import VideocamIcon from '@mui/icons-material/Videocam'
import VideocamOutlinedIcon from '@mui/icons-material/VideocamOutlined'
import {
  Avatar,
  Box,
  Card,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'

const statusColorMap = {
  Unknown: { label: 'Unknown', color: 'default' },
  Decommissioned: { label: 'Decommissioned', color: 'warning' },
  Inactive: { label: 'Inactive', color: 'error' },
  Active: { label: 'Active', color: 'success' },
  Testing: { label: 'Testing', color: 'info' },
  Staging: { label: 'Staging', color: 'primary' },
}

const deviceTypeMap = {
  Unknown: { label: 'Unknown', icon: <QuestionMarkIcon /> },
  SignalController: { label: 'Signal Controller', icon: <TrafficIcon /> },
  RampController: { label: 'Ramp Controller', icon: <RampLeftIcon /> },
  AICamera: { label: 'AI Camera', icon: <VideocamIcon /> },
  FIRCamera: { label: 'FIR Camera', icon: <VideocamOutlinedIcon /> },
  LidarSensor: { label: 'Lidar Sensor', icon: <SensorsIcon /> },
  WavetronixSpeed: { label: 'Wavetronix Speed', icon: <SettingsRemoteIcon /> },
}

const StyledLabel = ({ children }: { children: React.ReactNode }) => (
  <Typography variant="overline" color={'#3B3B3B'} sx={{ width: '160px' }}>
    {children}
  </Typography>
)

interface DeviceCardProps {
  device: Device
  onEdit: (device: Device) => void
  onDelete: (deviceId: string) => void
}

const DeviceCard = ({ device, onEdit, onDelete }: DeviceCardProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()

  const deviceConfigurations = deviceConfigurationsData?.value

  if (!deviceConfigurations) return null

  device.deviceConfiguration = deviceConfigurations.find(
    (dc) => dc.id === device.deviceConfigurationId
  )

  const handleMenuClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
  }

  return (
    <Card
      key={device.id}
      sx={{
        p: 3,
        pr: 0,
        mb: 2,
        minWidth: '400px',
        maxWidth: '400px',
        height: '400px',
      }}
    >
      <Box mx={2}>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <Chip
            icon={<CircleIcon />}
            label={statusColorMap[device.deviceStatus]?.label}
            color={statusColorMap[device.deviceStatus]?.color}
            variant="outlined"
          />
          <IconButton
            id="device-options"
            onClick={handleMenuClick}
            aria-label="device-options"
          >
            <MoreVertIcon />
          </IconButton>
          <Menu
            MenuListProps={{ 'aria-labelledby': 'device-options' }}
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
          >
            <MenuItem
              onClick={() => {
                handleMenuClose()
                onEdit(device)
              }}
            >
              Edit
            </MenuItem>
            <MenuItem
              onClick={() => {
                handleMenuClose()
                onDelete(device.id)
              }}
            >
              Delete
            </MenuItem>
          </Menu>
        </Box>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            mt: 3,
          }}
        >
          <Avatar sx={{ width: 40, height: 40, marginRight: '10px' }}>
            {deviceTypeMap[device.deviceType]?.icon}
          </Avatar>
          <Box sx={{ textAlign: 'left' }}>
            <Typography variant="h4" fontWeight={'bold'} component={'h3'}>
              {deviceTypeMap[device.deviceType].label}
            </Typography>
          </Box>
        </Box>

        <Box mt={2} display={'flex'} flexDirection={'column'}>
          <Box display={'flex'} justifyContent={'flex-start'}>
            <StyledLabel>Manufacturer</StyledLabel>
            <Typography variant="body1">
              {device.deviceConfiguration?.product?.manufacturer}
            </Typography>
          </Box>
          <Box display={'flex'} justifyContent={'flex-start'}>
            <StyledLabel>Model</StyledLabel>
            <Typography variant="body1">
              {device.deviceConfiguration?.product?.model}
            </Typography>
          </Box>
          <Box display={'flex'} justifyContent={'flex-start'}>
            <StyledLabel>IP Address</StyledLabel>
            <Typography variant="body1">{device.ipaddress}</Typography>
          </Box>
          <Box display={'flex'} justifyContent={'flex-start'}>
            <StyledLabel>Logging Enabled</StyledLabel>
            <Typography variant="body1">
              {device.loggingEnabled ? 'Yes' : 'No'}
            </Typography>
          </Box>
          <Box display={'flex'} justifyContent={'flex-start'}>
            <StyledLabel>Notes</StyledLabel>
          </Box>
          <Typography variant="body1">{device.notes}</Typography>
        </Box>
      </Box>
    </Card>
  )
}

export default DeviceCard
