import CustomSelect from '@/components/customSelect'
import { useAreas } from '@/features/areas/api'
import { useJurisdictions } from '@/features/jurisdictions/api'
import { useEditLocation } from '@/features/locations/api'
import { useLocationTypes } from '@/features/locations/api/getLocationTypes'
import { LocationExpanded } from '@/features/locations/types'
import { useRegions } from '@/features/regions/api'
import { dateToTimestamp } from '@/utils/dateTime'
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

interface EditGeneralLocationProps {
  location: LocationExpanded
  updateLocation: (location: LocationExpanded) => void
  handleLocationEdit: (name: string, value: string) => void
  // handleChange: (e: React.ChangeEvent<HTMLInputElement>) => void
  // handleSelectChange: (e: SelectChangeEvent<unknown>) => void
  refetchLocation: () => void
}

function EditGeneralLocation({
  location,
  // handleChange,
  // handleSelectChange,
  handleLocationEdit,
  updateLocation,
  refetchLocation,
}: EditGeneralLocationProps) {
  const queryClient = useQueryClient()
  const { data: areasData } = useAreas()
  const { data: regionsData } = useRegions()
  const { data: jurisdictionData } = useJurisdictions()
  const { data: locationTypeData } = useLocationTypes()

  const { mutate: updateGeneralInfo } = useEditLocation()

  const handleAreaDelete = (id: number | string) => {
    updateLocation({
      ...location,
      areas: location.areas.filter((area) => area.id !== id),
    })
  }

  const handleDateChange = (value: Date | null) => {
    if (!value) return

    updateLocation({
      ...location,
      start: dateToTimestamp(value),
    })
  }

  const handleCheckboxChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target
    updateLocation({
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

    updateLocation({
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

    updateGeneralInfo(
      {
        id: location.id,
        data: generalInfo,
      },
      {
        onSuccess: () => {
          queryClient.invalidateQueries()
        },
      }
    )
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
                  label="Enable Charts"
                  checked={location.chartEnabled}
                  sx={{
                    minWidth: '226px',
                  }}
                />
                <FormControlLabel
                  control={<Checkbox onChange={handleCheckboxChange} />}
                  name="pedsAre1to1"
                  label={
                    <Tooltip
                      title={
                        'Pedestrian channels use the same numbers as the vehicle channels'
                      }
                    >
                      <Typography
                        sx={{
                          textDecoration: 'underline',
                          textDecorationStyle: 'dotted',
                          textUnderlineOffset: '5px',
                        }}
                      >
                        Peds are 1 to 1
                      </Typography>
                    </Tooltip>
                  }
                  checked={location.pedsAre1to1}
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
                displayEmpty
                value={location.areas.map((area) => area.id)}
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

export default EditGeneralLocation
