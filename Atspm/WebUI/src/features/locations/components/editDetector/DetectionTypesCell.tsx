import { DetectionType } from '@/features/locations/types'
import {
  Avatar,
  AvatarGroup,
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
  IQ: {
    id: 'IQ',
    description: 'Intermediate Queue',
    displayOrder: 10,
    abbreviation: 'IQ',
  },
  EQ: {
    id: 'EQ',
    description: 'Excessive Queue',
    displayOrder: 11,
    abbreviation: 'EQ',
  },
}

interface DetectionTypesProps {
  detectionTypes: string
  onUpdate?: (newDetectionTypes: string[]) => void
  readonly?: boolean
}

const DetectionTypesCell = ({
  detectionTypes,
  onUpdate,
  readonly,
}: DetectionTypesProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const theme = useTheme()

  const descriptionToAbbreviation = React.useMemo(
    () =>
      Object.values(options).reduce(
        (acc, option) => {
          acc[option.description] = option.abbreviation
          return acc
        },
        {} as Record<string, string>
      ),
    []
  )

  const abbreviationToDescription = React.useMemo(
    () =>
      Object.values(options).reduce(
        (acc, option) => {
          acc[option.abbreviation] = option.description
          return acc
        },
        {} as Record<string, string>
      ),
    []
  )

  const valueAbbreviations = new Set(
    detectionTypes.split(', ').map((desc) => descriptionToAbbreviation[desc])
  )

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    if (!readonly) {
      setAnchorEl(event.currentTarget)
    }
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleSelect = (abbreviation: string) => {
    const description = abbreviationToDescription[abbreviation]
    const isSelected = valueAbbreviations.has(abbreviation)

    const newDetectionTypes = isSelected
      ? detectionTypes.filter((desc) => desc !== description)
      : [...detectionTypes, description]

    if (onUpdate) onUpdate(newDetectionTypes)
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
          cursor: readonly ? 'default' : 'pointer',
        }}
        onClick={handleClick}
      >
        <AvatarGroup max={12}>
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
                  WebkitPrintColorAdjust: 'exact',
                  printColorAdjust: 'exact',
                }}
                slotProps={{
                  width: { style: { width: 26, height: 26, fontSize: '11px' } },
                }}
              >
                {abbreviation}
              </Avatar>
            </Tooltip>
          ))}
        </AvatarGroup>
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
