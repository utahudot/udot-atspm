import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import React from 'react'
// import { useDropzone } from 'react-dropzone'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface MapLayer {
  id?: number | undefined
  name: string
  mapLayerUrl?: string
  showByDefault: boolean
  serviceType: 'mapserver' | 'featureserver'
  createdOn?: string
  createdBy?: string
  updatedOn?: string
  updatedBy?: string
  deletedOn?: string | null
  deletedBy?: string | null
}


interface MapLayerCreateEditModalProps {
  open: boolean
  onClose: () => void
  data: MapLayer | null
  onCreate: (mapLayer: MapLayer) => void
  onEdit: (mapLayer: MapLayer) => void
  onSave: (mapLayer: MapLayer) => void
}

const mapLayerSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  mapLayerUrl: z.string().optional(),
  showByDefault: z.boolean(),
  serviceType: z.enum(['mapserver', 'featureserver']),
})

type FormData = z.infer<typeof mapLayerSchema>

export const MapLayerCreateEditModal: React.FC<
  MapLayerCreateEditModalProps
> = ({ open, onClose, data, onCreate, onSave, onEdit }) => {
  const isEditMode = data?.id ? true : false
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(mapLayerSchema),
    defaultValues: {
      name: data?.name || '',
      mapLayerUrl: data?.mapLayerUrl || '',
      showByDefault: data?.showByDefault || false,
      serviceType: data?.serviceType,
    },
  })

  const onSubmit = async (formData: FormData) => {
    try {
      if (isEditMode) {
        const currentDateTime = new Date().toISOString()
        const updatedMapLayer = {
          ...formData,
          id: data?.id,
          createdOn: data?.createdOn || currentDateTime,
          createdBy: data?.createdBy || '',
          updatedOn: currentDateTime,
          updatedBy: '',
          deletedOn: null,
          deletedBy: null,
        }
        await onEdit(updatedMapLayer)
      } else {
        await onCreate(formData)
        onSave
      }
      onClose()
    } catch (error) {
      console.error('Api Error on MapLayer:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <Typography variant="h4" sx={{ ml: 3, mt: 2 }}>
        {isEditMode ? 'Edit Map Layer' : 'Create New Map Layer'}
      </Typography>

      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogContent>
          <Controller
            name="name"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Name"
                fullWidth
                margin="normal"
                error={!!errors.name}
                helperText={errors.name?.message}
              />
            )}
          />

          <Controller
            name="mapLayerUrl"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Map URL (Optional)"
                fullWidth
                margin="normal"
                error={!!errors.mapLayerUrl}
                helperText={errors.mapLayerUrl?.message}
              />
            )}
          />
          <Controller
            name="serviceType"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth margin="normal">
                <InputLabel>Service Type</InputLabel>
                <Select
                  {...field}
                  label="Service Type"
                  error={!!errors.serviceType}
                >
                  <MenuItem value="featureserver">Feature Server</MenuItem>
                  <MenuItem value="mapserver">Map Server</MenuItem>
                </Select>
              </FormControl>
            )}
          />

          {/* <Box
            {...getRootProps()}
            sx={{
              mt: 2,
              p: 3,
              border: '2px dashed',
              borderColor: isDragActive ? 'primary.main' : 'grey.300',
              borderRadius: 1,
              cursor: 'pointer',
              '&:hover': {
                borderColor: 'primary.main',
              },
            }}
          >
            <input {...getInputProps()} />
            <Typography align="center" color="textSecondary">
              {isDragActive
                ? 'Drop the file here'
                : selectedFile
                  ? `Selected file: ${(selectedFile as File).name}`
                  : 'Drag and drop a file here, or click to select a file'}
            </Typography>
          </Box> */}

          <Controller
            name="showByDefault"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Checkbox
                    checked={field.value}
                    onChange={(e) => field.onChange(e.target.checked)}
                  />
                }
                label="Show by default"
                sx={{ mt: 2 }}
              />
            )}
          />
        </DialogContent>

        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" color="primary">
            {isEditMode ? 'Save Changes' : 'Create'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default MapLayerCreateEditModal
