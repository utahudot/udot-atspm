import {
  FormControl,
  InputLabel,
  ListSubheader,
  MenuItem,
  OutlinedInput,
  Select,
  SelectChangeEvent,
} from '@mui/material'
import { useEffect } from 'react';

export interface GroupedListDataItems {
  id: string
  options: { id: number | string; label: string | number }[]
}

interface props {
  label: string
  name: string
  value: number | number[] | string | string[] | undefined
  data: GroupedListDataItems[]
  onChange: (event: SelectChangeEvent<unknown>) => void
  displayProperty: string
}

export const AggregateTypeSelect = ({
  label,
  name,
  value,
  data,
  onChange,
}: props) => {
  useEffect(() => {
    // Auto-set the first option as default if no value is selected
    if (!value && data.length > 0 && data[0].options.length > 0) {
      const defaultValue = `${data[0].id}-${data[0].options[0].id}`
      onChange({ target: { value: defaultValue } } as SelectChangeEvent<unknown>)
    }
  }, [value, data, onChange])
  const renderGroupedOptions = (group: GroupedListDataItems) => {
    return [
      <ListSubheader
        sx={{
          fontSize: '1rem',
          fontWeight: 'bold',
          color: 'black',
        }}
        key={group.id}
      >
        {group.id}
      </ListSubheader>,
      ...group.options.map((option) => (
        <MenuItem key={option.label} value={`${group.id}-${option.id}`}>
          {option.label}
        </MenuItem>
      )),
    ]
  }

  return (
    <FormControl>
      <InputLabel htmlFor={label}>{label}</InputLabel>
      <Select
        inputProps={{id: label}}
        name={name}
        value={value}
        onChange={onChange}
        label={label}
        input={<OutlinedInput id={`select-single-${name}`} label={label} />}
        sx={{ marginRight: 2, marginBottom: 1, minWidth: '226px' }}
      >
        {data.flatMap((item) => renderGroupedOptions(item))}
      </Select>
    </FormControl>
  )
}
