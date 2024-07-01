// DeviceConfigModal.tsx
import { DeviceConfiguration } from '@/features/devices/types/index'
import { useGetProducts } from '@/features/products/api'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'

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
import React, { useEffect, useState } from 'react'

interface ModalProps {
  open: boolean
  onClose: () => void
  data: DeviceConfiguration | null
  onCreate: (device: DeviceConfiguration) => void
  onEdit: (device: DeviceConfiguration) => void
  onSave: (device: DeviceConfiguration) => void
}

const DeviceConfigModal = ({
  open,
  onClose,
  data,
  onCreate,
  onSave,
  onEdit,
}: ModalProps) => {
  const { data: productData } = useGetProducts()

  const transportProtocols = useConfigEnums(ConfigEnum.TransportProtocols)

  const [errors, setErrors] = useState({
    firmware: false,
    productId: false,
  })

  const [device, setDevice] = useState<DeviceConfiguration | null>(null)

  useEffect(() => {
    if (data) {
      setDevice(data)
    } else {
      setDevice({
        id: null,
        firmware: '',
        notes: '',
        protocol: '',
        port: null,
        directory: '',
        connectionTimeout: null,
        operationTimeout: null,
        userName: '',
        password: '',
        productId: null,
      })
    }
  }, [data])

  const handleChange = (
    event: React.ChangeEvent<{ name?: string; value: unknown }>
  ) => {
    const { name, value } = event.target
    setDevice((prevDevice) => {
      if (prevDevice) {
        return {
          ...prevDevice,
          [name as keyof DeviceConfiguration]:
            name === 'productId' ? Number(value) : value,
        }
      }
      return prevDevice
    })

    // Clear the error state when the field value changes
    setErrors((prevErrors) => ({
      ...prevErrors,
      [name as keyof typeof errors]: false,
    }))
  }

  const handleSubmit = async () => {
    if (device) {
      if (!device.firmware || !device.productId) {
        // Set the error state for the missing fields
        setErrors({
          firmware: !device.firmware,
          productId: !device.productId,
        })
        return
      }

      try {
        const selectedProduct = productData?.value.find(
          (product) => product.id === device.productId
        )

        const sanitizedDevice: Partial<DeviceConfiguration> = {
          ...device,
          productId: device.productId,
          productName: selectedProduct ? selectedProduct.model : '',
        }

        if (device.id) {
          await onEdit(sanitizedDevice as DeviceConfiguration)
        } else {
          await onCreate(sanitizedDevice as DeviceConfiguration)
        }
        onSave(sanitizedDevice as DeviceConfiguration)
        onClose()
      } catch (error) {
        console.error('Error occurred while editing/creating device:', error)
        // Handle the error, show an error message, or perform any necessary actions
      }
    }
  }

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle sx={{ fontSize: '1.3rem' }} id="role-permissions-label">
        Device Configuration Details
      </DialogTitle>

      <DialogContent>
        {device ? (
          <>
            <TextField
              autoFocus
              margin="dense"
              name="firmware"
              label="Firmware"
              type="text"
              fullWidth
              value={device.firmware}
              onChange={handleChange}
              error={errors.firmware}
              helperText={errors.firmware ? 'Firmware is required' : ''}
            />
            <TextField
              margin="dense"
              name="notes"
              label="Notes"
              type="text"
              fullWidth
              value={device.notes}
              onChange={handleChange}
            />
            <FormControl fullWidth margin="dense">
              <InputLabel id="protocol-label">Protocol</InputLabel>
              <Select
                labelId="protocol-label"
                id="protocol-select"
                name="protocol"
                value={device.protocol}
                label="Protocol"
                onChange={handleChange}
              >
                {transportProtocols?.data?.members.map((protocol) => (
                  <MenuItem key={protocol.value} value={protocol.name}>
                    {protocol.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <TextField
              margin="dense"
              name="port"
              label="Port"
              type="number"
              fullWidth
              value={device.port}
              onChange={handleChange}
              inputProps={{
                inputMode: 'numeric',
                pattern: '[0-9]*',
              }}
            />
            <TextField
              margin="dense"
              name="directory"
              label="Directory"
              type="text"
              fullWidth
              value={device.directory}
              onChange={handleChange}
            />
            {/* <TextField
              margin="dense"
              name="searchTerms"
              label="Search Terms"
              type="text"
              fullWidth
              value={device.searchTerms}
              onChange={handleChange}
            /> */}
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <TextField
                  margin="dense"
                  name="connectionTimeout"
                  label="Connection Timeout"
                  type="number"
                  fullWidth
                  value={device.connectionTimeout}
                  onChange={handleChange}
                  inputProps={{
                    inputMode: 'numeric',
                    pattern: '[0-9]*',
                  }}
                />
              </Grid>
              <Grid item xs={6}>
                <TextField
                  margin="dense"
                  name="operationTimeout"
                  label="Operation Timeout"
                  type="number"
                  fullWidth
                  value={device.operationTimeout}
                  onChange={handleChange}
                  inputProps={{
                    inputMode: 'numeric',
                    pattern: '[0-9]*',
                  }}
                />
              </Grid>
            </Grid>
            {/* <TextField
              margin="dense"
              name="dataModel"
              label="Data Model"
              type="text"
              fullWidth
              value={device.dataModel}
              onChange={handleChange}
            /> */}
            <TextField
              margin="dense"
              name="userName"
              label="Username"
              type="text"
              fullWidth
              value={device.userName}
              onChange={handleChange}
            />
            <TextField
              margin="dense"
              name="password"
              label="Password"
              type="text"
              fullWidth
              value={device.password}
              onChange={handleChange}
            />
            <FormControl fullWidth margin="dense">
              <InputLabel id="product-label">Product</InputLabel>
              <Select
                labelId="product-label"
                id="product-select"
                name="productId"
                value={device.productId || ''}
                label="Product"
                onChange={handleChange}
                error={errors.productId}
                helperText={errors.productId ? 'Product is required' : ''}
              >
                {productData?.value.map((product) => (
                  <MenuItem key={product.id} value={product.id}>
                    {product.model}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </>
        ) : (
          <div>Loading...</div>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>

        <Button variant="contained" onClick={handleSubmit}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default DeviceConfigModal
