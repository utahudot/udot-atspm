import {
  useCreateLocation,
  useLatestVersionOfAllLocations,
} from '@/features/locations/api'
import { Location, LocationExpanded } from '@/features/locations/types'
import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  InputAdornment,
  TextField,
} from '@mui/material'
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
      primaryName: '',
      secondaryName: '',
      latitude: 0,
      longitude: 0,
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

  return (
    <Dialog
      open={true}
      onClose={closeModal}
      PaperProps={{
        sx: {
          padding: 2,
          minWidth: 400,
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
                sx={{ marginBottom: 1 }}
              />
            )}
          />
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
