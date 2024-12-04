import {
  useCreateLocation,
  useLatestVersionOfAllLocations,
} from '@/features/locations/api'
import { useLocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import { Location, LocationExpanded } from '@/features/locations/types'
import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'

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
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface NewLocationModalProps {
  closeModal: () => void
  setLocation: (location: Location) => void
}

const locationSchema = z.object({
  locationIdentifier: z
    .string()
    .min(1, { message: 'Location Identifier is required.' }),
})

const NewLocationModal = ({
  closeModal,
  setLocation,
}: NewLocationModalProps) => {
  const [selectedLocation, setSelectedLocation] = useState<Location | null>(
    null
  )
  const [checked, setChecked] = useState<boolean>(false)
  const locationHandler = useLocationConfigHandler({
    location: selectedLocation as Location,
  })
  const {
    control,
    handleSubmit,
    setValue,
    formState: { errors },
    watch,
  } = useForm<LocationExpanded>({
    resolver: zodResolver(locationSchema),
    defaultValues: {
      locationIdentifier: '',
    },
  })

  const { mutate: createLocation, isSuccess } = useCreateLocation()
  const { data: allLocationsData } = useLatestVersionOfAllLocations()
  const allLocations = allLocationsData?.value

  const locationIdentifier = watch('locationIdentifier')

  const locationIsUnique = !allLocations?.find(
    (loc: { locationIdentifier: string }) =>
      loc.locationIdentifier === locationIdentifier
  )

  const onSubmit = (data: LocationExpanded) => {
    if (!locationIsUnique) {
      setValue('locationIdentifier', data.locationIdentifier, {
        shouldValidate: true,
        shouldDirty: true,
      })
      return
    }

    const defaultValues = {
      locationIdentifier: data.locationIdentifier,
      note: '',
      start: new Date().toISOString(),
      primaryName: data.primaryName || '',
      secondaryName: data.secondaryName || '',
      latitude: data.latitude || 0,
      longitude: data.longitude || 0,
      pedsAre1to1: false,
      locationTypeId: 1,
      chartEnabled: false,
      regionId: 10,
      jurisdictionId: 1,
    }

    createLocation(defaultValues, {
      onSuccess: (createdData) => {
        setLocation(createdData as unknown as Location)
      },
      onSettled: closeModal,
    })
  }

  const handleCopyLocationCheckBoxChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setChecked(event.target.checked)
    if (!event.target.checked) {
      setSelectedLocation(null)
    }
  }

  const locations = allLocationsData?.value || []

  const handleCopyLocationChange = (
    _: React.SyntheticEvent,
    value: Location | null
  ) => {
    setSelectedLocation(value)
    if (value) {
      // Pre-fill the form with selected location's data
      setValue('primaryName', '')
      setValue('secondaryName', '')
      setValue('latitude', undefined)
      setValue('longitude', undefined)
      setValue('deviceIpAddress', undefined)
    }
  }
  console.log('selectedLocation yo ', locationHandler)
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
      <DialogTitle
        variant="h4"
        sx={{ fontWeight: 'bold' }}
        id="add-location-title"
      >
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
                  error={!!errors.locationIdentifier || !locationIsUnique}
                  color="success"
                  InputProps={{
                    endAdornment: locationIdentifier ? (
                      <InputAdornment position="end">
                        {locationIsUnique ? (
                          <CheckCircleOutlineOutlinedIcon color="success" />
                        ) : (
                          <ErrorOutlineIcon color="error" />
                        )}
                      </InputAdornment>
                    ) : null,
                  }}
                  helperText={
                    errors.locationIdentifier
                      ? errors.locationIdentifier.message
                      : locationIsUnique
                        ? ' '
                        : 'Location Identifier already exists.'
                  }
                  label="Location Identifier"
                />
              )}
            />
          </Box>
          <FormControlLabel
            control={
              <Checkbox
                checked={checked}
                onChange={handleCopyLocationCheckBoxChange}
                inputProps={{ 'aria-label': 'controlled' }}
              />
            }
            label={'Copy existing Location'}
          />
          {checked && (
            <Box sx={{ width: '60%', minWidth: '400px' }}>
              <Box sx={{ marginBottom: 1 }}>
                <LocationInput
                  location={selectedLocation}
                  locations={locations}
                  handleChange={handleCopyLocationChange}
                  sx={{ marginBottom: 1 }}
                />
              </Box>
              <Controller
                name="primaryName"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label="Primary Name"
                    sx={{ marginBottom: 1 }}
                  />
                )}
              />
              <Controller
                name="secondaryName"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label="Secondary Name"
                    sx={{ marginBottom: 1 }}
                  />
                )}
              />
              <Controller
                name="latitude"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label="Latitude"
                    type="number"
                    sx={{ marginBottom: 1 }}
                  />
                )}
              />
              <Controller
                name="longitude"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    fullWidth
                    label="Longitude"
                    type="number"
                    sx={{ marginBottom: 1 }}
                  />
                )}
              />
              {locationHandler?.expandedLocation?.devices &&
                locationHandler.expandedLocation.devices.map((device) => (
                  <Controller
                    name="deviceIpAddress"
                    control={control}
                    render={({ field }) => (
                      <TextField
                        {...field}
                        fullWidth
                        label={`Device IP Address - ${device.deviceType}`}
                        sx={{ marginBottom: 1 }}
                      />
                    )}
                  />
                ))}
            </Box>
          )}
        </DialogContent>

        <DialogActions sx={{ p: 2 }}>
          <Button onClick={closeModal} variant="outlined">
            Cancel
          </Button>
          <Button
            variant="contained"
            color="success"
            type="submit"
            disabled={
              !locationIsUnique ||
              !!errors.locationIdentifier ||
              !locationIdentifier
            }
          >
            Create Location
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default NewLocationModal
