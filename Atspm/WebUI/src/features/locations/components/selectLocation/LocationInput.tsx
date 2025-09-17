import { SearchLocation as Location } from '@/api/config'
import { Filters } from '@/features/locations/components/selectLocation/SelectLocation'
import { Autocomplete, Badge, TextField, Tooltip } from '@mui/material'
import match from 'autosuggest-highlight/match'
import parse from 'autosuggest-highlight/parse'
import { useState } from 'react'

const isNumericInput = (inputValue: string) => {
  return /^\d+$/.test(inputValue)
}

const formatLocationLabel = (option: Location) =>
  `${option.locationIdentifier} - ${option.primaryName} @ ${option.secondaryName}`

const customSort = (options: Location[], value: string) => {
  if (isNumericInput(value)) {
    return options.sort((a, b) => {
      const aStartsWithInput = a?.locationIdentifier?.startsWith(value)
      const bStartsWithInput = b?.locationIdentifier?.startsWith(value)
      if (aStartsWithInput && !bStartsWithInput) {
        return -1
      }
      if (bStartsWithInput && !aStartsWithInput) {
        return 1
      }
      return 0
    })
  }
  return options
}

interface LocationInputProps {
  location: Location | null
  locations: Location[]
  chartsDisabled?: boolean
  filters: Filters
  handleChange: (_: React.SyntheticEvent, value: Location | null) => void
}

const LocationInput = ({
  location,
  locations,
  filters,
  handleChange,
}: LocationInputProps) => {
  const amountOfFiltersApplied = filters
    ? Object.values(filters).filter((value) => value !== null).length
    : 0

  // sort locations so that templates come first
  const templateLocations = locations.sort((a, b) => {
    if (a.locationIdentifier.includes('template')) {
      return -1
    }
    if (b.locationIdentifier.includes('template')) {
      return 1
    }
    return 0
  })
  const [inputValue, setInputValue] = useState('')
  return (
    <Badge
      color="primary"
      overlap="rectangular"
      invisible={amountOfFiltersApplied === 0}
      badgeContent={
        <Tooltip
          title={`${amountOfFiltersApplied} filter(s) applied`}
          placement="top"
        >
          <span>{amountOfFiltersApplied}</span>
        </Tooltip>
      }
      sx={{ width: '100%' }}
    >
      <Autocomplete
        value={
          templateLocations?.find(
            (l) => l.locationIdentifier === location?.locationIdentifier
          ) || null
        }
        options={customSort(templateLocations, inputValue)}
        inputValue={inputValue}
        onInputChange={(event, newInputValue) => {
          setInputValue(newInputValue)
        }}
        sx={{ width: '100%', marginBottom: 2 }}
        renderInput={(params) => <TextField {...params} label="Location" />}
        autoHighlight={true}
        autoSelect={true}
        onChange={handleChange}
        selectOnFocus={true}
        getOptionLabel={(option) => formatLocationLabel(option)}
        renderOption={(props, option, { inputValue }) => {
          const matches = match(formatLocationLabel(option), inputValue, {
            insideWords: true,
          })
          const parts = parse(formatLocationLabel(option), matches)

          return (
            <li {...props} key={option.id}>
              <div>
                {parts.map((part, index) => (
                  <span
                    key={index}
                    style={{ fontWeight: part.highlight ? 700 : 400 }}
                  >
                    {part.text}
                  </span>
                ))}
              </div>
            </li>
          )
        }}
      />
    </Badge>
  )
}

export default LocationInput
