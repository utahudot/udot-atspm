import type { Location as ConfigLocation, SearchLocation } from '@/api/config'
import {
  useCreateLocation,
  useLatestVersionOfAllLocations,
  useSaveTemplatedLocation,
} from '@/features/locations/api'
import { useLocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import { removeAuditFields } from '@/utils/removeAuditFields'
import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  InputAdornment,
  TextField,
} from '@mui/material'
import FormControlLabel from '@mui/material/FormControlLabel'
import { useMemo, useState } from 'react'
import { Controller, Resolver, useForm } from 'react-hook-form'
import { z } from 'zod'
import LocationTemplateInputs, {
  type NewLocationFormData,
} from './LocationTemplateInputs'

interface NewLocationModalProps {
  closeModal: () => void
  setLocation: (location: ConfigLocation) => void
  onCreatedFromTemplate: () => void
}

// Schemas
// For no template: Only locationIdentifier is required
const noTemplateSchema = z.object({
  locationIdentifier: z
    .string()
    .min(1, { message: 'Location Identifier is required.' })
    .max(10, {
      message: 'Location Identifier must be 10 characters or fewer.',
    }),
})

// For template: locationIdentifier + latitude + longitude + devices (with ipaddress)
const templateSchema = z.object({
  locationIdentifier: z
    .string()
    .min(1, { message: 'Location Identifier is required.' })
    .max(10, {
      message: 'Location Identifier must be 10 characters or fewer.',
    }),
  primaryName: z.string().optional(),
  secondaryName: z.string().optional(),
  latitude: z
    .union([z.string(), z.number()])
    .transform((val) => (val === '' ? NaN : Number(val)))
    .refine((val) => !isNaN(val), {
      message: 'Latitude is required when copying from template',
    }),
  longitude: z
    .union([z.string(), z.number()])
    .transform((val) => (val === '' ? NaN : Number(val)))
    .refine((val) => !isNaN(val), {
      message: 'Longitude is required when copying from template',
    }),
  devices: z
    .array(
      z.object({
        ipaddress: z.string().min(1, { message: 'IP Address is required.' }),
      })
    )
    .min(1, {
      message: 'At least one device is required when copying from template',
    }),
})

