import { MenuItem, Select, SelectChangeEvent } from '@mui/material'
import React, { useEffect, useState } from 'react'

interface DropdownOption {
  id: string | number
  description: string
  abbreviation?: string
  displayOrder?: number
}

interface DropdownCellProps {
  value: string
  options: DropdownOption[]
  icons?: Record<string, React.ReactNode>
  onValueChange: (newValue: string) => void
}

function DropdownCell({
  value,
  options,
  icons,
  onValueChange,
}: DropdownCellProps) {
  const [open, setOpen] = useState(false)

  useEffect(() => {
    setOpen(true)
  }, [])

  const handleChange = (event: SelectChangeEvent) => {
    onValueChange(event.target.value as string)
    setOpen(false)
  }

  const handleClose = () => {
    setOpen(false)
  }

  const sortedOptions = options.sort(
    (a, b) => (a.displayOrder || 0) - (b.displayOrder || 0)
  )

  return (
    <Select
      value={value || ''}
      open={open}
      onClose={handleClose}
      fullWidth
      onChange={handleChange}
    >
      {sortedOptions.map((option) => (
        <MenuItem
          key={option.id}
          value={option.abbreviation || option.description}
        >
          <div style={{ display: 'flex', alignItems: 'center' }}>
            {icons && icons[option.abbreviation || option.description]}
            <span style={{ marginLeft: icons ? 8 : 0 }}>
              {option.description}
            </span>
          </div>
        </MenuItem>
      ))}
    </Select>
  )
}

export default DropdownCell
