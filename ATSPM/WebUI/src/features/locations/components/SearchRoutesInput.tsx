import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { Autocomplete, TextField } from '@mui/material'
import match from 'autosuggest-highlight/match'
import parse from 'autosuggest-highlight/parse'
import { useState } from 'react'

interface SearchRoutesInputProps {
  route: SpeedManagementRoute | null
  routes: SpeedManagementRoute[]
  handleChange: (
    _: React.SyntheticEvent,
    value: SpeedManagementRoute | null
  ) => void
}

const SearchRoutesInput = ({
  route,
  routes,
  handleChange,
}: SearchRoutesInputProps) => {
  const [inputValue, setInputValue] = useState('')

  const formatRouteLabel = (option: SpeedManagementRoute) =>
    option.properties.name

  return (
    <Autocomplete
      size="small"
      value={
        routes?.find(
          (r) => r?.properties.route_id === route?.properties.route_id
        ) || null
      }
      options={routes}
      inputValue={inputValue}
      onInputChange={(event, newInputValue) => {
        setInputValue(newInputValue)
      }}
      renderInput={(params) => <TextField {...params} label="Segments" />}
      autoHighlight={true}
      autoSelect={true}
      onChange={handleChange}
      selectOnFocus={true}
      getOptionLabel={(option) => formatRouteLabel(option)}
      renderOption={(props, option, { inputValue }) => {
        const matches = match(formatRouteLabel(option), inputValue, {
          insideWords: true,
        })
        const parts = parse(formatRouteLabel(option), matches)

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
      fullWidth
    />
  )
}

export default SearchRoutesInput
