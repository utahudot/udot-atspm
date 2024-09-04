import { useGetAccessCategories } from '@/features/speedManagementTool/api/getAccessCategory'
import { useGetCities } from '@/features/speedManagementTool/api/getCity'
import { useGetCounties } from '@/features/speedManagementTool/api/getCounty'
import { useGetFunctionalTypes } from '@/features/speedManagementTool/api/getFunctionalType'
import { useGetRegions } from '@/features/speedManagementTool/api/getRegion'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import TuneOutlinedIcon from '@mui/icons-material/TuneOutlined'
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Popover,
  Select,
} from '@mui/material'
import React, { useState } from 'react'

export default function FiltersButton() {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)

  const { data: countiesData } = useGetCounties()
  const { data: accessCategoriesData } = useGetAccessCategories()
  const { data: citiesData } = useGetCities()
  const { data: functionalTypesData } = useGetFunctionalTypes()
  const { data: regionsData } = useGetRegions()

  const counties = countiesData?.sort((a, b) => a.name.localeCompare(b.name))
  const accessCategories = accessCategoriesData?.sort((a, b) => {
    const nameA = a.name.replace('Category', '')
    const nameB = b.name.replace('Category', '')
    return parseInt(nameA) - parseInt(nameB)
  })

  const cities = citiesData?.sort((a, b) => a.name.localeCompare(b.name))
  const functionalTypes = functionalTypesData?.sort((a, b) =>
    a.name.localeCompare(b.name)
  )
  const regions = regionsData?.sort((a, b) => a.name.localeCompare(b.name))

  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const open = Boolean(anchorEl)

  const handleCountyChange = (event: React.ChangeEvent<{ value: unknown }>) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      county:
        event.target.value === 'all' ? null : (event.target.value as string),
    })
  }

  const handleAccessCategoryChange = (
    event: React.ChangeEvent<{ value: unknown }>
  ) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      accessCategory:
        event.target.value === 'all' ? null : (event.target.value as string),
    })
  }

  const handleCityChange = (event: React.ChangeEvent<{ value: unknown }>) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      city:
        event.target.value === 'all' ? null : (event.target.value as string),
    })
  }

  const handleFunctionalTypeChange = (
    event: React.ChangeEvent<{ value: unknown }>
  ) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      functionalType:
        event.target.value === 'all' ? null : (event.target.value as string),
    })
  }

  const handleRegionChange = (event: React.ChangeEvent<{ value: unknown }>) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      region:
        event.target.value === 'all' ? null : (event.target.value as string),
    })
  }

  return (
    <Box>
      <Button
        variant="outlined"
        startIcon={<TuneOutlinedIcon />}
        onClick={handleClick}
        sx={{ textTransform: 'none' }}
      >
        Filters
      </Button>
      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'left',
        }}
      >
        <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* County Filter */}
          <FormControl fullWidth>
            <InputLabel id="county-label">County</InputLabel>
            <Select
              labelId="county-label"
              value={routeSpeedRequest.county || 'all'}
              onChange={handleCountyChange}
              sx={{ minWidth: 200 }}
              label="County"
            >
              <MenuItem value="all">All</MenuItem>
              {counties?.map((county) => (
                <MenuItem key={county.id} value={county.name}>
                  {county.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Access Category Filter */}
          <FormControl fullWidth>
            <InputLabel id="access-category-label">Access Category</InputLabel>
            <Select
              labelId="access-category-label"
              value={routeSpeedRequest.accessCategory || 'all'}
              onChange={handleAccessCategoryChange}
              sx={{ minWidth: 200 }}
              label="Access Category"
            >
              <MenuItem value="all">All</MenuItem>
              {accessCategories?.map((category) => (
                <MenuItem key={category.id} value={category.name}>
                  {category.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* City Filter */}
          <FormControl fullWidth>
            <InputLabel id="city-label">City</InputLabel>
            <Select
              labelId="city-label"
              value={routeSpeedRequest.city || 'all'}
              onChange={handleCityChange}
              sx={{ minWidth: 200 }}
              label="City"
            >
              <MenuItem value="all">All</MenuItem>
              {cities?.map((city) => (
                <MenuItem key={city.id} value={city.name}>
                  {city.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Functional Type Filter */}
          <FormControl fullWidth>
            <InputLabel id="functional-type-label">Functional Type</InputLabel>
            <Select
              labelId="functional-type-label"
              value={routeSpeedRequest.functionalType || 'all'}
              onChange={handleFunctionalTypeChange}
              sx={{ minWidth: 200 }}
              label="Functional Type"
            >
              <MenuItem value="all">All</MenuItem>
              {functionalTypes?.map((type) => (
                <MenuItem key={type.id} value={type.name}>
                  {type.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          {/* Region Filter */}
          <FormControl fullWidth>
            <InputLabel id="region-label">Region</InputLabel>
            <Select
              labelId="region-label"
              value={routeSpeedRequest.region || 'all'}
              onChange={handleRegionChange}
              sx={{ minWidth: 200 }}
              label="Region"
            >
              <MenuItem value="all">All</MenuItem>
              {regions?.map((region) => (
                <MenuItem key={region.id} value={region.name}>
                  {region.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </Popover>
    </Box>
  )
}
