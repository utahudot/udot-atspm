import { DetectionType, Detector } from '@/features/locations/types'
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

interface DetectionTypesProps {
  detector: Detector
  detectionTypes: DetectionType[]
  onUpdate?: (newDetectionTypes: DetectionType[]) => void
  readonly?: boolean
}

const DetectionTypesCell = ({
  detector,
  detectionTypes,
  onUpdate,
  readonly = false,
}: DetectionTypesProps) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const theme = useTheme()

  const valueAbbreviations = new Set(
    detector.detectionTypes?.map((dt) => dt.abbreviation)
  )

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    if (readonly) return
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleSelect = (abbreviation: string) => {
    if (!onUpdate) return
    const selectedOption = detectionTypes.find(
      (option) => option.abbreviation === abbreviation
    )
    if (!selectedOption) return
    const isSelected = valueAbbreviations.has(abbreviation)
    const newDetectionTypes = isSelected
      ? detector.detectionTypes.filter((dt) => dt.abbreviation !== abbreviation)
      : [...detector.detectionTypes, selectedOption]
    onUpdate(newDetectionTypes)
  }

  return (
    <TableCell
      component={readonly ? 'div' : 'td'}
      sx={{
        p: readonly ? 0 : undefined,
        border: readonly ? 'none' : undefined,
        py: 1,
      }}
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
          {detectionTypes.map((option) => (
            <Tooltip key={option.id} title={option.description}>
              <Avatar
                sx={{
                  bgcolor: valueAbbreviations.has(option.abbreviation)
                    ? theme.palette.primary.main
                    : theme.palette.grey[400],
                  width: 26,
                  height: 26,
                  fontSize: '11px',
                  WebkitPrintColorAdjust: 'exact',
                  printColorAdjust: 'exact',
                }}
              >
                {option.abbreviation}
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
          {detectionTypes.map((option) => (
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
