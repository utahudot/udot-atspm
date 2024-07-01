import { DetectionType, Detector } from '@/features/locations/types'
import {
  Avatar,
  Box,
  Checkbox,
  ListItemText,
  Menu,
  MenuItem,
  TableCell,
  Tooltip,
  useTheme,
} from '@mui/material'
import React, { useState } from 'react'

const options: Record<string, DetectionType> = {
  LLC: {
    id: 'LLC',
    description: 'Lane-by-lane Count',
    displayOrder: 4,
    abbreviation: 'LLC',
  },
  LLS: {
    id: 'LLS',
    description: 'Lane-by-lane with Speed Restriction',
    displayOrder: 5,
    abbreviation: 'LLS',
  },
  SBP: {
    id: 'SBP',
    description: 'Stop Bar Presence',
    displayOrder: 6,
    abbreviation: 'SBP',
  },
  AP: {
    id: 'AP',
    description: 'Advanced Presence',
    displayOrder: 7,
    abbreviation: 'AP',
  },
  AC: {
    id: 'AC',
    description: 'Advanced Count',
    displayOrder: 2,
    abbreviation: 'AC',
  },
  AS: {
    id: 'AS',
    description: 'Advanced Speed',
    displayOrder: 3,
    abbreviation: 'AS',
  },
}

interface DetectionTypesProps {
  detector: Detector
  onUpdate?: (newDetectionTypes: DetectionType[]) => void
  readonly?: boolean
}

const DetectionTypesCell = ({
  detector,
  onUpdate,
  readonly,
}: DetectionTypesProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const theme = useTheme()

  const valueAbbreviations = new Set(
    detector.detectionTypes?.map((dt) => dt.abbreviation)
  )

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleSelect = (abbreviation: string) => {
    const selectedOption = Object.values(options).find(
      (option) => option.abbreviation === abbreviation
    )
    if (selectedOption) {
      const isSelected = valueAbbreviations.has(abbreviation)
      const newDetectionTypes = isSelected
        ? detector.detectionTypes.filter(
            (dt) => dt.abbreviation !== abbreviation
          )
        : [...detector.detectionTypes, selectedOption]

      if (onUpdate) onUpdate(newDetectionTypes)
    }
  }

  return (
    <TableCell
      sx={{ p: readonly ? 0 : '', border: readonly ? 'none' : '' }}
      component={readonly ? 'div' : 'td'}
    >
      <Box
        sx={{
          display: 'flex',
          gap: 1,
          alignItems: 'center',
          flexWrap: 'wrap',
          cursor: 'pointer',
        }}
        onClick={handleClick}
      >
        {Object.entries(options).map(([abbreviation, option]) => (
          <Tooltip key={option.id} title={option.description}>
            <Avatar
              sx={{
                bgcolor: valueAbbreviations.has(abbreviation)
                  ? theme.palette.primary.main
                  : theme.palette.grey[400],
                width: 26,
                height: 26,
                fontSize: '11px',
              }}
            >
              {abbreviation}
            </Avatar>
          </Tooltip>
        ))}
      </Box>
      {!readonly && (
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleClose}
        >
          {Object.values(options).map((option) => (
            <MenuItem
              key={option.id}
              onClick={() => handleSelect(option.abbreviation)}
            >
              <Checkbox checked={valueAbbreviations.has(option.abbreviation)} />
              <ListItemText primary={option.description} />
            </MenuItem>
          ))}
        </Menu>
      )}
    </TableCell>
  )
}

export default DetectionTypesCell
