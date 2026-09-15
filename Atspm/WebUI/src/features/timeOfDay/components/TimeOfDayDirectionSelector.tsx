import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward'
import {
  Box,
  FormControlLabel,
  Switch,
  ToggleButton,
  Typography,
} from '@mui/material'
import type { ChangeEvent } from 'react'
import { useEffect, useState } from 'react'
import type { TimeOfDayFormState } from '../types'

type DirectionField =
  | 'allDayPrimaryDirections'
  | 'amPrimaryDirections'
  | 'pmPrimaryDirections'

type DirectionButtonDefinition = {
  direction: string
  abbreviation: string
  rotation: number
  column: number
  row: number
}

const directionButtonDefinitions: DirectionButtonDefinition[] = [
  {
    direction: 'NorthWest',
    abbreviation: 'NW',
    rotation: -45,
    column: 1,
    row: 1,
  },
  {
    direction: 'Northbound',
    abbreviation: 'N',
    rotation: 0,
    column: 2,
    row: 1,
  },
  {
    direction: 'NorthEast',
    abbreviation: 'NE',
    rotation: 45,
    column: 3,
    row: 1,
  },
  {
    direction: 'Westbound',
    abbreviation: 'W',
    rotation: -90,
    column: 1,
    row: 2,
  },
  {
    direction: 'Eastbound',
    abbreviation: 'E',
    rotation: 90,
    column: 3,
    row: 2,
  },
  {
    direction: 'SouthWest',
    abbreviation: 'SW',
    rotation: -135,
    column: 1,
    row: 3,
  },
  {
    direction: 'Southbound',
    abbreviation: 'S',
    rotation: 180,
    column: 2,
    row: 3,
  },
  {
    direction: 'SouthEast',
    abbreviation: 'SE',
    rotation: 135,
    column: 3,
    row: 3,
  },
]

const areDirectionSetsEqual = (a: string[], b: string[]) =>
  a.length === b.length && a.every((direction) => b.includes(direction))

interface TimeOfDayDirectionSelectorProps {
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
}

