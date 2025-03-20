import { useGetDetectionType } from '@/api/config/aTSPMConfigurationApi'
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

  const { data } = useGetDetectionType()

  const detectionTypes = data?.value?.filter(
    (d) => d.abbreviation !== 'B' && d.abbreviation !== 'NA'
  ) as unknown as DetectionType[]

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
    const selectedOption = Object.values(detectionTypes).find(
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

  if (!detectionTypes) return null

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
                slotProps={{
                  width: { style: { width: 26, height: 26, fontSize: '11px' } },
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
          {Object.values(detectionTypes).map((option) => (
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
