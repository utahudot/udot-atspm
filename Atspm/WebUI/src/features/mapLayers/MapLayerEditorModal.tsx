import { MapLayer, ServiceType } from '@/features/mapLayers/types'
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

const formatLabel = (value: string) => value.replace(/([a-z])([A-Z])/g, '$1 $2')

const baseSchema = z.object({
  id: z.number().optional(),
  name: z.string().min(1, 'Name is required'),
  mapLayerUrl: z.string().min(1, 'URL is required'),
  showByDefault: z.boolean(),
  refreshIntervalSeconds: z.number().nullable(),
  serviceType: z
    .string()
    .refine((val) => Object.values(ServiceType).includes(val as ServiceType), {
      message: 'Invalid service type',
    }),
  resourceId: z.string().optional(),
  style: z.string().optional(),
})

const mapLayerSchema = baseSchema.superRefine((data, ctx) => {
  const { serviceType, mapLayerUrl, resourceId } = data

  if (serviceType === ServiceType.FeatureServer) {
    if (!/\/FeatureServer\/\d+$/.test(mapLayerUrl)) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'FeatureServer URL must end with "/FeatureServer/{layerId}".',
      })
    }
  }

  if (serviceType === ServiceType.MapServer) {
    if (!/\/MapServer$/.test(mapLayerUrl)) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'MapServer URL must end with "/MapServer".',
      })
    }
  }

  if (serviceType === ServiceType.WMS) {
    if (!/(\/ows$|\/wms$)/i.test(mapLayerUrl)) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'WMS base should end with "/ows" or "/wms".',
      })
    }
    if (!resourceId || resourceId.trim() === '') {
      ctx.addIssue({
        path: ['resourceId'],
        code: z.ZodIssueCode.custom,
        message: 'WMS Layer Name is required.',
      })
    }
  }

  if (serviceType === ServiceType.WFS) {
    if (!/(\/ows$|\/wfs$)/i.test(mapLayerUrl)) {
      ctx.addIssue({
        path: ['mapLayerUrl'],
        code: z.ZodIssueCode.custom,
        message: 'WFS base should end with "/ows" or "/wfs".',
      })
    }
    if (!resourceId || resourceId.trim() === '') {
      ctx.addIssue({
        path: ['resourceId'],
        code: z.ZodIssueCode.custom,
        message: 'WFS Type Name is required.',
      })
    }
  }
})

type FormData = z.infer<typeof mapLayerSchema>

interface MapLayerCreateEditModalProps {
  data?: MapLayer | null
  isOpen?: boolean
  open?: boolean
  onClose?: () => void
  onSave: (mapLayer: MapLayer) => void
}

export const MapLayerCreateEditModal = ({
  isOpen,
  open,
  onClose = () => undefined,
  data: mapLayer,
  onSave,
}: MapLayerCreateEditModalProps) => {
  const modalOpen = open ?? isOpen ?? false
  const isEditMode = typeof mapLayer?.id === 'number'
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
      showByDefault: mapLayer?.showByDefault ?? false,
      refreshIntervalSeconds: mapLayer?.refreshIntervalSeconds ?? null,
      serviceType:
        (mapLayer?.serviceType as ServiceType) || ServiceType.FeatureServer,
      resourceId: mapLayer?.resourceId || '',
      style: mapLayer?.style || '',
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
      setValue('resourceId', '')
      setValue('style', '')
    }

    if (serviceType === ServiceType.FeatureServer) {
      setUrlPlaceholder('e.g., https://host/.../FeatureServer/0')
    } else if (serviceType === ServiceType.MapServer) {
      setUrlPlaceholder('e.g., https://host/.../MapServer')
    } else if (serviceType === ServiceType.WMS) {
      setUrlPlaceholder('e.g., https://host/geoserver/ows or /wms')
    } else {
      setUrlPlaceholder('e.g., https://host/geoserver/ows or /wfs')
    }
    previousServiceType.current = serviceType
  }, [serviceType, setValue])

  const onSubmit: SubmitHandler<FormData> = (data) => {
    const payload: MapLayer = {
      id: data.id,
      name: data.name,
      mapLayerUrl: data.mapLayerUrl,
      showByDefault: data.showByDefault,
      refreshIntervalSeconds: data.refreshIntervalSeconds,
      serviceType: data.serviceType,
    }

    if (
      data.serviceType === ServiceType.WMS ||
      data.serviceType === ServiceType.WFS
    ) {
      payload.resourceId = data.resourceId?.trim()
    }

    if (data.serviceType === ServiceType.WMS && data.style) {
      payload.style = data.style.trim()
    }

    onSave({ ...mapLayer, ...payload })
    onClose()
  }

  const isWms = serviceType === ServiceType.WMS
  const isWfs = serviceType === ServiceType.WFS
  const isArcGisMap = serviceType === ServiceType.MapServer
  const isArcGisFeature = serviceType === ServiceType.FeatureServer

  return (
    <Dialog open={modalOpen} onClose={onClose} maxWidth="sm" fullWidth>
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
                label={
                  isArcGisFeature
                    ? 'FeatureServer URL'
                    : isArcGisMap
                      ? 'MapServer URL'
                      : isWms
                        ? 'WMS Base URL'
                        : 'WFS Base URL'
                }
                placeholder={urlPlaceholder}
                fullWidth
                margin="normal"
                error={!!errors.mapLayerUrl}
                helperText={
                  errors.mapLayerUrl?.message ||
                  (isArcGisFeature
                    ? 'Must end with /FeatureServer/{layerId}'
                    : isArcGisMap
                      ? 'Must end with /MapServer'
                      : isWms
                        ? 'Use /ows or /wms without a query string'
                        : 'Use /ows or /wfs without a query string')
                }
              />
            )}
          />

          {(isWms || isWfs) && (
            <>
              <Controller
                name="resourceId"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={
                      isWms
                        ? 'Layer Name (workspace:layer)'
                        : 'Type Name (workspace:layer)'
                    }
                    placeholder="e.g., workspace:layer"
                    fullWidth
                    margin="normal"
                    error={!!errors.resourceId}
                    helperText={errors.resourceId?.message}
                  />
                )}
              />

              {isWms && (
                <Controller
                  name="style"
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Named Style (optional)"
                      placeholder="e.g., simple_roads"
                      fullWidth
                      margin="normal"
                      error={!!errors.style}
                      helperText={errors.style?.message}
                    />
                  )}
                />
              )}
            </>
          )}

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
                  label="Refresh Interval (sec)"
                  type="number"
                  sx={{ mr: 2 }}
                  InputProps={{ inputProps: { min: 0 } }}
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
