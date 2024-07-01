import { Location } from '@/features/locations/types'
import { Autocomplete, TextField } from '@mui/material'
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
      const aStartsWithInput = a.locationIdentifier.startsWith(value)
      const bStartsWithInput = b.locationIdentifier.startsWith(value)
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
  handleChange: (_: React.SyntheticEvent, value: Location | null) => void
}

const LocationInput = ({
  location,
  locations,
  handleChange,
}: LocationInputProps) => {
  const [inputValue, setInputValue] = useState('')

  return (
    <Autocomplete
      value={
        locations?.find(
          (l) => l.locationIdentifier === location?.locationIdentifier
        ) || null
      }
      options={customSort(locations, inputValue)}
      inputValue={inputValue}
      onInputChange={(event, newInputValue) => {
        setInputValue(newInputValue)
      }}
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
  )
}

export default LocationInput
