import AutocompleteInput from '@/components/AutocompleteInput'
import { useGetAccessCategories } from '@/features/speedManagementTool/api/getAccessCategory'
import { useGetCities } from '@/features/speedManagementTool/api/getCity'
import { useGetCounties } from '@/features/speedManagementTool/api/getCounty'
import { useGetFunctionalTypes } from '@/features/speedManagementTool/api/getFunctionalType'
import { useGetRegions } from '@/features/speedManagementTool/api/getRegion'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import TuneOutlinedIcon from '@mui/icons-material/TuneOutlined'
import { Badge, Box, Button, Popover } from '@mui/material'
import React, { useEffect, useState } from 'react'

const optionalFilters = [
  'county',
  'city',
  'accessCategory',
  'functionalType',
  'region',
]

const Filters = () => {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const { data: countiesData } = useGetCounties()
  const { data: accessCategoriesData } = useGetAccessCategories()
  const { data: citiesData } = useGetCities()
  const { data: functionalTypesData } = useGetFunctionalTypes()
  const { data: regionsData } = useGetRegions()

  const {
    routeSpeedRequest,
    setRouteSpeedRequest,
    submittedRouteSpeedRequest,
    setSubmittedRouteSpeedRequest,
  } = useStore()

  const counties = countiesData || []
  const accessCategories = accessCategoriesData || []
  const cities = citiesData || []
  const functionalTypes = functionalTypesData || []
  const regions = regionsData || []

  const [filterCount, setFilterCount] = useState(0)

  useEffect(() => {
    const activeFilters = optionalFilters.filter(
      (key) =>
        submittedRouteSpeedRequest[
          key as keyof typeof submittedRouteSpeedRequest
        ] !== null
    )
    setFilterCount(activeFilters.length)
  }, [submittedRouteSpeedRequest])

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)

  const handleAutocompleteChange = <T,>(
    key: keyof typeof routeSpeedRequest,
    value: T | null
  ) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      [key]: value,
    })
  }

  const handleSubmit = () => {
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    setAnchorEl(null)
  }

  return (
    <Box display="flex" alignItems="center" gap={2}>
      <Badge badgeContent={filterCount} color="primary">
        <Button
          variant="outlined"
          startIcon={<TuneOutlinedIcon />}
          onClick={handleClick}
          sx={{ textTransform: 'none' }}
        >
          Filters
        </Button>
      </Badge>
      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right',
        }}
      >
        <Box
          sx={{
            p: 2,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
            minWidth: '300px',
          }}
        >
          {/* County Autocomplete */}
          <AutocompleteInput
            label="County"
            options={counties}
            value={routeSpeedRequest.county}
            onChange={(_, newValue) =>
              handleAutocompleteChange('county', newValue)
            }
            getOptionLabelProperty="name"
          />

          {/* Access Category Autocomplete */}
          <AutocompleteInput
            label="Access Category"
            options={accessCategories}
            value={routeSpeedRequest.accessCategory}
            onChange={(event, newValue) =>
              handleAutocompleteChange('accessCategory', newValue)
            }
            getOptionLabelProperty="name"
          />

          {/* City Autocomplete */}
          <AutocompleteInput
            label="City"
            options={cities}
            value={routeSpeedRequest.city}
            onChange={(_, newValue) =>
              handleAutocompleteChange('city', newValue)
            }
            getOptionLabelProperty="name"
          />

          {/* Functional Type Autocomplete */}
          <AutocompleteInput
            label="Functional Type"
            options={functionalTypes}
            value={routeSpeedRequest.functionalType}
            onChange={(_, newValue) =>
              handleAutocompleteChange('functionalType', newValue)
            }
            getOptionLabelProperty="name"
          />

          {/* Region Autocomplete */}
          <AutocompleteInput
            label="Region"
            options={regions}
            value={routeSpeedRequest.region}
            onChange={(_, newValue) =>
              handleAutocompleteChange('region', newValue)
            }
            getOptionLabelProperty="name"
          />

          <Button variant="contained" color="primary" onClick={handleSubmit}>
            Apply Filters
          </Button>
        </Box>
      </Popover>
    </Box>
  )
}

export default Filters