export default function TimeOfDayDirectionSelector({
  options,
  onChange,
}: TimeOfDayDirectionSelectorProps) {
  const [usePeriodSpecificDirections, setUsePeriodSpecificDirections] =
    useState(true)

  useEffect(() => {
    const hasPeriodSpecificDirections =
      !areDirectionSetsEqual(
        options.allDayPrimaryDirections,
        options.amPrimaryDirections
      ) ||
      !areDirectionSetsEqual(
        options.allDayPrimaryDirections,
        options.pmPrimaryDirections
      )

    if (hasPeriodSpecificDirections) {
      setUsePeriodSpecificDirections(true)
    }
  }, [
    options.allDayPrimaryDirections,
    options.amPrimaryDirections,
    options.pmPrimaryDirections,
  ])

  const getLaneCountsForDirections = (directions: string[]) => {
    const selectedDirections = new Set(directions)

    return Object.fromEntries(
      Object.entries(options.directionLaneCounts).filter(([direction]) =>
        selectedDirections.has(direction)
      )
    )
  }

  const handleDirectionToggle = (field: DirectionField, direction: string) => {
    const selectedDirections = options[field]
    const nextDirections = selectedDirections.includes(direction)
      ? selectedDirections.filter(
          (selectedDirection) => selectedDirection !== direction
        )
      : [...selectedDirections, direction]

    if (field === 'allDayPrimaryDirections' && !usePeriodSpecificDirections) {
      onChange({
        ...options,
        allDayPrimaryDirections: nextDirections,
        amPrimaryDirections: nextDirections,
        pmPrimaryDirections: nextDirections,
        directionLaneCounts: getLaneCountsForDirections(nextDirections),
      })
      return
    }

    const activeDirections =
      field === 'amPrimaryDirections'
        ? [...nextDirections, ...options.pmPrimaryDirections]
        : [...options.amPrimaryDirections, ...nextDirections]

    onChange({
      ...options,
      [field]: nextDirections,
      directionLaneCounts: getLaneCountsForDirections(activeDirections),
    })
  }

  const handleUsePeriodSpecificDirectionsChange = (
    event: ChangeEvent<HTMLInputElement>
  ) => {
    const checked = event.target.checked
    setUsePeriodSpecificDirections(checked)

    const activeDirections = checked
      ? [...options.amPrimaryDirections, ...options.pmPrimaryDirections]
      : options.allDayPrimaryDirections

    onChange({
      ...options,
      ...(checked
        ? {}
        : {
            amPrimaryDirections: options.allDayPrimaryDirections,
            pmPrimaryDirections: options.allDayPrimaryDirections,
          }),
      directionLaneCounts: getLaneCountsForDirections(activeDirections),
    })
  }

  const renderDirectionGrid = (
    label: string,
    field: DirectionField,
    buttonSize = 36
  ) => {
    const selectedColor = field === 'amPrimaryDirections' ? 'error' : 'primary'

    return (
      <Box sx={{ minWidth: 0 }}>
        {label && (
          <Typography
            id={`${field}-label`}
            variant="subtitle2"
            sx={{
              mb: 0.75,
              minHeight: 20,
              textAlign: 'center',
            }}
          >
            {label}
          </Typography>
        )}
        <Box
          role="group"
          aria-label={label ? undefined : 'Primary directions'}
          aria-labelledby={label ? `${field}-label` : undefined}
          sx={{
            display: 'grid',
            gridTemplateColumns: `repeat(3, ${buttonSize}px)`,
            gridTemplateRows: `repeat(3, ${buttonSize}px)`,
            justifyContent: 'center',
            gap: 0.5,
          }}
        >
          {directionButtonDefinitions.map((definition) => (
            <ToggleButton
              key={definition.direction}
              size="small"
              value={definition.direction}
              selected={options[field].includes(definition.direction)}
              onChange={() =>
                handleDirectionToggle(field, definition.direction)
              }
              aria-label={definition.direction}
              sx={{
                gridColumn: definition.column,
                gridRow: definition.row,
                width: buttonSize,
                height: buttonSize,
                p: 0,
                display: 'flex',
                flexDirection: 'column',
                gap: 0,
                border: 1,
                borderColor: 'divider',
                borderRadius: 1,
                lineHeight: 1,
                '&.Mui-selected': {
                  bgcolor: `${selectedColor}.main`,
                  color: `${selectedColor}.contrastText`,
                  '&:hover': { bgcolor: `${selectedColor}.dark` },
                },
              }}
            >
              <ArrowUpwardIcon
                sx={{
                  fontSize: buttonSize > 36 ? 18 : 15,
                  transform: `rotate(${definition.rotation}deg)`,
                }}
              />
              <Typography
                component="span"
                variant="caption"
                sx={{ fontSize: '0.625rem', lineHeight: 1 }}
              >
                {definition.abbreviation}
              </Typography>
            </ToggleButton>
          ))}
        </Box>
      </Box>
    )
  }

  return (
    <>
      <Box sx={{ minHeight: 167 }}>
        {usePeriodSpecificDirections ? (
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
              gap: 1,
            }}
          >
            {renderDirectionGrid('AM', 'amPrimaryDirections')}
            {renderDirectionGrid('PM', 'pmPrimaryDirections')}
          </Box>
        ) : (
          <Box sx={{ pt: 1 }}>
            {renderDirectionGrid('', 'allDayPrimaryDirections', 44)}
          </Box>
        )}
      </Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-start',
          mt: 1.5,
        }}
      >
        <FormControlLabel
          control={
            <Switch
              size="small"
              checked={usePeriodSpecificDirections}
              onChange={handleUsePeriodSpecificDirectionsChange}
            />
          }
          label="Separate AM/PM directions"
          componentsProps={{
            typography: { variant: 'body2' },
          }}
          sx={{ m: 0 }}
        />
      </Box>
    </>
  )
}
