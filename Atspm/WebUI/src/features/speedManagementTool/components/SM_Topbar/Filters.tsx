import { useGetAccessCategories } from '@/features/speedManagementTool/api/getAccessCategory'
import { useGetCities } from '@/features/speedManagementTool/api/getCity'
import { useGetCounties } from '@/features/speedManagementTool/api/getCounty'
import { useGetFunctionalTypes } from '@/features/speedManagementTool/api/getFunctionalType'
import { useGetRegions } from '@/features/speedManagementTool/api/getRegion'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import TuneOutlinedIcon from '@mui/icons-material/TuneOutlined'
import {
  Autocomplete,
  Badge,
  Box,
  Button,
  Popover,
  TextField,
} from '@mui/material'
import match from 'autosuggest-highlight/match'
import parse from 'autosuggest-highlight/parse'
import React, { useEffect, useState } from 'react'

const formatLabel = (option: any) => option.name

const customSort = (options: any[], value: string) => {
  return options.sort((a, b) => a.name.localeCompare(b.name))
}

export default function FiltersButton() {
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

  const optionalFilters = [
    'county',
    'city',
    'accessCategory',
    'functionalType',
    'region',
  ]

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

  const handleAutocompleteChange = (key: string, value: any) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      [key]: value ? value.name : null,
    })
  }

  const handleSubmit = () => {
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    setAnchorEl(null) // Close the popover after submitting filters
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
          <Autocomplete
            value={
              counties.find(
                (option) => option.name === routeSpeedRequest.county
              ) || null
            }
            options={customSort(counties, routeSpeedRequest.county || '')}
            getOptionLabel={(option) => formatLabel(option)}
            onChange={(event, newValue) =>
              handleAutocompleteChange('county', newValue)
            }
            renderInput={(params) => <TextField {...params} label="County" />}
            renderOption={(props, option, { inputValue }) => {
              const matches = match(option.name, inputValue)
              const parts = parse(option.name, matches)
              return (
                <li {...props} key={option.id}>
                  {parts.map((part, index) => (
                    <span
                      key={index}
                      style={{ fontWeight: part.highlight ? 700 : 400 }}
                    >
                      {part.text}
                    </span>
                  ))}
                </li>
              )
            }}
          />

          {/* Access Category Autocomplete */}
          <Autocomplete
            value={
              accessCategories.find(
                (option) => option.name === routeSpeedRequest.accessCategory
              ) || null
            }
            options={customSort(
              accessCategories,
              routeSpeedRequest.accessCategory || ''
            )}
            getOptionLabel={(option) => formatLabel(option)}
            onChange={(event, newValue) =>
              handleAutocompleteChange('accessCategory', newValue)
            }
            renderInput={(params) => (
              <TextField {...params} label="Access Category" />
            )}
            renderOption={(props, option, { inputValue }) => {
              const matches = match(option.name, inputValue)
              const parts = parse(option.name, matches)
              return (
                <li {...props} key={option.id}>
                  {parts.map((part, index) => (
                    <span
                      key={index}
                      style={{ fontWeight: part.highlight ? 700 : 400 }}
                    >
                      {part.text}
                    </span>
                  ))}
                </li>
              )
            }}
          />

          {/* City Autocomplete */}
          <Autocomplete
            value={
              cities.find((option) => option.name === routeSpeedRequest.city) ||
              null
            }
            options={customSort(cities, routeSpeedRequest.city || '')}
            getOptionLabel={(option) => formatLabel(option)}
            onChange={(event, newValue) =>
              handleAutocompleteChange('city', newValue)
            }
            renderInput={(params) => <TextField {...params} label="City" />}
            renderOption={(props, option, { inputValue }) => {
              const matches = match(option.name, inputValue)
              const parts = parse(option.name, matches)
              return (
                <li {...props} key={option.id}>
                  {parts.map((part, index) => (
                    <span
                      key={index}
                      style={{ fontWeight: part.highlight ? 700 : 400 }}
                    >
                      {part.text}
                    </span>
                  ))}
                </li>
              )
            }}
          />

          {/* Functional Type Autocomplete */}
          <Autocomplete
            value={
              functionalTypes.find(
                (option) => option.name === routeSpeedRequest.functionalType
              ) || null
            }
            options={customSort(
              functionalTypes,
              routeSpeedRequest.functionalType || ''
            )}
            getOptionLabel={(option) => formatLabel(option)}
            onChange={(event, newValue) =>
              handleAutocompleteChange('functionalType', newValue)
            }
            renderInput={(params) => (
              <TextField {...params} label="Functional Type" />
            )}
            renderOption={(props, option, { inputValue }) => {
              const matches = match(option.name, inputValue)
              const parts = parse(option.name, matches)
              return (
                <li {...props} key={option.id}>
                  {parts.map((part, index) => (
                    <span
                      key={index}
                      style={{ fontWeight: part.highlight ? 700 : 400 }}
                    >
                      {part.text}
                    </span>
                  ))}
                </li>
              )
            }}
          />

          {/* Region Autocomplete */}
          <Autocomplete
            value={
              regions.find(
                (option) => option.name === routeSpeedRequest.region
              ) || null
            }
            options={customSort(regions, routeSpeedRequest.region || '')}
            getOptionLabel={(option) => formatLabel(option)}
            onChange={(event, newValue) =>
              handleAutocompleteChange('region', newValue)
            }
            renderInput={(params) => <TextField {...params} label="Region" />}
            renderOption={(props, option, { inputValue }) => {
              const matches = match(option.name, inputValue)
              const parts = parse(option.name, matches)
              return (
                <li {...props} key={option.id}>
                  {parts.map((part, index) => (
                    <span
                      key={index}
                      style={{ fontWeight: part.highlight ? 700 : 400 }}
                    >
                      {part.text}
                    </span>
                  ))}
                </li>
              )
            }}
          />

          <Button variant="contained" color="primary" onClick={handleSubmit}>
            Apply Filters
          </Button>
        </Box>
      </Popover>
    </Box>
  )
}
