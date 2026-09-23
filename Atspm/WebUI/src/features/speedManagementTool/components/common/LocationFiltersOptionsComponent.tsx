import MultiSelectAutocomplete from '@/components/MultiSelectAutocomplete'
import { Alert, Box } from '@mui/material'
import { useCallback } from 'react'
import { LocationFiltersHandler } from './handlers/LocationFiltersHandler'

interface Props {
  handler: LocationFiltersHandler
}

type FilterKey =
  | 'county'
  | 'accessCategory'
  | 'city'
  | 'functionalType'
  | 'region'

export const LocationFiltersOptions = (props: Props) => {
  const { handler } = props

  // Map each filter key to its update function.
  // wrap in useCallback to prevent unnecessary re-renders.
  const updateFunctions: Record<FilterKey, (value: string | null) => void> =
    useCallback(
      {
        county: handler.updateCounty,
        accessCategory: handler.updateAccessCategory,
        city: handler.updateCity,
        functionalType: handler.updateFunctionalType,
        region: handler.updateRegion,
      },
      [handler]
    )

  /**
   * Handles changes from the autocomplete components.
   * Extracts the `name` property from the first selected object,
   * then passes that single value (or null) to the appropriate update function.
   */
  const handleAutocompleteChange = useCallback(
    <T extends { name: string }>(key: FilterKey, value: T[] | null) => {
      // Extract the name from the first object if available.
      updateFunctions[key](value)
    },
    [updateFunctions]
  )

  return (
    <Box>
      <Box
        sx={{
          p: 2,
          display: 'flex',
          flexDirection: 'column',
          gap: 3,
          minHeight: '440px',
        }}
      >
        {/* County Filter */}
        <MultiSelectAutocomplete
          label="County"
          options={handler.counties}
          value={handler.county}
          onChange={(_, newValue) =>
            handleAutocompleteChange('county', newValue)
          }
          getOptionLabelProperty="name"
        />

        {/* Access Category Filter */}
        <MultiSelectAutocomplete
          label="Access Category"
          options={handler.accessCategories}
          value={handler.accessCategory}
          onChange={(_, newValue) =>
            handleAutocompleteChange('accessCategory', newValue)
          }
          getOptionLabelProperty="name"
        />

        {/* City Filter */}
        <MultiSelectAutocomplete
          label="City"
          options={handler.cities}
          value={handler.city}
          onChange={(_, newValue) => handleAutocompleteChange('city', newValue)}
          getOptionLabelProperty="name"
        />

        {/* Functional Type Filter */}
        <MultiSelectAutocomplete
          label="Functional Type"
          options={handler.functionalTypes}
          value={handler.functionalType}
          onChange={(_, newValue) =>
            handleAutocompleteChange('functionalType', newValue)
          }
          getOptionLabelProperty="name"
        />

        {/* Region Filter */}
        <MultiSelectAutocomplete
          label="Region"
          options={handler.regions}
          value={handler.region}
          onChange={(_, newValue) =>
            handleAutocompleteChange('region', newValue)
          }
          getOptionLabelProperty="name"
        />

        {handler.showWarning && (
          <Alert severity="warning" sx={{ maxWidth: '300px' }}>
            Location Filters Recommended
          </Alert>
        )}
      </Box>
    </Box>
  )
}
