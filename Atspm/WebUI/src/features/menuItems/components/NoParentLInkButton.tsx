import React from 'react'
import { Box, TextField, Table, TableBody, TableCell, TableHead, TableRow, Paper, TableContainer } from '@mui/material'
import { Control, Controller } from 'react-hook-form'
import { MenuItems } from '@/features/menuItems/types/linkDto'

interface NoParentLinkButtonProps {
  control: Control<any>
  parentLinks?: MenuItems[]
}

const NoParentLinkButton: React.FC<NoParentLinkButtonProps> = ({
  control,
  parentLinks = []
}) => {
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
        name="link"
        control={control}
        render={({ field, fieldState }) => (
          <TextField
            {...field}
            margin="dense"
            label="Link"
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

      {/* Parent Links Table */}
      <TableContainer component={Paper} sx={{ marginTop: 2 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Parent Name</TableCell>
              <TableCell>Display Order</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {parentLinks.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name}</TableCell>
                <TableCell>{item.displayOrder}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}

export default NoParentLinkButton