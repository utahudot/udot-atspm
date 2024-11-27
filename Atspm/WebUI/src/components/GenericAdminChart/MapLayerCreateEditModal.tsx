import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  TextField,
  Typography,
} from '@mui/material'
import React from 'react'
// import { useDropzone } from 'react-dropzone'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface MapLayer {
  id: number
  name: string
  mapURL?: string
  showByDefault: boolean
  blob?: Blob
}

interface MapLayerCreateEditModalProps {
  open: boolean
  onClose: () => void
  data: MapLayer | null
}

const mapLayerSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  mapURL: z.string().optional(),
  showByDefault: z.boolean(),
  blob: z.any().optional(), // Can't strictly type Blob with zod
})

type FormData = z.infer<typeof mapLayerSchema>

export const MapLayerCreateEditModal: React.FC<
  MapLayerCreateEditModalProps
> = ({ open, onClose, data }) => {
  const isEditMode = !!data

  const {
    control,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(mapLayerSchema),
    defaultValues: {
      name: data?.name || '',
      mapURL: data?.mapURL || '',
      showByDefault: data?.showByDefault || false,
    },
  })

  const onDrop = React.useCallback(
    (acceptedFiles: File[]) => {
      if (acceptedFiles?.[0]) {
        setValue('blob', acceptedFiles[0])
      }
    },
    [setValue]
  )

//   const { getRootProps, getInputProps, isDragActive } = useDropzone({
//     onDrop,
//     accept: {
//       'image/*': ['.png', '.jpg', '.jpeg', '.gif'],
//     },
//     maxFiles: 1,
//   })

  const onSubmit = async (data: FormData) => {
    // Placeholder for API call
    if (isEditMode) {
      console.log('Updating map layer:', { ...data, id: data.id })
    } else {
      console.log('Creating new map layer:', data)
    }
    onClose()
  }

  const selectedFile = watch('blob')

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <Typography variant="h4" sx={{ml:3,mt:2}} >
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
            name="mapURL"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Map URL (Optional)"
                fullWidth
                margin="normal"
                error={!!errors.mapURL}
                helperText={errors.mapURL?.message}
              />
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
