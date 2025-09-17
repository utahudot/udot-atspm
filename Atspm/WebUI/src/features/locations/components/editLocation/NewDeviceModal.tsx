import { usePutDeviceFromKey } from '@/api/config'
import { useGetDeviceConfigurations } from '@/features/devices/api'
import { useCreateDevice } from '@/features/devices/api/devices'
import { DeviceConfiguration } from '@/features/devices/types'
import { useGetProducts } from '@/features/products/api'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { useNotificationStore } from '@/stores/notifications'
import { zodResolver } from '@hookform/resolvers/zod'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Modal,
  OutlinedInput,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { Controller, useFieldArray, useForm } from 'react-hook-form'
import { z } from 'zod'

export const modalStyle = {
  position: 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 800,
  maxWidth: '95%',
  bgcolor: 'background.paper',
  border: 'none',
  borderRadius: '10px',
  boxShadow: 24,
  p: 4,
  '@media (max-width: 400px)': {
    width: '100%',
  },
}

const knownDeviceKeys = new Set([
  'id',
  'loggingEnabled',
  'ipaddress',
  'deviceStatus',
  'deviceType',
  'notes',
  'locationId',
  'deviceConfigurationId',
  'deviceIdentifier',
  'location',
  'deviceConfiguration',
  'created',
  'modified',
  'createdBy',
  'modifiedBy',
])

const deviceSchema = z.object({
  id: z.coerce.number().nullable().optional(),
  deviceIdentifier: z.string().optional(),
  loggingEnabled: z.boolean().default(true),
  ipaddress: z.string().optional(),
  deviceStatus: z.string().nonempty({ message: 'Status is required' }),
  notes: z
    .string()
    .max(512, { message: 'Notes must be 512 characters or less' })
    .optional(),
  deviceType: z.string().nonempty({ message: 'Device type is required' }),
  deviceConfigurationId: z.coerce.number({
    required_error: 'Configuration is required',
  }),
  productId: z.coerce.number({
    required_error: 'Product is required',
  }),
  locationId: z.number(),
  deviceProperties: z
    .array(z.object({ key: z.string(), value: z.string() }))
    .nullable(),
})

export interface NewDeviceModalProps {
  onClose: () => void
  device?: any | null
  locationId: string
  refetchDevices: () => void
}

