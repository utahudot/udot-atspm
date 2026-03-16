// import { useGetLocationSaveTemplatedLocationFromKey } from '@/api/config'
import {
  useCreateLocation,
  useLatestVersionOfAllLocations,
} from '@/features/locations/api'
import { Location, LocationExpanded } from '@/features/locations/types'
import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  AlertTitle,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  InputAdornment,
  TextField,
} from '@mui/material'
import { isAxiosError } from 'axios'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface NewLocationModalProps {
  closeModal: () => void
  setLocation: (location: Location) => void
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
  const [createError, setCreateError] = useState<string | null>(null)
  // const [selectedLocation, setSelectedLocation] = useState<Location | null>(
  //   null
  // )
  // const [copyLocationFromTemplate, setCopyLocationFromTemplate] =
  //   useState<boolean>(false)

  // const locationHandler = useLocationConfigHandler({
  //   location: selectedLocation as Location,
  // })

  const { mutate: createLocation } = useCreateLocation()
  const { data: allLocationsData } = useLatestVersionOfAllLocations()
  const allLocations = allLocationsData?.value || []

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
    watch,
  } = useForm<LocationExpanded>({
    resolver: zodResolver(noTemplateSchema),
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
    (loc: { locationIdentifier: string }) =>
      loc.locationIdentifier === locationIdentifier
  )
  const locationIsLessThan10Characters = (locationIdentifier || '').length <= 10

  const onSubmit = async (data: LocationExpanded) => {
    setCreateError(null)
    // const devices = locationHandler?.expandedLocation?.devices || []
    // const transformedDevices = devices.map((device, index) => {
    //   const { id, locationId, ...rest } = device
    //   return {
    //     ...rest,
    //     ipaddress: data.devices ? data.devices[index].ipaddress : '',
    //   }
    // })

    // if (copyLocationFromTemplate && selectedLocation) {
    //   const templateData = {
    //     locationIdentifier: data.locationIdentifier,
    //     primaryName: data.primaryName || '',
    //     secondaryName: data.secondaryName || '',
    //     latitude: data.latitude || null,
    //     longitude: data.longitude || null,
    //     devices: transformedDevices,
    //   }

    //   await createFromTemplate(
    //     {
    //       key: parseInt(selectedLocation.id),
    //       data: templateData,
    //     },
    //     {
    //       onSuccess: (createdData) => {
    //         setLocation(createdData as unknown as Location)

    //         onCreatedFromTemplate()
    //       },
    //       onSettled: closeModal,
    //     }
    //   )
    // } else {
    // If not copying template, we just need locationIdentifier.
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
      regionId: 1,
      jurisdictionId: 1,
      versionAction: 'Initial',
    }

    createLocation(defaultValues, {
      onSuccess: (createdData) => {
        setLocation(createdData as unknown as Location)
        closeModal()
      },
      onError: (error) => {
        setCreateError(getCreateLocationErrorMessage(error))
      },
    })
  }
  // }

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

  const getCreateLocationErrorMessage = (error: unknown) => {
    if (!isAxiosError(error)) {
      return 'Location creation failed. Please verify the required setup records exist and try again.'
    }

    const responseData = error.response?.data as
      | {
          error?: string
          title?: string
          errors?: Record<string, string[]>
        }
      | undefined

    const modelErrors = responseData?.errors
      ? Object.values(responseData.errors)
          .flat()
          .filter(Boolean)
      : []

    if (modelErrors.length > 0) {
      return `${modelErrors.join(' ')} Create the missing Region or Jurisdiction first, or rerun DatabaseInstaller after applying the latest Config migration.`
    }

    if (responseData?.error) {
      return responseData.error
    }

    if (responseData?.title) {
      return responseData.title
    }

    return 'Location creation failed because the site is missing required configuration data, usually a valid Region or Jurisdiction.'
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
          {createError && (
            <Alert severity="error" variant="outlined" sx={{ mb: 2 }}>
              <AlertTitle>Location Could Not Be Created</AlertTitle>
              {createError}
            </Alert>
          )}
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
              !locationIsLessThan10Characters
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