const NewLocationModal = ({
  closeModal,
  setLocation,
  onCreatedFromTemplate,
}: NewLocationModalProps) => {
  const [selectedLocation, setSelectedLocation] =
    useState<SearchLocation | null>(null)
  const [copyLocationFromTemplate, setCopyLocationFromTemplate] =
    useState<boolean>(false)

  const locationHandler = useLocationConfigHandler({
    location: selectedLocation as unknown as ConfigLocation,
  })

  const { mutateAsync: createFromTemplate } = useSaveTemplatedLocation(
    selectedLocation?.id
  )
  const { mutate: createLocation } = useCreateLocation()
  const { data: allLocationsData } = useLatestVersionOfAllLocations()
  const locationsResponse = allLocationsData as unknown as
    | SearchLocation[]
    | { value?: SearchLocation[] }
    | undefined
  const allLocations = Array.isArray(locationsResponse)
    ? locationsResponse
    : locationsResponse?.value || []

  const chosenSchema = useMemo(() => {
    return copyLocationFromTemplate ? templateSchema : noTemplateSchema
  }, [copyLocationFromTemplate])

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
    watch,
  } = useForm<NewLocationFormData>({
    resolver: zodResolver(chosenSchema) as Resolver<NewLocationFormData>,
    defaultValues: {
      locationIdentifier: '',
      primaryName: '',
      secondaryName: '',
      latitude: undefined,
      longitude: undefined,
      devices: [],
    },
  })

  const locationIdentifier = watch('locationIdentifier')

  const locationIsUnique = !allLocations.find(
    (loc) => loc.locationIdentifier === locationIdentifier
  )
  const locationIsLessThan10Characters = (locationIdentifier || '').length <= 10

  const onSubmit = async (data: NewLocationFormData) => {
    const devices = locationHandler?.expandedLocation?.devices || []
    const transformedDevices = devices.map((device, index) => {
      const { id, locationId, ...rest } = device
      return {
        ...rest,
        ipaddress: data.devices?.[index]?.ipaddress ?? '',
      }
    })

    const devicesWithoutAuditFields = transformedDevices.map(removeAuditFields)

    if (copyLocationFromTemplate && selectedLocation) {
      const templateData = {
        locationIdentifier: data.locationIdentifier,
        primaryName: data.primaryName || '',
        secondaryName: data.secondaryName || '',
        latitude: data.latitude === undefined ? null : Number(data.latitude),
        longitude: data.longitude === undefined ? null : Number(data.longitude),
        devices: devicesWithoutAuditFields,
      }

      await createFromTemplate(templateData, {
        onSuccess: (createdData) => {
          setLocation(createdData as unknown as ConfigLocation)

          onCreatedFromTemplate()
        },
        onSettled: closeModal,
      })
    } else {
      const defaultValues = {
        locationIdentifier: data.locationIdentifier,
        note: '',
        start: new Date().toISOString(),
        primaryName: '',
        secondaryName: '',
        latitude: 0,
        longitude: 0,
        pedsAre1to1: false,
        locationTypeId: 1,
        chartEnabled: false,
        regionId: 10,
        jurisdictionId: 1,
        versionAction: 'Initial',
      }

      createLocation(defaultValues, {
        onSuccess: (createdData) => {
          setLocation(createdData as unknown as ConfigLocation)
        },
        onSettled: closeModal,
      })
    }
  }

  const errorMessage = () => {
    if (errors.locationIdentifier) {
      return errors.locationIdentifier.message
    }
    if (!locationIsLessThan10Characters) {
      return 'Location Identifier must be 10 characters or fewer.'
    }
    if (!locationIsUnique) {
      return 'Location Identifier already exists.'
    }
    return ''
  }

  const handleCopyLocationCheckBoxChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setCopyLocationFromTemplate(event.target.checked)
    if (!event.target.checked) {
      setSelectedLocation(null)
    }
  }

  return (
    <Dialog
      open={true}
      onClose={closeModal}
      PaperProps={{
        sx: {
          padding: 2,
          minWidth: 400,
          maxWidth: 480,
        },
      }}
    >
      <DialogTitle variant="h4" sx={{ fontWeight: 'bold' }}>
        New Location
      </DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogContent>
          <Box sx={{ width: '60%', minWidth: '400px' }}>
            <Controller
              name="locationIdentifier"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  fullWidth
                  autoComplete="off"
                  error={
                    !!errors.locationIdentifier ||
                    !locationIsUnique ||
                    !locationIsLessThan10Characters
                  }
                  color="success"
                  InputProps={{
                    endAdornment: locationIdentifier ? (
                      <InputAdornment position="end">
                        {locationIsUnique && locationIsLessThan10Characters ? (
                          <CheckCircleOutlineOutlinedIcon color="success" />
                        ) : (
                          <ErrorOutlineIcon color="error" />
                        )}
                      </InputAdornment>
                    ) : null,
                  }}
                  helperText={errorMessage()}
                  label="Location Identifier"
                  sx={{ marginBottom: 1 }}
                />
              )}
            />
          </Box>
          <FormControlLabel
            control={
              <Checkbox
                checked={copyLocationFromTemplate}
                onChange={handleCopyLocationCheckBoxChange}
              />
            }
            label="Copy existing Location"
          />
          {copyLocationFromTemplate && (
            <LocationTemplateInputs
              locationHandler={locationHandler}
              control={control}
              selectedLocation={selectedLocation}
              setSelectedLocation={setSelectedLocation}
              locations={allLocations}
              errors={errors}
            />
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button
            onClick={closeModal}
            variant="outlined"
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <LoadingButton
            variant="contained"
            color="success"
            type="submit"
            loading={isSubmitting}
            disabled={
              !locationIsUnique ||
              !!errors.locationIdentifier ||
              !locationIdentifier ||
              !locationIsLessThan10Characters ||
              (copyLocationFromTemplate && !selectedLocation)
            }
          >
            Create Location
          </LoadingButton>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default NewLocationModal
