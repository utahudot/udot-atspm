import { DeviceConfiguration } from '@/features/devices/types/index'
import { useGetProducts } from '@/features/products/api'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'

interface ModalProps {
  data?: DeviceConfiguration
  isOpen: boolean
  onClose: () => void
  onSave: (device: DeviceConfiguration) => void
}

// Define Zod schema for form validation
const deviceConfigSchema = z.object({
  id: z.number().nullable().optional(),
  firmware: z.string().min(1, 'Firmware is required'),
  notes: z.string().optional(),
  protocol: z.string(),
  port: z.number().nullable(),
  directory: z.string(),
  connectionTimeout: z.number().nullable(),
  operationTimeout: z.number().nullable(),
  userName: z.string(),
  password: z.string(),
  productId: z
    .number()
    .nullable()
    .refine((val) => val !== null, 'Product is required'),
})

type DeviceConfigFormData = z.infer<typeof deviceConfigSchema>

const DeviceConfigModal = ({
  data: deviceConfiguration,
  isOpen,
  onClose,
  onSave,
}: ModalProps) => {
  const { data: productData } = useGetProducts()
  const { data: transportProtocols } = useConfigEnums(
    ConfigEnum.TransportProtocols
  )
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<DeviceConfigFormData>({
    resolver: zodResolver(deviceConfigSchema),
    defaultValues: {
      firmware: deviceConfiguration?.firmware || '',
      notes: deviceConfiguration?.notes || '',
      protocol: deviceConfiguration?.protocol || '',
      port: deviceConfiguration?.port || null,
      directory: deviceConfiguration?.directory || '',
      connectionTimeout: deviceConfiguration?.connectionTimeout,
      operationTimeout: deviceConfiguration?.operationTimeout,
      userName: deviceConfiguration?.userName || '',
      password: deviceConfiguration?.password || '',
      productId: deviceConfiguration?.productId,
      id: deviceConfiguration?.id,
    },
  })

  useEffect(() => {
    if (!deviceConfiguration) return

    setValue('protocol', deviceConfiguration.protocol)
    setValue('productId', deviceConfiguration.productId)

    Object.entries(deviceConfiguration).forEach(([key, value]) => {
      if (key !== 'protocol' && key !== 'productId') {
        setValue(key as keyof DeviceConfigFormData, value)
      }
    })
  }, [deviceConfiguration, setValue])

  const onSubmit = async (data: DeviceConfigFormData) => {
    try {
      const selectedProduct = productData?.value.find(
        (product) => product.id === data.productId
      )

      const sanitizedDevice: Partial<DeviceConfiguration> = {
        ...data,
        productName: selectedProduct?.model ? selectedProduct?.model : '',
      }

      onSave(sanitizedDevice as DeviceConfiguration)
      onClose()
    } catch (error) {
      console.error('Error occurred while editing/creating device:', error)
    }
  }

  return (
    <Dialog open={isOpen} onClose={onClose}>
      <DialogTitle sx={{ fontSize: '1.3rem' }} id="role-permissions-label">
        Device Configuration Details
      </DialogTitle>

      <DialogContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            {...register('firmware')}
            autoFocus
            margin="dense"
            id="firmware"
            label="Firmware"
            type="text"
            fullWidth
            error={!!errors.firmware}
            helperText={errors.firmware ? errors.firmware.message : ''}
          />
          <TextField
            {...register('notes')}
            margin="dense"
            id="notes"
            label="Notes"
            type="text"
            fullWidth
          />
          <FormControl fullWidth margin="dense">
            <InputLabel id="protocol-label">Protocol</InputLabel>
            <Select
              labelId="protocol-label"
              id="protocol-select"
              label="Protocol"
              error={!!errors.protocol}
              value={watch('protocol') || ''}
              onChange={(e) => setValue('protocol', e.target.value)}
            >
              {transportProtocols?.map((protocol) => (
                <MenuItem key={protocol.value} value={protocol.name}>
                  {protocol.name}
                </MenuItem>
              ))}
            </Select>
            {errors.protocol && (
              <p style={{ color: 'red', fontSize: '12px' }}>
                {errors.protocol.message}
              </p>
            )}
          </FormControl>
          <TextField
            {...register('port', { valueAsNumber: true })}
            margin="dense"
            id="port"
            label="Port"
            type="number"
            fullWidth
            error={!!errors.port}
            helperText={errors.port ? errors.port.message : ''}
          />
          <TextField
            {...register('directory')}
            margin="dense"
            id="directory"
            label="Directory"
            type="text"
            fullWidth
          />
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField
                {...register('connectionTimeout', { valueAsNumber: true })}
                margin="dense"
                id="connectionTimeout"
                label="Connection Timeout"
                type="number"
                fullWidth
                error={!!errors.connectionTimeout}
                helperText={
                  errors.connectionTimeout
                    ? errors.connectionTimeout.message
                    : ''
                }
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                {...register('operationTimeout', { valueAsNumber: true })}
                margin="dense"
                id="operationTimeout"
                label="Operation Timeout"
                type="number"
                fullWidth
                error={!!errors.operationTimeout}
                helperText={
                  errors.operationTimeout ? errors.operationTimeout.message : ''
                }
              />
            </Grid>
          </Grid>
          <TextField
            {...register('userName')}
            margin="dense"
            id="userName"
            label="Username"
            type="text"
            fullWidth
          />
          <TextField
            {...register('password')}
            margin="dense"
            id="password"
            label="Password"
            type="text"
            fullWidth
          />
          <FormControl fullWidth margin="dense">
            <InputLabel id="product-label">Product</InputLabel>
            <Select
              labelId="product-label"
              id="product-select"
              label="Product"
              error={!!errors.productId}
              value={watch('productId') || ''}
              onChange={(e) =>
                setValue('productId', Number(e.target.value), {
                  shouldValidate: true,
                })
              } 
            >
              {productData?.value.map((product) => (
                <MenuItem key={product.id} value={product.id}>
                  {product.model}
                </MenuItem>
              ))}
            </Select>
            {errors.productId && (
              <p style={{ color: 'red', fontSize: '12px' }}>
                {errors.productId.message}
              </p>
            )}
          </FormControl>
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button variant="contained" type="submit">
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default DeviceConfigModal
