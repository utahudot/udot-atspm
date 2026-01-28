import {
  useGetArea,
  useGetJurisdiction,
  useGetLocationType,
  useGetRegion,
  usePutLocationFromKey,
} from '@/api/config'
import CustomSelect from '@/components/customSelect'
import LocationCoordinatePicker from '@/features/locations/components/editLocation/LocationCoordinatesPicker'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useNotificationStore } from '@/stores/notifications'
import { dateToTimestamp } from '@/utils/dateTime'
import { removeAuditFields } from '@/utils/removeAuditFields'
import SaveIcon from '@mui/icons-material/Save'
import {
  Box,
  Button,
  Checkbox,
  Divider,
  FormControlLabel,
  Grid,
  Paper,
  SelectChangeEvent,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { format, parseISO } from 'date-fns'
import { ChangeEvent } from 'react'
import { useQueryClient } from 'react-query'

const LocationGeneralOptionsEditor = () => {
  const { location, handleLocationEdit, setLocation } = useLocationStore()
  const { addNotification } = useNotificationStore()
  const queryClient = useQueryClient()
  const { data: areasData } = useGetArea()
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionData } = useGetJurisdiction()
  const { data: locationTypeData } = useGetLocationType()

  const { mutateAsync: updateGeneralInfo } = usePutLocationFromKey()

  const handleAreaDelete = (id: number | string) => {
    setLocation({
      ...location,
      areas: location?.areas?.filter((area) => area.id !== id),
    })
  }

  const handleDateChange = (value: Date | null) => {
    if (!value) return

    setLocation({
      ...location,
      start: dateToTimestamp(value),
    })
  }

  const handleCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target
    setLocation({
      ...location,
      [name]: checked,
    })
  }

  const handleAreaChange = (e: SelectChangeEvent<unknown>) => {
    const { value } = e.target

    const areas = []
    for (const id of value as number[]) {
      const area = areasData?.value.find((area) => area.id === id)
      if (area) {
        areas.push(area)
      }
    }

    setLocation({
      ...location,
      areas,
    })
  }

  const handleChange = (
    name: string,
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const { value } = e.target
    handleLocationEdit(name, value)
  }

  const handleSelectChange = (name: string, e: SelectChangeEvent<unknown>) => {
    const { value } = e.target
    handleLocationEdit(name, value as string)
  }

  const handleSaveGeneralLocation = () => {
    const { approaches, region, jurisdiction, devices, ...generalInfo } =
      location

    generalInfo.start = format(parseISO(location.start), 'yyyy-MM-dd')

    const generalInfoDto = removeAuditFields(generalInfo)
    generalInfoDto.areas = location?.areas?.map(removeAuditFields)

    try {
      updateGeneralInfo(
        {
          key: location.id,
          data: generalInfoDto,
        },
        {
          onSuccess: () => {
            queryClient.invalidateQueries()
          },
        }
      )
      addNotification({
        type: 'success',
        title: 'Location saved successfully',
      })
    } catch (error) {
      console.error('Error saving general location info:', error)
      addNotification({
        type: 'error',
        title: 'Error saving general location info',
        message: 'An error occurred while saving the general location info.',
      })
    }
  }

  if (!location) return null

  return (
    <>
      <Box display="flex" justifyContent="flex-end" mb={1}>
        <Button
          variant="contained"
          color="success"
          startIcon={<SaveIcon />}
          onClick={handleSaveGeneralLocation}
        >
          Save General Info
        </Button>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Paper
              elevation={0}
              sx={{ p: 2, bgcolor: 'grey.50', borderRadius: 1 }}
            >
              <Typography
                variant="h5"
                fontWeight="bold"
                gutterBottom
                sx={{ mb: 2 }}
              >
                Version Information
              </Typography>
              <Grid container spacing={2} mb={1}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Label"
                    name="note"
                    value={location.note}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      handleChange('note', e)
                    }
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <DatePicker
                    label="Start"
                    name="start"
                    value={location.start ? new Date(location.start) : null}
                    onChange={handleDateChange}
                    sx={{ width: '100%' }}
                  />
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          <Grid item xs={12} md={6}>
            <Paper
              elevation={0}
              sx={{ p: 2, bgcolor: 'grey.50', borderRadius: 1 }}
            >
              <Typography
                variant="h5"
                fontWeight="bold"
                gutterBottom
                sx={{ mb: 2 }}
              >
                Configuration
              </Typography>
              <Grid container spacing={2} mb={1}>
                <Grid item xs={12}>
                  <Box display="flex" flexDirection="row" gap={4}>
                    <CustomSelect
                      label="Location Type"
                      name="locationTypeId"
                      value={location.locationTypeId}
                      data={locationTypeData?.value.map((type) => ({
                        id: type.id,
                        name: type.name,
                      }))}
                      onChange={(e) => handleSelectChange('locationTypeId', e)}
                      displayProperty="name"
                      fullWidth
                    />
                    <FormControlLabel
                      sx={{ minWidth: 150 }}
                      control={
                        <Checkbox
                          onChange={handleCheckboxChange}
                          name="chartEnabled"
                          checked={location.chartEnabled}
                        />
                      }
                      label={
                        <Tooltip
                          title={
                            'Allow charts to be displayed for this location. If disabled, the location will not be visible in the charts section.'
                          }
                        >
                          <Typography
                            sx={{
                              textDecoration: 'underline',
                              textDecorationStyle: 'dotted',
                              textUnderlineOffset: '5px',
                            }}
                          >
                            Enable Charts
                          </Typography>
                        </Tooltip>
                      }
                    />
                  </Box>
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          <Divider />

          <Grid item xs={12} md={6}>
            <Paper
              elevation={0}
              sx={{ p: 2, bgcolor: 'grey.50', borderRadius: 1 }}
            >
              <Typography
                variant="h5"
                fontWeight="bold"
                gutterBottom
                sx={{ mb: 2 }}
              >
                Address &amp; Coordinates
              </Typography>
              <Grid container spacing={2} mb={2}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Primary Name"
                    name="primaryName"
                    value={location.primaryName}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      handleChange('primaryName', e)
                    }
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Secondary Name"
                    name="secondaryName"
                    value={location.secondaryName}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      handleChange('secondaryName', e)
                    }
                  />
                </Grid>
              </Grid>

              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Latitude"
                    name="latitude"
                    value={location.latitude}
                    onChange={(e: ChangeEvent<HTMLInputElement>) =>
                      handleChange('latitude', e)
                    }
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Box display="flex" alignItems="center" gap={2}>
                    <TextField
                      fullWidth
                      label="Longitude"
                      name="longitude"
                      value={location.longitude}
                      onChange={(e: ChangeEvent<HTMLInputElement>) =>
                        handleChange('longitude', e)
                      }
                    />
                    <LocationCoordinatePicker
                      onChange={(lat, lng) => {
                        const latStr = lat.toFixed(6)
                        const lngStr = lng.toFixed(6)
                        handleLocationEdit('latitude', latStr)
                        handleLocationEdit('longitude', lngStr)
                      }}
                    />
                  </Box>
                </Grid>
              </Grid>
            </Paper>
          </Grid>

          <Grid item xs={12} md={6}>
            <Paper
              elevation={0}
              sx={{ p: 2, bgcolor: 'grey.50', borderRadius: 1 }}
            >
              <Typography
                variant="h5"
                fontWeight="bold"
                gutterBottom
                sx={{ mb: 2 }}
              >
                Categories
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <CustomSelect
                    label="Region"
                    name="regionId"
                    value={location.regionId}
                    data={regionsData?.value}
                    onChange={(e) => handleSelectChange('regionId', e)}
                    displayProperty="description"
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <CustomSelect
                    label="Jurisdiction"
                    name="jurisdictionId"
                    value={location.jurisdictionId}
                    data={jurisdictionData?.value}
                    onChange={(e) => handleSelectChange('jurisdictionId', e)}
                    displayProperty="name"
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12}>
                  <CustomSelect
                    label="Areas"
                    name="areas"
                    value={location?.areas?.map((area) => area.id)}
                    data={areasData?.value}
                    onChange={handleAreaChange}
                    onDelete={handleAreaDelete}
                    displayProperty="name"
                    sx={{ height: '56px' }}
                    fullWidth
                    multiple
                  />
                </Grid>
              </Grid>
            </Paper>
          </Grid>
        </Grid>
      </Paper>
    </>
  )
}

export default LocationGeneralOptionsEditor
