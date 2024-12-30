import React from 'react'
import { Box, TextField, FormControl, InputLabel, MenuItem, Select, Table, TableBody, TableCell, TableHead, TableRow, Paper, TableContainer } from '@mui/material'
import { Control, Controller } from 'react-hook-form'
import { MenuItems } from '@/features/menuItems/types/linkDto'

interface MenuItemDropdownLinkProps {
  control: Control<any>
  parentItems: MenuItems[]
  selectedParentChildren?: MenuItems[]
}

const MenuItemDropdownLink: React.FC<MenuItemDropdownLinkProps> = ({
  control,
  parentItems,
  selectedParentChildren = []
}) => {
  return (
    <Box>
      <Controller
        name="parentId"
        control={control}
        render={({ field, fieldState }) => (
          <FormControl fullWidth margin="dense">
            <InputLabel id="parent-label">Parent Dropdown Tab</InputLabel>
            <Select
              {...field}
              labelId="parent-label"
              label="Parent Dropdown Tab"
              error={!!fieldState.error}
            >
              {parentItems.map((item) => (
                <MenuItem key={item.id} value={item.id}>
                  {item.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
      />
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

      {/* Children Table */}
      <TableContainer component={Paper} sx={{ marginTop: 2 }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Dropdown Name</TableCell>
              <TableCell>Child Name</TableCell>
              <TableCell>Display Order</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {selectedParentChildren.map((child) => (
              <TableRow key={child.id}>
                <TableCell>{parentItems.find(p => p.id === child.parentId)?.name}</TableCell>
                <TableCell>{child.name}</TableCell>
                <TableCell>{child.displayOrder}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  )
}

export default MenuItemDropdownLink