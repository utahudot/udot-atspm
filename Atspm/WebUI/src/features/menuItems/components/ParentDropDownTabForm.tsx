import React from 'react'
import { Box, TextField } from '@mui/material'
import { Control, Controller } from 'react-hook-form'
import { MenuItems } from '@/features/menuItems/types/linkDto'

interface ParentDropdownTabProps {
  control: Control<any>
}

const ParentDropdownTab: React.FC<ParentDropdownTabProps> = ({ control }) => {
  return (
    <Box>
      <Controller
        name="name"
        control={control}
        render={({ field, fieldState }) => (
          <TextField
            {...field}
            margin="dense"
            label="Name"
            type="text"
            fullWidth
            error={!!fieldState.error}
            helperText={fieldState.error?.message}
          />
        )}
      />
      <Controller
        name="displayOrder"
        control={control}
        render={({ field, fieldState }) => (
          <TextField
            {...field}
            margin="dense"
            label="Display Order"
            type="number"
            fullWidth
            inputProps={{ inputMode: 'numeric', pattern: '[0-9]*' }}
            error={!!fieldState.error}
            helperText={fieldState.error?.message}
          />
        )}
      />
    </Box>
  )
}

export default ParentDropdownTab