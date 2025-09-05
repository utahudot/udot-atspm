import {
  useGetArea,
  useGetJurisdiction,
  useGetLocationType,
  useGetRegion,
  usePutLocationFromKey,
} from '@/api/config'
import CustomSelect from '@/components/customSelect'
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

    // remove audit fields
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
      <Paper sx={{ padding: 2 }}>
        <Box display="flex" marginTop={2}>
          {/* Left Column */}
          <Box flex={1} marginRight={3}>
            <Typography variant="h4" fontWeight={'bold'} component="p">
              Version Information
            </Typography>
            <Box
              marginBottom={3}
              marginTop={2}
              minHeight={'84px'}
              display={'flex'}
              alignItems={'center'}
            >
              <TextField
                label="Label"
                name="note"
                value={location.note}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  handleChange('note', e)
                }
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <DatePicker
                label="Start"
                name="start"
                value={location.start ? new Date(location.start) : null}
                onChange={handleDateChange}
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  maxWidth: '226px',
                }}
              />
            </Box>
            <Divider />
            <Typography variant="h4" fontWeight={'bold'} marginY={3}>
              Address & Coordinates
            </Typography>
            <Box marginY={3}>
              <TextField
                label="Primary Name"
                name="primaryName"
                value={location.primaryName}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  handleChange('primaryName', e)
                }
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <TextField
                label="Secondary Name"
                name="secondaryName"
                value={location.secondaryName}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  handleChange('secondaryName', e)
                }
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
            </Box>
            <Box marginY={3}>
              <TextField
                label="Latitude"
                name="latitude"
                value={location.latitude}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  handleChange('latitude', e)
                }
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
              <TextField
                label="Longitude"
                name="longitude"
                value={location.longitude}
                onChange={(e: ChangeEvent<HTMLInputElement>) =>
                  handleChange('longitude', e)
                }
                sx={{ marginRight: 2, marginBottom: 1 }}
              />
            </Box>
          </Box>
          {/* Right Column */}
          <Box flex={1}>
            <Typography variant="h4" fontWeight={'bold'}>
              Configuration
            </Typography>
            <Box marginY={3} display={'flex'} alignItems={'center'}>
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
                sx={{
                  marginRight: 2,
                  minWidth: '226px',
                }}
              />
              <Box display={'flex'} flexDirection={'column'}>
                <FormControlLabel
                  control={<Checkbox onChange={handleCheckboxChange} />}
                  name="chartEnabled"
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
                  checked={location.chartEnabled}
                  sx={{
                    minWidth: '226px',
                  }}
                />
              </Box>
            </Box>
            <Divider />
            <Typography variant="h4" fontWeight={'bold'} marginY={3}>
              Categories
            </Typography>
            <Box marginY={3} width={'100%'}>
              <CustomSelect
                label="Region"
                name="regionId"
                value={location.regionId}
                data={regionsData?.value}
                onChange={(e) => handleSelectChange('regionId', e)}
                displayProperty="description"
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  minWidth: '226px',
                }}
              />
              <CustomSelect
                label="Jurisdiction"
                name="jurisdictionId"
                value={location.jurisdictionId}
                data={jurisdictionData?.value}
                onChange={(e) => handleSelectChange('jurisdictionId', e)}
                displayProperty="name"
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  minWidth: '226px',
                }}
              />
            </Box>
            <Box>
              <CustomSelect
                label="Areas"
                name="areas"
                value={location?.areas?.map((area) => area.id)}
                data={areasData?.value}
                onChange={handleAreaChange}
                onDelete={handleAreaDelete}
                displayProperty="name"
                sx={{
                  marginRight: 2,
                  marginBottom: 1,
                  width: 470,
                }}
                multiple
              />
            </Box>
          </Box>
        </Box>
      </Paper>
    </>
  )
}

export default LocationGeneralOptionsEditor
