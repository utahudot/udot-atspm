import { LocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import { Location, LocationExpanded } from '@/features/locations/types'
import { Box, TextField } from '@mui/material'
import { Control, Controller, FieldErrors } from 'react-hook-form'

interface LocationTemplateInputsProps {
  locationHandler: LocationConfigHandler
  control: Control<LocationExpanded>
  selectedLocation: Location | null
  setSelectedLocation: (location: Location | null) => void
  locations: Location[]
  errors: FieldErrors<LocationExpanded>
}

const LocationTemplateInputs = ({
  locationHandler,
  control,
  selectedLocation,
  setSelectedLocation,
  locations,
  errors,
}: LocationTemplateInputsProps) => {
  const devices = locationHandler?.expandedLocation?.devices || []

  const handleCopyLocationChange = (
    _: React.SyntheticEvent,
    value: Location | null
  ) => {
    setSelectedLocation(value)
  }

  return (
    <Box sx={{ width: '60%', minWidth: '400px' }}>
      <Box sx={{ marginBottom: 1 }}>
        <LocationInput
          location={selectedLocation}
          locations={locations}
          handleChange={handleCopyLocationChange}
        />
      </Box>
      {selectedLocation && locationHandler?.expandedLocation?.devices && (
        <>
          <Controller
            name="primaryName"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                fullWidth
                label="Primary Name"
                sx={{ marginBottom: 1 }}
                error={!!errors.primaryName}
                helperText={errors.primaryName?.message}
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
                error={!!errors.secondaryName}
                helperText={errors.secondaryName?.message}
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
                error={!!errors.latitude}
                helperText={errors.latitude?.message}
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
                error={!!errors.longitude}
                helperText={errors.longitude?.message}
              />
            )}
          />
          {devices.map((device, index) => (
            <Controller
              key={device.id}
              name={`devices.${index}.ipaddress`}
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  fullWidth
                  sx={{ marginBottom: 1 }}
                  error={!!errors.devices?.[index]?.ipaddress}
                  helperText={errors.devices?.[index]?.ipaddress?.message}
                  label={`Device IP Address - ${device.deviceType}`}
                />
              )}
            />
          ))}
        </>
      )}
    </Box>
  )
}

export default LocationTemplateInputs
