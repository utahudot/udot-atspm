import { MapLayer } from '@/api/config/aTSPMConfigurationApi.schemas'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  FormControlLabel,
  Radio,
  RadioGroup,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'

// Schema with dynamic URL validation
const mapLayerSchema = z
  .object({
    name: z.string().min(1, 'Name is required'),
    mapLayerUrl: z.string(),
    showByDefault: z.boolean(),
    serviceType: z.enum(['mapserver', 'featureserver']),
  })
  .superRefine((data, ctx) => {
    const { serviceType, mapLayerUrl } = data

    // Validation for Feature Server
    if (
      serviceType === 'featureserver' &&
      !/\/FeatureServer\/\d+$/.test(mapLayerUrl)
    ) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'FeatureServer URL must end with "/FeatureServer/{layerId}".',
      })
    }

    // Validation for Map Server
    if (serviceType === 'mapserver' && !/\/MapServer$/.test(mapLayerUrl)) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'MapServer URL must end with "/MapServer".',
      })
    }
  })

type FormData = z.infer<typeof mapLayerSchema>

interface MapLayerCreateEditModalProps {
  open: boolean
  onClose: () => void
  data: MapLayer | null
  onCreate: (mapLayer: MapLayer) => void
  onEdit: (mapLayer: MapLayer) => void
  onSave: (mapLayer: MapLayer) => void
}

export const MapLayerCreateEditModal = ({
  open,
  onClose,
  data,
  onCreate,
  onEdit,
  onSave,
}: MapLayerCreateEditModalProps) => {
  const isEditMode = !!data
  const {
    control,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<FormData>({
    resolver: zodResolver(mapLayerSchema),
    defaultValues: {
      name: data?.name || '',
      mapLayerUrl: data?.mapLayerUrl || '',
      showByDefault: data?.showByDefault || false,
      serviceType: data?.serviceType || 'featureserver',
    },
  })

  const serviceType = useWatch({ control, name: 'serviceType' })

  const [urlPlaceholder, setUrlPlaceholder] = useState('')

  useEffect(() => {
    if (serviceType === 'featureserver') {
      setUrlPlaceholder('e.g., https://maps.example.com/.../FeatureServer/2')
    } else {
      setUrlPlaceholder('e.g., https://maps.example.com/.../MapServer')
    }
    setValue('mapLayerUrl', '') // Reset URL when service type changes
  }, [serviceType, setValue])

  const onSubmit = (formData: FormData) => {
    if (isEditMode) {
      onEdit(formData)
    } else {
      onCreate(formData)
    }
    onClose()
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <Typography variant="h5" sx={{ ml: 3, mt: 2 }}>
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
            name="serviceType"
            control={control}
            render={({ field }) => (
              <>
                <Typography variant="body1" sx={{ mt: 2 }}>
                  Select Service Type:
                </Typography>
                <RadioGroup
                  {...field}
                  row
                  onChange={(e) => field.onChange(e.target.value)}
                >
                  <FormControlLabel
                    value="featureserver"
                    control={<Radio />}
                    label="Feature Server"
                  />
                  <FormControlLabel
                    value="mapserver"
                    control={<Radio />}
                    label="Map Server"
                  />
                </RadioGroup>
              </>
            )}
          />

          <Controller
            name="mapLayerUrl"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Map Layer URL"
                placeholder={urlPlaceholder}
                fullWidth
                margin="normal"
                error={!!errors.mapLayerUrl}
                helperText={
                  errors.mapLayerUrl?.message ||
                  'URL must match the selected service type.'
                }
              />
            )}
          />

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
