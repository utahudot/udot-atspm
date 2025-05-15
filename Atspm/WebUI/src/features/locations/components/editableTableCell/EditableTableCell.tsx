import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import LockIcon from '@mui/icons-material/Lock'
import LockOpenIcon from '@mui/icons-material/LockOpen'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import {
  Box,
  FormControl,
  IconButton,
  Input,
  InputAdornment,
  TableCell,
  Tooltip,
} from '@mui/material'
import React, { useState } from 'react'

interface EditableTableCellProps {
  value: string | number | boolean | null
  onUpdate: (value: string | number | boolean | null) => void
  disabled?: boolean
  lockable?: boolean
  isLocked?: boolean
  error?: string
  warning?: string
  sx?: object
}

const EditableTableCell = ({
  value,
  onUpdate,
  disabled = false,
  lockable = false,
  isLocked = false,
  error,
  warning,
  sx,
}: EditableTableCellProps) => {
  const [isEditing, setIsEditing] = useState(false)

  const toggleBoolean = () => {
    if (typeof value === 'boolean' && !disabled && !isLocked) {
      onUpdate(!value)
    }
  }

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onUpdate(event.target.value)
  }

  const handleBlur = () => {
    setIsEditing(false)
  }

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      setIsEditing(false)
    }
  }

  // Red if error, yellow if warning, blue if editing, else none
  const actionColor = (gradient = 0.5) => {
    if (error) return `rgba(255, 0, 0, ${gradient})`
    if (warning) return `rgba(255, 255, 0, ${gradient})`
    if (isEditing) return `rgba(0, 123, 255, ${gradient})`
    return 'none'
  }

  return (
    <Tooltip
      title={
        isLocked
          ? 'Currently locked to protected phase. Unselect Peds are 1:1 to change.'
          : ''
      }
    >
      <TableCell
        onClick={typeof value === 'boolean' ? toggleBoolean : undefined}
        sx={{
          ...sx,
          backgroundColor: `${actionColor(0.1)}`,
          position: 'relative',
          boxShadow: `inset 0 0 0 1px ${actionColor()}`,
          paddingY: 1,
        }}
        aria-disabled={disabled || isLocked}
      >
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            width: '100%',
          }}
        >
          {typeof value === 'boolean' ? (
            <Box>{value ? <CheckBoxIcon /> : <CheckBoxOutlineBlankIcon />}</Box>
          ) : (
            <Box sx={{ width: '100%', position: 'relative' }}>
              <FormControl
                variant="outlined"
                fullWidth
                sx={{ m: 0, minWidth: '80px' }}
              >
                <Input
                  inputProps={{
                    'aria-label': 'editable-cell',
                  }}
                  disableUnderline
                  endAdornment={
                    error ? (
                      <InputAdornment position="end">
                        <Tooltip title={error}>
                          <ErrorOutlineIcon color="error" />
                        </Tooltip>
                      </InputAdornment>
                    ) : warning ? (
                      <InputAdornment position="end">
                        <Tooltip title={warning}>
                          <WarningAmberOutlinedIcon color="warning" />
                        </Tooltip>
                      </InputAdornment>
                    ) : (
                      <InputAdornment position="end" sx={{ width: '24px' }} />
                    )
                  }
                  size="small"
                  fullWidth
                  value={value || ''}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  onKeyDown={handleKeyDown}
                  disabled={disabled || isLocked}
                  onClick={() => setIsEditing(true)}
                  error={!!error}
                />
              </FormControl>
            </Box>
          )}
          {lockable && (
            <IconButton size="small" sx={{ ml: 1 }} disabled>
              {isLocked ? <LockIcon /> : <LockOpenIcon />}
            </IconButton>
          )}
        </Box>
      </TableCell>
    </Tooltip>
  )
}

export default EditableTableCell
