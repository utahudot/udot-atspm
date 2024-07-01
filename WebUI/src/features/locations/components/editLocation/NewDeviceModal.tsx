import { useGetDeviceConfigurations } from '@/features/devices/api'
import {
  useCreateDevice,
  useUpdateDevice,
} from '@/features/devices/api/devices'
import { Device, DeviceConfiguration } from '@/features/devices/types'
import { useGetProducts } from '@/features/products/api'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import {
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Modal,
  OutlinedInput,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import React, { useEffect, useState } from 'react'

export const modalStyle = {
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
  '@media (max-width: 400px)': {
    width: '100%',
  },
}

interface NewDeviceModalProps {
  onClose: () => void
  device?: Device
  locationId: string
  refetchDevices: () => void
}

const DeviceModal = ({
  onClose,
  device,
  locationId,
  refetchDevices,
}: NewDeviceModalProps) => {
  const initialState = {
    id: null,
    loggingEnabled: true,
    ipaddress: '',
    deviceStatus: 'Active',
    notes: '',
    deviceType: '',
    deviceConfigurationId: null,
    product: '',
    locationId: locationId,
  }

  const [deviceData, setDeviceData] = useState<Device>(device || initialState)
  const [filteredConfigurations, setFilteredConfigurations] = useState<
    DeviceConfiguration[]
  >([])
  const [errors, setErrors] = useState({
    product: false,
    deviceConfiguration: false,
    deviceType: false,
    deviceStatus: false,
  })

  const { data: productsData } = useGetProducts()
  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()
  const { mutate: updateDevice } = useUpdateDevice()
  const { mutate: createDevice } = useCreateDevice()
  const deviceTypesData = useConfigEnums(ConfigEnum.DeviceTypes)
  const deviceStatusData = useConfigEnums(ConfigEnum.DeviceStatus)

  const deviceConfigurations = deviceConfigurationsData?.value
  const products = productsData?.value
  const deviceTypes = deviceTypesData?.data?.members.map(
    (member) => member.name
  )
  const deviceStatus = deviceStatusData?.data?.members.map(
    (member) => member.name
  )

  useEffect(() => {
    const product = deviceData.deviceConfiguration?.product
    if (!product || !deviceConfigurations) return

    const configurations = deviceConfigurations.filter(
      (config) => config.productId === product.id
    )
    setFilteredConfigurations(configurations)
  }, [deviceData.deviceConfiguration?.product, deviceConfigurations])

  const handleChange = (
    event: React.ChangeEvent<{ name?: string; value: unknown }>
  ) => {
    const name = event.target.name as keyof Device
    const value = event.target.value as string | boolean
    setDeviceData((prev) => ({ ...prev, [name]: value }))
    
    if (value) {
      setErrors((prev) => ({ ...prev, [name]: false }))
    }
  }


  const handleProductChange = (
    event: React.ChangeEvent<{ value: unknown }>
  ) => {
    const productId = event.target.value as string
    const product = products?.find((p) => p.id === parseInt(productId))
    setDeviceData((prev) => ({
      ...prev,
      deviceConfiguration: { ...prev.deviceConfiguration, product },
    }))
    setFilteredConfigurations([])

    if (product) {
      setErrors((prev) => ({ ...prev, product: false }))
    }
  }


  const handleConfigurationChange = (
    event: React.ChangeEvent<{ value: unknown }>
  ) => {
    const configurationId = event.target.value as string
    const configuration = deviceConfigurations?.find(
      (c) => c.id === parseInt(configurationId)
    )
    setDeviceData((prev) => ({ ...prev, deviceConfiguration: configuration }))

    if (configuration) {
      setErrors((prev) => ({ ...prev, deviceConfiguration: false }))
    }
  }

  const handleSave = () => {
    const newErrors = {
      product: !deviceData.deviceConfiguration?.product,
      deviceConfiguration: !deviceData.deviceConfiguration,
      deviceType: !deviceData.deviceType,
      deviceStatus: !deviceData.deviceStatus,
    }
    setErrors(newErrors)

    if (
      !newErrors.product &&
      !newErrors.deviceConfiguration &&
      !newErrors.deviceType &&
      !newErrors.deviceStatus
    ) {
      if (deviceData.id) {
        console.log('Updating device', deviceData)
        const { location, product, ...deviceDTO } = deviceData
        updateDevice(
          { data: deviceDTO, id: deviceData.id },
          {
            onSuccess: () => {
              refetchDevices()
              onClose()
            },
          }
        )
      } else {
        if (!deviceData.deviceConfiguration) return
        deviceData.deviceConfigurationId = deviceData?.deviceConfiguration?.id
        const { deviceConfiguration, product, id, ...deviceDTO } = deviceData
        createDevice(deviceDTO, {
          onSuccess: () => {
            refetchDevices()
            onClose()
          },
        })
      }
    }
  }

  const handleClose = () => {
    setDeviceData(initialState)
    onClose()
  }

  if (!products || !deviceTypes || !deviceConfigurations) return null

  return (
    <Modal open={true} onClose={handleClose}>
      <Box sx={modalStyle}>
        <Typography variant="h4" sx={{ mb: 2 }}>
          {device ? 'Edit Device' : 'Add New Device'}
        </Typography>
        <FormControl fullWidth sx={{ mb: 2 }} error={errors.product}>
          <InputLabel>Product</InputLabel>
          <Select
            name="product"
            value={
              deviceData?.deviceConfiguration?.product?.id?.toString() || ''
            }
            label="Product"
            onChange={handleProductChange}
          >
            {products.map((product) => (
              <MenuItem key={product.id} value={product.id}>
                {product.manufacturer} - {product.model}
              </MenuItem>
            ))}
          </Select>
          {errors.product}
        </FormControl>
        <FormControl
          fullWidth
          sx={{ mb: 2 }}
          error={errors.deviceConfiguration}
        >
          <InputLabel>Configuration</InputLabel>
          <Select
            name="deviceConfiguration"
            value={deviceData?.deviceConfiguration?.id?.toString() || ''}
            label="Configuration"
            onChange={handleConfigurationChange}
            disabled={!deviceData.deviceConfiguration?.product}
          >
            {filteredConfigurations?.map((config) => (
              <MenuItem key={config.id} value={config.id}>
                {config.firmware}
              </MenuItem>
            ))}
          </Select>
          {errors.deviceConfiguration}
        </FormControl>
        <FormControl fullWidth sx={{ mb: 2 }} error={errors.deviceType}>
          <InputLabel>Device Type</InputLabel>
          <Select
            name="deviceType"
            value={deviceData.deviceType}
            label="Device Type"
            onChange={handleChange}
          >
            {deviceTypes?.map((type) => (
              <MenuItem key={type} value={type}>
                {type}
              </MenuItem>
            ))}
          </Select>
          {errors.deviceType}
        </FormControl>
        <FormControl fullWidth sx={{ mb: 2 }} error={errors.deviceStatus}>
          <InputLabel>Status</InputLabel>
          <Select
            name="deviceStatus"
            value={deviceData.deviceStatus}
            label="Status"
            onChange={handleChange}
          >
            {deviceStatus?.map((type) => (
              <MenuItem key={type} value={type}>
                {type}
              </MenuItem>
            ))}
          </Select>
          {errors.deviceStatus}
        </FormControl>
        <FormControl fullWidth sx={{ mb: 2 }}>
          <InputLabel htmlFor="ip-input">IP Address</InputLabel>
          <OutlinedInput
            id="ip-input"
            label="IP Address"
            value={deviceData.ipaddress || ''}
            onChange={handleChange}
            name="ipaddress"
          />
        </FormControl>
        <TextField
          fullWidth
          multiline
          label="Notes"
          name="notes"
          value={deviceData.notes}
          onChange={handleChange}
          sx={{ mb: 2 }}
        />
        <FormControlLabel
          control={
            <Checkbox
              name="loggingEnabled"
              checked={deviceData.loggingEnabled || false}
              onChange={(e) =>
                setDeviceData((prev) => ({
                  ...prev,
                  loggingEnabled: e.target.checked,
                }))
              }
            />
          }
          label="Enable Logging"
        />
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
          <Button onClick={onClose} sx={{ mr: 1 }}>
            Cancel
          </Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            {device ? 'Update Device' : 'Add Device'}
          </Button>
        </Box>
      </Box>
    </Modal>
  )
}

export default DeviceModal
