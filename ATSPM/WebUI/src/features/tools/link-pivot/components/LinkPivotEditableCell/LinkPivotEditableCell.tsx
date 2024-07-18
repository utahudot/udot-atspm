import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import {
  Box,
  FormControl,
  Input,
  InputAdornment,
  TableCell,
  Tooltip,
  useTheme,
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

const LinkPivotEditableCell = ({
  value,
  onUpdate,
  error,
  warning,
  sx,
}: EditableTableCellProps) => {
  const theme = useTheme()
  const [isEditing, setIsEditing] = useState(false)

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
    <TableCell
      sx={{
        ...sx,
        position: 'relative',
        overflow: 'visible',
      }}
    >
      <Box
        sx={{
          backgroundColor: 'background.default',
          borderRadius: '4px',
          outline: isEditing
            ? `2px solid ${theme.palette.primary.main}`
            : '1px solid rgba(0, 0, 0, 0.3)',
          display: 'flex',
          alignItems: 'center',
          width: '100%',
          ':hover': {
            outline: isEditing ? '' : '1px solid black',
          },
        }}
      >
        <Box
          sx={{
            width: '100%',
            position: 'relative',
            p: 1,
          }}
        >
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
              onClick={() => setIsEditing(true)}
              error={!!error}
            />
          </FormControl>
        </Box>
      </Box>
    </TableCell>
  )
}

export default LinkPivotEditableCell
