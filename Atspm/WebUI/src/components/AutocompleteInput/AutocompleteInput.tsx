import { Autocomplete, TextField } from '@mui/material'
import match from 'autosuggest-highlight/match'
import parse from 'autosuggest-highlight/parse'
import React from 'react'

interface AutocompleteInputProps<T> {
  label: string
  options: T[]
  value: string | null
  onChange: (event: React.SyntheticEvent, newValue: string | null) => void
  getOptionLabelProperty: keyof T
}

const AutocompleteInput = <T,>({
  label,
  options,
  value,
  onChange,
  getOptionLabelProperty,
}: AutocompleteInputProps<T>) => {
  const getOptionLabel = (option: T) => option[getOptionLabelProperty] as string

  return (
    <Autocomplete
      value={
        options.find((option) => option[getOptionLabelProperty] === value) ||
        null
      }
      options={options}
      getOptionLabel={getOptionLabel}
      onChange={(event, newValue) =>
        onChange(event, newValue ? getOptionLabel(newValue) : null)
      }
      renderInput={(params) => <TextField {...params} label={label} />}
      renderOption={(props, option, { inputValue }) => {
        const label = getOptionLabel(option)
        const matches = match(label, inputValue)
        const parts = parse(label, matches)

        return (
          <li {...props} key={label}>
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
  )
}

export default AutocompleteInput
