import CheckIcon from '@mui/icons-material/Check'
import { Box, Paper, Stack, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material'

export type FilterMode = 'combine' | 'split'
export interface FilterOption {
  value: string
  label: string
}

interface TurningMovementCountsFiltersProps {
  directionOptions: FilterOption[]
  movementOptions: FilterOption[]
  selectedDirections: string[]
  selectedMovementTypes: string[]
  directionMode: FilterMode
  movementMode: FilterMode
  onToggleDirection: (value: string) => void
  onToggleMovement: (value: string) => void
  onDirectionModeChange: (value: FilterMode) => void
  onMovementModeChange: (value: FilterMode) => void
}

const filterToggleSx = {
  borderRadius: 999,
  border: '1px solid',
  borderColor: 'primary.main',
  bgcolor: 'background.paper',
  px: 1.25,
  py: 0.35,
  textTransform: 'none',
  color: 'text.primary',
  '&:hover': {
    borderColor: 'primary.dark',
    bgcolor: 'primary.50',
  },
  '&.Mui-selected': {
    bgcolor: 'primary.main',
    borderColor: 'primary.main',
    color: 'primary.contrastText',
  },
  '&.Mui-selected:hover': {
    bgcolor: 'primary.dark',
    borderColor: 'primary.dark',
  },
} as const

const filterModeSx = {
  '& .MuiToggleButtonGroup-grouped': {
    textTransform: 'none',
    px: 1,
    py: 0.5,
    minHeight: 24,
    fontSize: '0.72rem',
    fontWeight: 600,
    '&.Mui-selected': {
      bgcolor: 'primary.main',
      borderColor: 'primary.main',
      color: 'primary.contrastText',
    },
    '&.Mui-selected:hover': {
      bgcolor: 'primary.dark',
      borderColor: 'primary.dark',
    },
  },
} as const

const rowLabelSx = {
  minWidth: 86,
  fontWeight: 700,
  letterSpacing: 0.3,
} as const

function renderModeToggle(
  value: FilterMode,
  onChange: (value: FilterMode) => void
) {
  return (
    <ToggleButtonGroup
      exclusive
      value={value}
      onChange={(_, v) => v && onChange(v as FilterMode)}
      sx={filterModeSx}
    >
      <ToggleButton value="combine">Combine</ToggleButton>
      <ToggleButton value="split">Split</ToggleButton>
    </ToggleButtonGroup>
  )
}

export default function TurningMovementCountsFilters({
  directionOptions,
  movementOptions,
  selectedDirections,
  selectedMovementTypes,
  directionMode,
  movementMode,
  onToggleDirection,
  onToggleMovement,
  onDirectionModeChange,
  onMovementModeChange,
}: TurningMovementCountsFiltersProps) {
  return (
    <Stack spacing={1.5} sx={{ mb: 2 }}>
      <Paper
        variant="outlined"
        sx={{
          p: 2,
          borderRadius: 2,
          bgcolor: 'grey.50',
          borderColor: 'grey.200',
        }}
      >
        <Stack spacing={1.5}>
          <Stack
            direction={{ xs: 'column', lg: 'row' }}
            alignItems={{ xs: 'flex-start', lg: 'center' }}
            gap={1}
          >
            <Typography variant="caption" color="text.secondary" sx={rowLabelSx}>
              DIRECTION
            </Typography>
            <Stack direction="row" gap={1} flexWrap="wrap" sx={{ flex: 1 }}>
              {directionOptions.map((dir) => {
                const isSelected = selectedDirections.includes(dir.value)
                return (
                  <ToggleButton
                    key={dir.value}
                    size="small"
                    value={dir.value}
                    selected={isSelected}
                    onChange={() => onToggleDirection(dir.value)}
                    sx={filterToggleSx}
                  >
                    <Box
                      sx={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: 0.75,
                      }}
                    >
                      {isSelected ? <CheckIcon sx={{ fontSize: 16 }} /> : null}
                      <span>{dir.label}</span>
                    </Box>
                  </ToggleButton>
                )
              })}
            </Stack>
            {renderModeToggle(directionMode, onDirectionModeChange)}
          </Stack>

          <Stack
            direction={{ xs: 'column', lg: 'row' }}
            alignItems={{ xs: 'flex-start', lg: 'center' }}
            gap={1}
          >
            <Typography variant="caption" color="text.secondary" sx={rowLabelSx}>
              MOVEMENT
            </Typography>
            <Stack direction="row" gap={1} flexWrap="wrap" sx={{ flex: 1 }}>
              {movementOptions.map((movement) => {
                const isSelected = selectedMovementTypes.includes(movement.value)
                return (
                  <ToggleButton
                    key={movement.value}
                    size="small"
                    value={movement.value}
                    selected={isSelected}
                    onChange={() => onToggleMovement(movement.value)}
                    sx={filterToggleSx}
                  >
                    <Box
                      sx={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: 0.75,
                      }}
                    >
                      {isSelected ? <CheckIcon sx={{ fontSize: 16 }} /> : null}
                      <span>{movement.label}</span>
                    </Box>
                  </ToggleButton>
                )
              })}
            </Stack>
            {renderModeToggle(movementMode, onMovementModeChange)}
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  )
}
