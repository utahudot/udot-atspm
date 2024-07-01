import CustomSelect from '@/components/customSelect'
import { useJurisdictions } from '@/features/jurisdictions/api'
import {
  useLatestVersionOfAllLocations,
  useLocationTypes,
} from '@/features/locations/api'
import {
  useCreateLocation,
  useEditLocation,
} from '@/features/locations/api/location'
import { LocationExpanded } from '@/features/locations/types'
import { useRegions } from '@/features/regions/api'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Divider,
  FormControlLabel,
  TextField,
  Typography,
} from '@mui/material'
import { SelectChangeEvent } from '@mui/material/Select'
import { DatePicker } from '@mui/x-date-pickers'
import {
  ChangeEvent,
  Dispatch,
  SetStateAction,
  useEffect,
  useState,
} from 'react'

interface NewLocationModalProps {
  closeModal: () => void
  setLocation: Dispatch<SetStateAction<Location | null>>
}

const NewLocationModal = ({
  closeModal,
  setLocation,
}: NewLocationModalProps) => {
  const [form, setForm] = useState<LocationExpanded | null>(null)
  const [formErrors, setFormErrors] = useState({})
  const { data: regionsData, error: regionsError } = useRegions()
  const { data: jurisdictionData, error: jurisdictionsError } =
    useJurisdictions()
  const { data: locationTypeData, error: locationTypesError } =
    useLocationTypes()
  const { mutate: createLocation } = useCreateLocation()
  const { mutate: EditLocation } = useEditLocation()
  const { data: allLocationsData, refetch: refetchLocations } =
    useLatestVersionOfAllLocations()

  const allLocations = allLocationsData?.value

  useEffect(() => {
    setForm({
      locationIdentifier: '',
      note: '',
      start: new Date().toISOString(),
      primaryName: '',
      secondaryName: '',
      latitude: '',
      longitude: '',
      pedsAre1to1: false,
      locationTypeId: 0,
      chartEnabled: false,
      regionId: 0,
      jurisdictionId: 0,
    })
  }, [])

  if (!form) return null

  const locationIsUnique = !allLocations?.find(
    (loc) => loc.locationIdentifier === form.locationIdentifier
  )

  if (regionsError || jurisdictionsError || locationTypesError) {
    return (
      <Alert severity="error">
        Failed to load data. Please try again later.
      </Alert>
    )
  }

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setForm({
      ...form,
      [name]: value,
    })
  }

  const handleSelectChange = (e: SelectChangeEvent<unknown>) => {
    const { name, value } = e.target
    setForm({
      ...form,
      [name as string]: value,
    })
  }

  const handleCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target
    setForm({
      ...form,
      [name]: checked,
    })
  }

  const handleDateChange = (value: Date | null) => {
    if (!value) return
    setForm({
      ...form,
      start: dateToTimestamp(value),
    })
  }

  const validateFormData = () => {
    const errors = {}
    if (!form.locationIdentifier.trim())
      errors.locationIdentifier = 'Location Identifier is required.'
    if (
      allLocations?.find(
        (loc) => loc.locationIdentifier === form.locationIdentifier
      )
    )
      errors.locationIdentifier = 'Location Identifier must be unique.'
    if (!form.primaryName.trim())
      errors.primaryName = 'Primary name is required.'
    if (!form.secondaryName.trim())
      errors.secondaryName = 'Secondary name is required.'
    if (!form.latitude || isNaN(Number(form.latitude)))
      errors.latitude = 'Valid latitude is required.'
    if (!form.longitude || isNaN(Number(form.longitude)))
      errors.longitude = 'Valid longitude is required.'
    if (!form.note.trim()) errors.note = 'Label is required.'
    if (!form.locationTypeId)
      errors.locationTypeId = 'Location type is required.'
    if (!form.regionId) errors.regionId = 'Region is required.'
    if (!form.jurisdictionId)
      errors.jurisdictionId = 'Jurisdiction is required.'

    return errors
  }

  const handleSaveGeneralLocation = () => {
    const errors = validateFormData()
    if (Object.keys(errors).length) {
      setFormErrors(errors)
      return
    }

    createLocation(form, {
      onSuccess: (data) => {
        setLocation(data as unknown as Location)
        refetchLocations()
        closeModal()
      },
    })
    closeModal()
  }
  return (
    <>
      <Box marginY={3} display="flex" gap={2}>
        <TextField
          error={!!formErrors.locationIdentifier}
          helperText={formErrors.locationIdentifier}
          label="Location Identifier"
          name="locationIdentifier"
          value={form.locationIdentifier}
          onChange={handleChange}
          sx={{ marginRight: 2, marginBottom: 1 }}
        />
        <Box>
          {form.locationIdentifier === '' ? null : locationIsUnique ? (
            <Alert severity="success">Identifier Not currently in use</Alert>
          ) : (
            <Alert severity="error">Location identifier already in use.</Alert>
          )}
        </Box>
      </Box>
      <Box>
        <Box display="flex" justifyContent="flex-start">
          <Box>
            <Typography variant="h4" fontWeight={'bold'}>
              Version Information
            </Typography>
            <Box marginY={3} display="flex" gap={2}>
              <TextField
                error={!!formErrors.note}
                label="Label"
                name="note"
                value={form.note}
                onChange={handleChange}
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <DatePicker
                label="Start"
                name="start"
                value={
                  form.start ? new Date(form.start.replace('Z', '')) : null
                }
                onChange={handleDateChange}
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  maxWidth: '226px',
                }}
              />
            </Box>
          </Box>
          <Box>
            <Box>
              <Typography variant="h4" fontWeight={'bold'}>
                Configuration
              </Typography>
              <Box marginY={3} display="flex" gap={2}>
                <Box>
                  <CustomSelect
                    error={!!formErrors.locationTypeId}
                    helperText={formErrors.locationTypeId}
                    label="Location Type"
                    name="locationTypeId"
                    value={form.locationTypeId}
                    data={locationTypeData?.value.map((type) => ({
                      id: type.id,
                      name: type.name,
                    }))}
                    onChange={handleSelectChange}
                    displayProperty="name"
                    sx={{
                      marginRight: 2,
                      marginBottom: 1,
                      minWidth: '226px',
                    }}
                  />
                </Box>
                <Box
                  display={'flex'}
                  flexDirection={'column'}
                  alignItems={'flex-start'}
                >
                  <FormControlLabel
                    control={
                      <Checkbox
                        onChange={handleCheckboxChange}
                        value={form.chartEnabled}
                      />
                    }
                    name="chartEnabled"
                    label="Enable Charts"
                    checked={form.chartEnabled}
                    sx={{
                      minWidth: '226px',
                    }}
                  />
                  <FormControlLabel
                    control={
                      <Checkbox
                        onChange={handleCheckboxChange}
                        value={form.pedsAre1to1}
                      />
                    }
                    name="pedsAre1to1"
                    label={'Peds are 1 to 1'}
                    checked={form.pedsAre1to1}
                  />
                </Box>
              </Box>
            </Box>
          </Box>
        </Box>
        <Divider />
        <Box display={'flex'} marginY={3}>
          <Box>
            <Typography variant="h4" fontWeight={'bold'}>
              Address & Coordinates
            </Typography>
            <Box mt={3} mb={1}>
              <TextField
                error={!!formErrors.primaryName}
                helperText={formErrors.primaryName}
                label="Primary Name"
                name="primaryName"
                value={form.primaryName}
                onChange={handleChange}
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <TextField
                error={!!formErrors.secondaryName}
                helperText={formErrors.secondaryName}
                label="Secondary Name"
                name="secondaryName"
                value={form.secondaryName}
                onChange={handleChange}
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
            </Box>
            <Box>
              <TextField
                error={!!formErrors.latitude}
                helperText={formErrors.latitude}
                label="Latitude"
                name="latitude"
                value={form.latitude}
                onChange={handleChange}
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <TextField
                error={!!formErrors.longitude}
                helperText={formErrors.longitude}
                label="Longitude"
                name="longitude"
                value={form.longitude}
                onChange={handleChange}
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
            </Box>
          </Box>
          <Box>
            <Typography variant="h4" fontWeight={'bold'}>
              Categories
            </Typography>
            <Box marginY={3}>
              <CustomSelect
                error={!!formErrors.regionId}
                helperText={formErrors.regionId}
                label="Region"
                name="regionId"
                value={form.regionId}
                data={regionsData?.value}
                onChange={handleSelectChange}
                displayProperty="description"
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  minWidth: '226px',
                }}
              />
              <CustomSelect
                error={!!formErrors.jurisdictionId}
                helperText={formErrors.jurisdictionId}
                label="Jurisdiction"
                name="jurisdictionId"
                value={form.jurisdictionId}
                data={jurisdictionData?.value}
                onChange={handleSelectChange}
                displayProperty="name"
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  minWidth: '226px',
                }}
              />
            </Box>
          </Box>
        </Box>
        <Box display="flex" justifyContent="flex-end" mb={1}>
          <Button onClick={closeModal}>Cancel</Button>
          <Button
            variant="contained"
            color="success"
            onClick={handleSaveGeneralLocation}
            sx={{ marginLeft: 2 }}
            disabled={!locationIsUnique}
          >
            Create Location
          </Button>
        </Box>
      </Box>
    </>
  )
}

export default NewLocationModal
