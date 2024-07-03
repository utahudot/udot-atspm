import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import LockIcon from '@mui/icons-material/Lock'
import LockOpenIcon from '@mui/icons-material/LockOpen'
import { Box, IconButton, TableCell, TextField, Tooltip } from '@mui/material'
import React, { useState } from 'react'

interface EditableTableCellProps {
  value: string | number | boolean | null
  onUpdate: (value: string | number | boolean | null) => void
  disabled?: boolean
  lockable?: boolean
  isLocked?: boolean
  sx?: object
}

const EditableTableCell = ({
  value,
  onUpdate,
  disabled = false,
  lockable = false,
  isLocked = false,
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
          boxShadow: isEditing
            ? '0 0 0 2px rgba(0, 123, 255, 0.5) inset'
            : 'none',
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
            <TextField
              inputProps={{ 'aria-label': 'editable-cell' }}
              size="small"
              fullWidth
              value={value || ''}
              onChange={handleChange}
              onBlur={handleBlur}
              onKeyDown={handleKeyDown}
              disabled={disabled || isLocked}
              onClick={() => setIsEditing(true)}
              sx={{
                '& .MuiInputBase-input': {
                  padding: '0px',
                },
                '& .MuiOutlinedInput-root': {
                  '& fieldset': {
                    border: 'none',
                  },
                  '&:hover fieldset': {
                    border: 'none',
                  },
                  '&.Mui-focused fieldset': {
                    border: 'none',
                  },
                },
              }}
            />
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
