import {
  useGetApiV1AccessCategory,
  useGetApiV1City,
  useGetApiV1County,
  useGetApiV1FunctionalType,
  useGetApiV1Region,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import MultiSelectAutocomplete from '@/components/MultiSelectAutocomplete'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import FilterListIcon from '@mui/icons-material/FilterList'
import { Box, Button, Popover } from '@mui/material'
import React, { memo, useCallback, useMemo, useState } from 'react'

const Filters = () => {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const { data: countiesData } = useGetApiV1County()
  const { data: accessCategoriesData } = useGetApiV1AccessCategory()
  const { data: citiesData } = useGetApiV1City()
  const { data: functionalTypesData } = useGetApiV1FunctionalType()
  const { data: regionsData } = useGetApiV1Region()

  const counties = useMemo(
    () => (countiesData ? countiesData.map((item) => item.name) : []),
    [countiesData]
  )
  const accessCategories = useMemo(() => {
    const raw = accessCategoriesData
      ? accessCategoriesData.map((item) => item.name)
      : []
    return [...raw].sort((a, b) => {
      const numA = parseInt(a.match(/\d+/)?.[0] || '-1', 10)
      const numB = parseInt(b.match(/\d+/)?.[0] || '-1', 10)
      if (numA === -1 && numB === -1) return a.localeCompare(b)
      if (numA === -1) return 1
      if (numB === -1) return -1
      return numA - numB
    })
  }, [accessCategoriesData])
  const cities = useMemo(
    () => (citiesData ? citiesData.map((item) => item.name) : []),
    [citiesData]
  )
  const functionalTypes = useMemo(
    () =>
      functionalTypesData ? functionalTypesData.map((item) => item.name) : [],
    [functionalTypesData]
  )
  const regions = useMemo(
    () => (regionsData ? regionsData.map((item) => item.name) : []),
    [regionsData]
  )

  const {
    routeSpeedRequest,
    setRouteSpeedRequest,
    setSubmittedRouteSpeedRequest,
  } = useStore()

  const handleClick = useCallback((event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }, [])
  const handleClose = useCallback(() => {
    setAnchorEl(null)
  }, [])
  const open = useMemo(() => Boolean(anchorEl), [anchorEl])
  const handleAutocompleteChange = useCallback(
    <T,>(key: keyof typeof routeSpeedRequest, value: T | null) => {
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        [key]: value,
      })
    },
    [routeSpeedRequest, setRouteSpeedRequest]
  )
  const handleSubmit = useCallback(() => {
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    setAnchorEl(null)
  }, [routeSpeedRequest, setSubmittedRouteSpeedRequest])

  return (
    <Box display="flex" alignItems="center" gap={2}>
      {/* <Badge badgeContent={filterCount} color="primary"> */}
      <Button
        variant="outlined"
        startIcon={<FilterListIcon />}
        onClick={handleClick}
      >
        Filters
      </Button>
      {/* </Badge> */}
      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <Box
          sx={{
            px: 4,
            py: 2,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
            width: '400px',
          }}
        >
          <MultiSelectAutocomplete
            label="County"
            options={counties}
            value={routeSpeedRequest.county}
            onChange={(_, newValue) =>
              handleAutocompleteChange('county', newValue)
            }
          />
          <MultiSelectAutocomplete
            label="Access Category"
            options={accessCategories}
            value={routeSpeedRequest.accessCategory}
            onChange={(_, newValue) =>
              handleAutocompleteChange('accessCategory', newValue)
            }
          />
          <MultiSelectAutocomplete
            label="City"
            options={cities}
            value={routeSpeedRequest.city}
            onChange={(_, newValue) =>
              handleAutocompleteChange('city', newValue)
            }
          />
          <MultiSelectAutocomplete
            label="Functional Type"
            options={functionalTypes}
            value={routeSpeedRequest.functionalType}
            onChange={(_, newValue) =>
              handleAutocompleteChange('functionalType', newValue)
            }
          />
          <MultiSelectAutocomplete
            label="Region"
            options={regions}
            value={routeSpeedRequest.region}
            onChange={(_, newValue) =>
              handleAutocompleteChange('region', newValue)
            }
          />
          <Button variant="contained" color="primary" onClick={handleSubmit}>
            Apply Filters
          </Button>
        </Box>
      </Popover>
    </Box>
  )
}

export default memo(Filters)
