import { Location } from '@/features/locations/types'
import { Autocomplete, Box, TextField } from '@mui/material'
import match from 'autosuggest-highlight/match'
import parse from 'autosuggest-highlight/parse'
import React from 'react'

interface SelectLocationNoMapProps {
  location: Location | null
  setLocation: (location: Location | null) => void
  locations: Location[]
}

export default function SelectLocationNoMap({
  location,
  setLocation,
  locations,
}: SelectLocationNoMapProps) {
  const handleChange = (
    event: React.SyntheticEvent,
    value: Location | null
  ) => {
    setLocation(value)
  }

  return (
    <Box>
      <Autocomplete
        value={
          locations.find(
            (item) => item.locationIdentifier === location?.locationIdentifier
          ) || null
        }
        options={locations}
        renderInput={(params) => <TextField {...params} label="Location" />}
        autoHighlight={true}
        autoSelect={true}
        onChange={handleChange}
        selectOnFocus={true}
        getOptionLabel={(option) =>
          option.locationIdentifier +
          ' - ' +
          option.primaryName +
          ' @ ' +
          option.secondaryName
        }
        renderOption={(props, option, { inputValue }) => {
          const matches = match(
            option.locationIdentifier +
              ' - ' +
              option.primaryName +
              ' @ ' +
              option.secondaryName,
            inputValue,
            {
              insideWords: true,
            }
          )
          const parts = parse(
            option.locationIdentifier +
              ' - ' +
              option.primaryName +
              ' @ ' +
              option.secondaryName,
            matches
          )

          return (
            <li {...props}>
              <div>
                {parts.map((part, index) => (
                  <span
                    key={index}
                    style={{
                      fontWeight: part.highlight ? 700 : 400,
                    }}
                  >
                    {part.text}
                  </span>
                ))}
              </div>
            </li>
          )
        }}
      />
      <br />
    </Box>
  )
}