const DeviceModal = ({
  onClose,
  device,
  locationId,
  refetchDevices,
}: NewDeviceModalProps) => {
  const { data: productsData } = useGetProducts()
  const { data: deviceConfigurationsData } = useGetDeviceConfigurations()
  const { mutate: updateDevice } = usePutDeviceFromKey()
  const { mutate: createDevice } = useCreateDevice()
  const { data: deviceTypes } = useConfigEnums(ConfigEnum.DeviceTypes)
  const { data: deviceStatus } = useConfigEnums(ConfigEnum.DeviceStatus)
  const { addNotification } = useNotificationStore()

  const deviceConfigurations = deviceConfigurationsData?.value
  const products = productsData?.value

  const [filteredConfigurations, setFilteredConfigurations] = useState<
    DeviceConfiguration[]
  >([])

  const defaultDeviceProperties = device
    ? Object.entries(device)
        .filter(([key]) => !knownDeviceKeys.has(key))
        .map(([key, value]) => ({ key, value: String(value) }))
    : []

  const {
    control,
    register,
    handleSubmit,
    watch,
    setValue,
    getValues,
    reset,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(deviceSchema),
    defaultValues: {
      id: device?.id ?? null,
      deviceIdentifier: device?.deviceIdentifier ?? '',
      loggingEnabled: device?.loggingEnabled ?? true,
      ipaddress: device?.ipaddress ?? '',
      deviceStatus: device?.deviceStatus ?? 'Active',
      notes: device?.notes ?? '',
      deviceType: device?.deviceType ?? '',
      deviceConfigurationId: device?.deviceConfiguration?.id ?? '',
      productId: device?.deviceConfiguration?.product?.id ?? '',
      locationId: device?.locationId ?? Number(locationId),
      deviceProperties: device?.deviceProperties ?? defaultDeviceProperties,
    },
  })

  const {
    fields: devicePropertiesFields,
    append: appendDeviceProperty,
    remove: removeDeviceProperty,
  } = useFieldArray({
    control,
    name: 'deviceProperties',
  })

  const selectedProductId = watch('productId')

  useEffect(() => {
    if (selectedProductId && deviceConfigurations) {
      const filtered = deviceConfigurations.filter(
        (config) => config.productId === Number(selectedProductId)
      )
      setFilteredConfigurations(filtered)
      const currentConfigId = getValues('deviceConfigurationId')
      if (!filtered.some((config) => config.id === Number(currentConfigId))) {
        setValue('deviceConfigurationId', '')
      }
    } else {
      setFilteredConfigurations([])
      setValue('deviceConfigurationId', '')
    }
  }, [selectedProductId, deviceConfigurations, setValue, getValues])

  const onSubmit = (data: z.infer<typeof deviceSchema>) => {
    const flattenedProps =
      data.deviceProperties?.reduce(
        (acc, { key, value }) => {
          if (key) {
            acc[key] = value
          }
          return acc
        },
        {} as Record<string, any>
      ) || {}

    const { id, productId, deviceProperties, ...rest } = data
    const newDeviceDTO = { ...rest, ...flattenedProps }
    const updateDeviceDTO = { id, ...rest, ...flattenedProps }

    if (data.id) {
      updateDevice(
        { data: updateDeviceDTO, key: data.id },
        {
          onSuccess: () => {
            addNotification({ title: 'Device Updated', type: 'success' })

            refetchDevices()
            onClose()
          },
          onError: (error) => {
            addNotification({ title: 'Device Update Failed', type: 'error' })
          },
        }
      )
    } else {
      createDevice(newDeviceDTO, {
        onSuccess: () => {
          refetchDevices()
          onClose()
        },
        onError: (error) => {
          addNotification({ title: 'Device Creation Failed', type: 'error' })
        },
      })
    }
  }

  // Close handler
  const handleClose = () => {
    reset()
    onClose()
  }

  if (!products || !deviceTypes || !deviceConfigurations) return null

  return (
    <Modal open={true} onClose={handleClose}>
      <Box sx={modalStyle}>
        <Typography variant="h4" sx={{ mb: 2 }}>
          {device ? 'Edit Device' : 'Add New Device'}
        </Typography>

        <Box
          sx={{
            display: 'flex',
            flexDirection: { xs: 'column', md: 'row' },
            gap: 3,
          }}
        >
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>
              General
            </Typography>
            <Controller
              name="productId"
              control={control}
              render={({ field }) => (
                <FormControl
                  fullWidth
                  sx={{ mb: 2 }}
                  error={!!errors.productId}
                >
                  <InputLabel>Product</InputLabel>
                  <Select {...field} label="Product">
                    {products.map((product) => (
                      <MenuItem key={product.id} value={product.id}>
                        {product.manufacturer} - {product.model}
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.productId && (
                    <Typography variant="caption" color="error">
                      {String(errors.productId.message)}
                    </Typography>
                  )}
                </FormControl>
              )}
            />

            {/* Device Configuration */}
            <Controller
              name="deviceConfigurationId"
              control={control}
              render={({ field }) => (
                <FormControl
                  fullWidth
                  sx={{ mb: 2 }}
                  error={!!errors.deviceConfigurationId}
                >
                  <InputLabel>
                    {selectedProductId
                      ? 'Configurations'
                      : 'Please select a product'}
                  </InputLabel>
                  <Select
                    {...field}
                    label={
                      selectedProductId
                        ? 'Configurations'
                        : 'Please select a product'
                    }
                    disabled={!selectedProductId}
                  >
                    {filteredConfigurations.map((config) => (
                      <MenuItem key={config.id} value={config.id}>
                        {config.description}
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.deviceConfigurationId && (
                    <Typography variant="caption" color="error">
                      {String(errors.deviceConfigurationId.message)}
                    </Typography>
                  )}
                </FormControl>
              )}
            />

            <Controller
              name="deviceType"
              control={control}
              render={({ field }) => (
                <FormControl
                  fullWidth
                  sx={{ mb: 2 }}
                  error={!!errors.deviceType}
                >
                  <InputLabel>Device Type</InputLabel>
                  <Select {...field} label="Device Type">
                    {deviceTypes.map((type) => (
                      <MenuItem key={type.name} value={type.name}>
                        {type.name}
                      </MenuItem>
                    ))}
                  </Select>
                  {errors.deviceType && (
                    <Typography variant="caption" color="error">
                      {String(errors.deviceType.message)}
                    </Typography>
                  )}
                </FormControl>
              )}
            />

            {/* Status */}
            <Controller
              name="deviceStatus"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel>Status</InputLabel>
                  <Select {...field} label="Status">
                    {deviceStatus?.map((status) => (
                      <MenuItem key={status.name} value={status.name}>
                        {status.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />

            {/* IP Address */}
            <FormControl fullWidth sx={{ mb: 2 }}>
              <InputLabel htmlFor="ip-input">IP Address</InputLabel>
              <OutlinedInput
                id="ip-input"
                label="IP Address"
                {...register('ipaddress')}
              />
            </FormControl>

            <TextField
              fullWidth
              multiline
              label="Device Identifier"
              sx={{ mb: 2 }}
              maxRows={6}
              error={!!errors.notes}
              helperText={errors.notes ? 'String(errors.notes.message) ' : ''}
              {...register('deviceIdentifier')}
            />

            <TextField
              fullWidth
              multiline
              label="Notes"
              sx={{ mb: 2 }}
              maxRows={6}
              error={!!errors.notes}
              helperText={errors.notes ? String(errors.notes.message) : ''}
              {...register('notes')}
            />

            {/* Logging */}
            <Controller
              name="loggingEnabled"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Checkbox {...field} checked={field.value} />}
                  label="Enable Logging"
                />
              )}
            />
          </Box>

          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" sx={{ mb: 1 }}>
              Device Properties
            </Typography>
            {devicePropertiesFields.map((field, index) => (
              <Box
                key={field.id}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                  mb: 1,
                }}
              >
                <TextField
                  {...register(`deviceProperties.${index}.key`)}
                  margin="dense"
                  label={`Key ${index + 1}`}
                  fullWidth
                />
                <TextField
                  {...register(`deviceProperties.${index}.value`)}
                  margin="dense"
                  label={`Value ${index + 1}`}
                  fullWidth
                />
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => removeDeviceProperty(index)}
                  sx={{ mt: 1 }}
                >
                  <DeleteIcon />
                </IconButton>
              </Box>
            ))}
            <Button
              variant="outlined"
              size="small"
              onClick={() => appendDeviceProperty({ key: '', value: '' })}
            >
              + Device Property
            </Button>
          </Box>
        </Box>

        {/* Action Buttons */}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
          <Button onClick={handleClose} sx={{ mr: 1 }}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit(onSubmit)}
            variant="contained"
            color="primary"
          >
            {device ? 'Update Device' : 'Add Device'}
          </Button>
        </Box>
      </Box>
    </Modal>
  )
}

export default DeviceModal
