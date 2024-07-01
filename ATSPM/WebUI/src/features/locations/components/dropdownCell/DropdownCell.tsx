import { Box, Menu, MenuItem, TableCell, Tooltip } from '@mui/material'
import { SvgIconProps } from '@mui/material/SvgIcon'
import React, { MouseEvent, useState } from 'react'

interface OptionType {
  id: string
  description: string
  abbreviation?: string
  icon?: React.ReactElement<SvgIconProps>
}

interface SelectCellProps {
  options: OptionType[]
  value: string
  onUpdate: (id: string) => void
}

function DropdownCell({ options, value, onUpdate }: SelectCellProps) {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const isOpen = Boolean(anchorEl)

  const handleClick = (event: MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleSelect = (id: string) => {
    onUpdate(id)
    handleClose()
  }

  const currentOption = options.find((option) => option.id === value)

  return (
    <TableCell
      sx={{
        minWidth: '180px',
        boxShadow: isOpen ? '0 0 0 2px rgba(0, 123, 255, 0.5) inset' : 'none',
        cursor: 'pointer',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center' }} onClick={handleClick}>
        {currentOption?.icon && (
          <Tooltip title={currentOption.description}>
            {currentOption.icon}
          </Tooltip>
        )}
        <Box sx={{ ml: 1 }}>{currentOption?.description}</Box>
      </Box>
      <Menu anchorEl={anchorEl} open={isOpen} onClose={handleClose}>
        {options.map((option) => (
          <MenuItem key={option.id} onClick={() => handleSelect(option.id)}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              {option.icon}
              <Box sx={{ ml: 1 }}>{option.description}</Box>
            </Box>
          </MenuItem>
        ))}
      </Menu>
    </TableCell>
  )
}

export default DropdownCell
