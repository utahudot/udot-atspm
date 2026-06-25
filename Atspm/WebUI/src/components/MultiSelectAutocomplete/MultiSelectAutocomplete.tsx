import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import {
  Autocomplete,
  Box,
  Button,
  Checkbox,
  Paper,
  TextField,
} from '@mui/material'
import React, { memo, useCallback } from 'react'

interface MultiSelectAutocompleteProps<T> {
  label: string
  options: T[]
  value: T[] | null
  onChange: (event: React.SyntheticEvent | null, newValue: T[] | null) => void
  getOptionLabelProperty?: keyof T
  exclude?: boolean
}

const CustomPaper = memo(function CustomPaper<T>(props: any) {
  const { children, onSelectAll, onDeselectAll, ...other } = props
  const stop = useCallback((e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
  }, [])
  return (
    <Paper {...other}>
      <Box
        display="flex"
        alignItems="center"
        justifyContent="flex-end"
        p={1}
        onMouseDown={stop}
      >
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            size="small"
            variant="contained"
            onMouseDown={stop}
            onClick={(e) => {
              stop(e)
              onSelectAll()
            }}
          >
            Select All
          </Button>
          <Button
            size="small"
            variant="outlined"
            onMouseDown={stop}
            onClick={(e) => {
              stop(e)
              onDeselectAll()
            }}
          >
            Deselect All
          </Button>
        </Box>
      </Box>
      {children}
    </Paper>
  )
})

const MultiSelectAutocomplete = <T,>({
  label,
  options,
  value,
  onChange,
  getOptionLabelProperty,
}: MultiSelectAutocompleteProps<T>) => {
  const getOptionLabel = useCallback(
    (option: T) => {
      if (typeof option === 'string') return option
      if (getOptionLabelProperty)
        return (option[getOptionLabelProperty] as string) || ''
      return ''
    },
    [getOptionLabelProperty]
  )
  const handleChange = useCallback(
    (event: React.SyntheticEvent | null, newValue: T[]) => {
      if (newValue.length === 0) {
        onChange(event, null)
      } else {
        onChange(event, newValue)
      }
    },
    [onChange]
  )
  const renderPaper = useCallback(
    (paperProps) => (
      <CustomPaper
        {...paperProps}
        onSelectAll={() => onChange(null, options)}
        onDeselectAll={() => onChange(null, null)}
      />
    ),
    [value, options, onChange]
  )
  const renderOption = useCallback(
    (props, option, { selected }) => {
      const { key, ...optionProps } = props
      return (
        <li key={key} {...optionProps}>
          <Checkbox
            icon={<CheckBoxOutlineBlankIcon fontSize="small" />}
            checkedIcon={<CheckBoxIcon fontSize="small" />}
            checked={selected}
          />
          {getOptionLabel(option)}
        </li>
      )
    },
    [getOptionLabel]
  )
  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      fullWidth
      value={value || []}
      options={options}
      getOptionLabel={getOptionLabel}
      onChange={(event, newValue) => handleChange(event, newValue)}
      PaperComponent={renderPaper}
      /**
       * Render selected values as plain text:
       * - If the selection is empty or contains all options, display "All".
       * - If more than 5 items are selected, display the first 5 followed by ", ...".
       * - Otherwise, display all selected values separated by commas.
       */
      renderTags={(selected) => {
        let text: string
        if (selected.length === 0 || selected.length === options.length) {
          text = 'All'
        } else if (selected.length > 5) {
          const visible = selected.slice(0, 5).map(getOptionLabel).join(', ')
          text = `${visible}, ...`
        } else {
          text = selected.map(getOptionLabel).join(', ')
        }
        return (
          <Box
            component="span"
            sx={{
              display: 'inline-block',
              maxWidth: '84%',
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            }}
          >
            {text}
          </Box>
        )
      }}
      renderInput={(params) => <TextField {...params} label={label} />}
      renderOption={renderOption}
    />
  )
}

export default memo(MultiSelectAutocomplete)
