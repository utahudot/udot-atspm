import {
  MapLayer,
  ServiceType,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Box,
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
import { useEffect, useRef, useState } from 'react'
import { Controller, SubmitHandler, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'

const formatLabel = (value: string) => {
  return value.replace(/([a-z])([A-Z])/g, '$1 $2') // Insert space before uppercase letters
}

const mapLayerSchema = z
  .object({
    id: z.number().optional(),
    name: z.string().min(1, 'Name is required'),
    mapLayerUrl: z.string(),
    showByDefault: z.boolean(),
    refreshIntervalSeconds: z.number().nullable(),
    serviceType: z
      .string()
      .refine(
        (val) => Object.values(ServiceType).includes(val as ServiceType),
        {
          message: 'Invalid service type',
        }
      ),
  })
  .superRefine((data, ctx) => {
    const { serviceType, mapLayerUrl } = data

    if (
      serviceType === ServiceType.FeatureServer &&
      !/\/FeatureServer\/\d+$/.test(mapLayerUrl)
    ) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'FeatureServer URL must end with "/FeatureServer/{layerId}".',
      })
    }

    if (
      serviceType === ServiceType.MapServer &&
      !/\/MapServer$/.test(mapLayerUrl)
    ) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'MapServer URL must end with "/MapServer".',
      })
    }
  })

type FormData = z.infer<typeof mapLayerSchema>

interface MapLayerCreateEditModalProps {
  data?: MapLayer
  isOpen: boolean
  onClose: () => void
  onSave: (mapLayer: MapLayer) => void
}

export const MapLayerCreateEditModal = ({
  isOpen,
  onClose,
  data: mapLayer,
  onSave,
}: MapLayerCreateEditModalProps) => {
  const isEditMode = !!mapLayer?.name
  const previousServiceType = useRef<string>()

  const {
    control,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<FormData>({
    resolver: zodResolver(mapLayerSchema),
    defaultValues: {
      id: mapLayer?.id,
      name: mapLayer?.name || '',
      mapLayerUrl: mapLayer?.mapLayerUrl || '',
      showByDefault: mapLayer?.showByDefault || false,
      refreshIntervalSeconds: mapLayer?.refreshIntervalSeconds ?? '',
      serviceType: mapLayer?.serviceType || ServiceType.FeatureServer,
    },
  })

  const serviceType = useWatch({ control, name: 'serviceType' })

  const [urlPlaceholder, setUrlPlaceholder] = useState('')

  useEffect(() => {
    if (
      previousServiceType.current &&
      previousServiceType.current !== serviceType
    ) {
      setValue('mapLayerUrl', '')
    }

    if (serviceType === 'FeatureServer') {
      setUrlPlaceholder('e.g., https://maps.example.com/.../FeatureServer/2')
    } else {
      setUrlPlaceholder('e.g., https://maps.example.com/.../MapServer')
    }
    previousServiceType.current = serviceType
  }, [serviceType, setValue])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const updateMapLayer = { ...mapLayer, ...data } as MapLayer
    onSave(updateMapLayer)
    onClose()
  }

  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth="sm" fullWidth>
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
                  {Object.values(ServiceType).map((type) => (
                    <FormControlLabel
                      key={type}
                      value={type}
                      control={<Radio />}
                      label={formatLabel(type)}
                    />
                  ))}
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
          <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
            <Controller
              name="refreshIntervalSeconds"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  onChange={(e) => {
                    const value = e.target.value
                    field.onChange(value === '' ? null : Number(value))
                  }}
                  label="Refresh Interval (seconds)"
                  type="number"
                  sx={{ mr: 2 }}
                  InputProps={{
                    inputProps: { min: 0 },
                  }}
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
                />
              )}
            />
          </Box>
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
