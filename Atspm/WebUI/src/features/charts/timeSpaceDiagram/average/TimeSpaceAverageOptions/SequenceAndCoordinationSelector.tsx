import { LocationWithCoordPhases, LocationWithSequence } from '@/api/config'
import type { TimeSpaceRouteRow } from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceRouteSelect/TimeSpaceRouteSelect'
import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import {
  Box,
  Chip,
  FormControl,
  MenuItem,
  Select,
  SelectChangeEvent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { alpha } from '@mui/material/styles'
import React, { useState } from 'react'

const CUSTOM_SEQUENCE_VALUE = 'custom'
const CUSTOM_COORDINATED_PHASES_VALUE = 'custom'
const DEFAULT_SEQUENCE: number[][] = [
  [1, 2, 3, 4],
  [5, 6, 7, 8],
]
const DEFAULT_COORDINATED_PHASES = [2, 6]
const TABLE_CELL_PX = 1
const TABLE_COLUMN_WIDTHS = {
  location: 60,
  primaryPhase: 90,
  primarySpeed: 80,
  opposingPhase: 100,
  opposingSpeed: 80,
  distance: 70,
  sequence: 160,
  coordinatedPhases: 120,
} as const
export const TIME_SPACE_AVERAGE_ROUTE_PANEL_WIDTH = 900
const SEQUENCE_MENU_LABEL_WIDTH = 82
const SEQUENCE_MENU_PHASE_SIZE = 20
const SEQUENCE_MENU_ITEM_MY = 0.25

const SEQUENCE_PRESETS = [
  {
    value: '1',
    label: 'Sequence 1',
    sequence: DEFAULT_SEQUENCE,
  },
  {
    value: '2',
    label: 'Sequence 2',
    sequence: [
      [2, 1, 3, 4],
      [5, 6, 7, 8],
    ],
  },
  {
    value: '3',
    label: 'Sequence 3',
    sequence: [
      [1, 2, 4, 3],
      [5, 6, 7, 8],
    ],
  },
  {
    value: '4',
    label: 'Sequence 4',
    sequence: [
      [2, 1, 4, 3],
      [5, 6, 7, 8],
    ],
  },
  {
    value: '5',
    label: 'Sequence 5',
    sequence: [
      [1, 2, 3, 4],
      [6, 5, 7, 8],
    ],
  },
  {
    value: '6',
    label: 'Sequence 6',
    sequence: [
      [2, 1, 3, 4],
      [6, 5, 7, 8],
    ],
  },
  {
    value: '7',
    label: 'Sequence 7',
    sequence: [
      [1, 2, 4, 3],
      [6, 5, 7, 8],
    ],
  },
  {
    value: '8',
    label: 'Sequence 8',
    sequence: [
      [2, 1, 4, 3],
      [6, 5, 7, 8],
    ],
  },
  {
    value: '9',
    label: 'Sequence 9',
    sequence: [
      [1, 2, 3, 4],
      [5, 6, 8, 7],
    ],
  },
  {
    value: '10',
    label: 'Sequence 10',
    sequence: [
      [2, 1, 3, 4],
      [5, 6, 8, 7],
    ],
  },
  {
    value: '11',
    label: 'Sequence 11',
    sequence: [
      [1, 2, 4, 3],
      [5, 6, 8, 7],
    ],
  },
  {
    value: '12',
    label: 'Sequence 12',
    sequence: [
      [2, 1, 4, 3],
      [5, 6, 8, 7],
    ],
  },
  {
    value: '13',
    label: 'Sequence 13',
    sequence: [
      [1, 2, 3, 4],
      [6, 5, 8, 7],
    ],
  },
  {
    value: '14',
    label: 'Sequence 14',
    sequence: [
      [2, 1, 3, 4],
      [6, 5, 8, 7],
    ],
  },
  {
    value: '15',
    label: 'Sequence 15',
    sequence: [
      [1, 2, 4, 3],
      [6, 5, 8, 7],
    ],
  },
  {
    value: '16',
    label: 'Sequence 16',
    sequence: [
      [2, 1, 4, 3],
      [6, 5, 8, 7],
    ],
  },
]

const COORDINATED_PHASE_PRESETS = [
  {
    value: '2,6',
    label: '2,6',
    phases: DEFAULT_COORDINATED_PHASES,
  },
  {
    value: '1,5',
    label: '1,5',
    phases: [1, 5],
  },
  {
    value: '4,8',
    label: '4,8',
    phases: [4, 8],
  },
  {
    value: '3,7',
    label: '3,7',
    phases: [3, 7],
  },
]

const cloneSequence = (sequence: number[][]): number[][] =>
  sequence.map((band) => [...band])

const clonePhases = (phases: number[]): number[] => [...phases]

const sequenceToValues = (sequence?: number[][] | null): string[] =>
  sequence?.map((band) => band.join(',')) ?? []

const sequenceToInputValues = (sequence?: number[][] | null): string[] => {
  const values = sequenceToValues(sequence)

  return [values[0] ?? '', values[1] ?? '']
}

const parseSequenceValue = (value: string): number[] =>
  value
    .split(',')
    .map((phase) => phase.trim())
    .filter(Boolean)
    .map(Number)
    .filter(Number.isFinite)

const parsePhaseList = (value: string): number[] => parseSequenceValue(value)

const sequencesMatch = (left?: number[][] | null, right?: number[][] | null) =>
  Array.isArray(left) &&
  Array.isArray(right) &&
  left.length === right.length &&
  left.every(
    (leftBand, bandIndex) =>
      leftBand.length === right[bandIndex].length &&
      leftBand.every(
        (phase, phaseIndex) => phase === right[bandIndex][phaseIndex]
      )
  )

const getSequencePresetValue = (sequence?: number[][] | null): string =>
  SEQUENCE_PRESETS.find((preset) => sequencesMatch(sequence, preset.sequence))
    ?.value ?? CUSTOM_SEQUENCE_VALUE

const phaseListsMatch = (left?: number[] | null, right?: number[] | null) =>
  Array.isArray(left) &&
  Array.isArray(right) &&
  left.length === right.length &&
  left.every((phase, index) => phase === right[index])

const getCoordinatedPhasePresetValue = (
  phases?: number[] | null
): string =>
  COORDINATED_PHASE_PRESETS.find((preset) =>
    phaseListsMatch(phases, preset.phases)
  )?.value ?? CUSTOM_COORDINATED_PHASES_VALUE

const isPhaseChangedFromDefault = (
  phase: number,
  bandIndex: number,
  phaseIndex: number
) => DEFAULT_SEQUENCE[bandIndex]?.[phaseIndex] !== phase

const ChipCell = ({ value }: { value: number | null }) => (
  <Chip
    label={
      <Typography variant="caption" fontSize="12px">
        {value ?? 'N/A'}
      </Typography>
    }
    size="small"
    icon={
      value !== null ? (
        <CheckIcon color="success" sx={{ fontSize: '12px !important' }} />
      ) : (
        <CloseIcon color="error" sx={{ fontSize: '12px !important' }} />
      )
    }
    color={value !== null ? 'success' : 'error'}
  />
)

interface SequencePresetPreviewProps {
  label: string
  sequence: number[][]
  compact?: boolean
}

function SequencePresetPreview({
  label,
  sequence,
  compact = false,
}: SequencePresetPreviewProps) {
  const phaseSize = compact ? 18 : SEQUENCE_MENU_PHASE_SIZE

  return (
    <Box
      sx={{
        alignItems: 'center',
        display: 'flex',
        gap: 1,
        minWidth: 0,
        width: '100%',
      }}
    >
      <Typography
        variant="body2"
        sx={{
          color: 'text.primary',
          lineHeight: 1,
          minWidth: compact ? 86 : SEQUENCE_MENU_LABEL_WIDTH,
          whiteSpace: 'nowrap',
        }}
      >
        {label}
      </Typography>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.25 }}>
        {sequence.map((band, bandIndex) => (
          <Box key={bandIndex} sx={{ display: 'flex', gap: 0.25 }}>
            {band.map((phase, phaseIndex) => {
              const isChanged = isPhaseChangedFromDefault(
                phase,
                bandIndex,
                phaseIndex
              )

              return (
                <React.Fragment key={`${bandIndex}-${phaseIndex}-${phase}`}>
                  {phaseIndex === 2 && (
                    <Box
                      aria-hidden
                      sx={{
                        alignSelf: 'stretch',
                        bgcolor: 'divider',
                        mx: 0.5,
                        width: 2,
                      }}
                    />
                  )}
                  <Box
                    sx={{
                      alignItems: 'center',
                      backgroundColor: isChanged
                        ? (theme) => alpha(theme.palette.primary.main, 0.12)
                        : 'background.default',
                      border: '1px solid',
                      borderColor: isChanged
                        ? (theme) => alpha(theme.palette.primary.main, 0.35)
                        : 'divider',
                      borderRadius: 0.5,
                      color: isChanged ? 'primary.dark' : 'text.secondary',
                      display: 'flex',
                      fontSize: compact ? '0.7rem' : '0.75rem',
                      height: phaseSize,
                      justifyContent: 'center',
                      lineHeight: 1,
                      width: phaseSize,
                    }}
                  >
                    {phase}
                  </Box>
                </React.Fragment>
              )
            })}
          </Box>
        ))}
      </Box>
    </Box>
  )
}

interface CoordinatedPhasePreviewProps {
  label: string
}

function CoordinatedPhasePreview({ label }: CoordinatedPhasePreviewProps) {
  return <Typography variant="body2">{label}</Typography>
}

interface CoordinatedTableProps {
  routeRows: TimeSpaceRouteRow[]
  locationWithSequence: LocationWithSequence[]
  updateLocationWithSequence: (
    locationWithSequence: LocationWithSequence
  ) => void
  locationWithCoordPhases: LocationWithCoordPhases[]
  updateLocationWithCoordPhases: (
    locationWithCoordPhases: LocationWithCoordPhases
  ) => void
}

const SequenceAndCoordinationComponent = ({
  routeRows,
  locationWithSequence,
  updateLocationWithSequence,
  locationWithCoordPhases,
  updateLocationWithCoordPhases,
}: CoordinatedTableProps) => {
  const [manualSequenceLocationIds, setManualSequenceLocationIds] = useState<
    string[]
  >([])
  const [manualCoordPhaseLocationIds, setManualCoordPhaseLocationIds] =
    useState<string[]>([])
  const [manualSequenceValuesByLocation, setManualSequenceValuesByLocation] =
    useState<Record<string, string[]>>({})
  const [
    manualCoordPhaseValuesByLocation,
    setManualCoordPhaseValuesByLocation,
  ] = useState<Record<string, string>>({})

  const addManualSequenceLocation = (locationId: string) => {
    setManualSequenceLocationIds((prev) =>
      prev.includes(locationId) ? prev : [...prev, locationId]
    )
  }

  const removeManualSequenceLocation = (locationId: string) => {
    setManualSequenceLocationIds((prev) =>
      prev.filter((id) => id !== locationId)
    )
  }

  const addManualCoordPhaseLocation = (locationId: string) => {
    setManualCoordPhaseLocationIds((prev) =>
      prev.includes(locationId) ? prev : [...prev, locationId]
    )
  }

  const removeManualCoordPhaseLocation = (locationId: string) => {
    setManualCoordPhaseLocationIds((prev) =>
      prev.filter((id) => id !== locationId)
    )
  }

  const updateSequenceForLocation = (
    location: LocationWithSequence,
    locationId: string,
    newSequence: number[][]
  ) => {
    updateLocationWithSequence({
      ...location,
      locationIdentifier: location.locationIdentifier ?? locationId,
      sequence: newSequence,
    })
  }

  const updateCoordPhasesForLocation = (
    locationId: string,
    coordinatedPhases: number[]
  ) => {
    const existingLocation = locationWithCoordPhases.find(
      (loc) => loc.locationIdentifier === locationId
    )
    const baseLocation = existingLocation ?? { locationIdentifier: locationId }

    updateLocationWithCoordPhases({
      ...baseLocation,
      locationIdentifier: baseLocation.locationIdentifier ?? locationId,
      coordinatedPhases,
    })
  }

  const getCoordPhaseInputValue = (locationId: string) => {
    const existingLocation = locationWithCoordPhases.find(
      (loc) => loc.locationIdentifier === locationId
    )

    return (
      existingLocation?.coordinatedPhases ?? DEFAULT_COORDINATED_PHASES
    ).join(',')
  }

  const handleSequencePresetChange = (
    location: LocationWithSequence,
    locationId: string,
    event: SelectChangeEvent<string>
  ) => {
    const newValue = event.target.value

    if (newValue === CUSTOM_SEQUENCE_VALUE) {
      addManualSequenceLocation(locationId)
      setManualSequenceValuesByLocation((prev) => ({
        ...prev,
        [locationId]:
          prev[locationId] ?? sequenceToInputValues(location.sequence),
      }))
      return
    }

    const selectedPreset = SEQUENCE_PRESETS.find(
      (preset) => preset.value === newValue
    )
    if (!selectedPreset) return

    removeManualSequenceLocation(locationId)
    setManualSequenceValuesByLocation((prev) => {
      const next = { ...prev }
      delete next[locationId]
      return next
    })
    updateSequenceForLocation(
      location,
      locationId,
      cloneSequence(selectedPreset.sequence)
    )
  }

  const handleSequenceChange = (
    location: LocationWithSequence,
    locationId: string,
    index: number,
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const currentValues =
      manualSequenceValuesByLocation[locationId] ??
      sequenceToInputValues(location.sequence)
    const newSequenceValues = [
      ...(currentValues.length ? currentValues : ['', '']),
    ]

    newSequenceValues[index] = event.target.value
    addManualSequenceLocation(locationId)
    setManualSequenceValuesByLocation((prev) => ({
      ...prev,
      [locationId]: newSequenceValues,
    }))
    updateSequenceForLocation(
      location,
      locationId,
      newSequenceValues.map(parseSequenceValue)
    )
  }

  const handleCoordPhasePresetChange = (
    locationId: string,
    event: SelectChangeEvent<string>
  ) => {
    const newValue = event.target.value

    if (newValue === CUSTOM_COORDINATED_PHASES_VALUE) {
      addManualCoordPhaseLocation(locationId)
      setManualCoordPhaseValuesByLocation((prev) => ({
        ...prev,
        [locationId]: prev[locationId] ?? getCoordPhaseInputValue(locationId),
      }))
      return
    }

    const selectedPreset = COORDINATED_PHASE_PRESETS.find(
      (preset) => preset.value === newValue
    )
    if (!selectedPreset) return

    removeManualCoordPhaseLocation(locationId)
    setManualCoordPhaseValuesByLocation((prev) => {
      const next = { ...prev }
      delete next[locationId]
      return next
    })
    updateCoordPhasesForLocation(locationId, clonePhases(selectedPreset.phases))
  }

  const handleCoordPhaseManualChange = (
    locationId: string,
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    addManualCoordPhaseLocation(locationId)
    setManualCoordPhaseValuesByLocation((prev) => ({
      ...prev,
      [locationId]: event.target.value,
    }))
    updateCoordPhasesForLocation(locationId, parsePhaseList(event.target.value))
  }

  return (
    <TableContainer sx={{ width: '100%' }}>
      <Table
        size="small"
        sx={{
          tableLayout: 'fixed',
          width: '100%',
        }}
      >
        <TableHead>
          <TableRow>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.location,
              }}
            >
              Location
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.primaryPhase,
              }}
            >
              Primary Phase
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.primarySpeed,
              }}
            >
              Speed (mph)
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.opposingPhase,
              }}
            >
              Opposing Phase
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.opposingSpeed,
              }}
            >
              Speed (mph)
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.distance,
              }}
            >
              Distance
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.sequence,
              }}
            >
              Sequence
            </TableCell>
            <TableCell
              sx={{
                fontSize: '0.75rem',
                px: TABLE_CELL_PX,
                width: TABLE_COLUMN_WIDTHS.coordinatedPhases,
              }}
            >
              Coordinated Phases
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {routeRows.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8} align="center">
                No locations on route
              </TableCell>
            </TableRow>
          ) : (
            routeRows.map((row, index) => {
              const locationId = row.locationIdentifier
              const location =
                locationWithSequence.find(
                  (item) => item.locationIdentifier === locationId
                ) ??
                ({
                  locationIdentifier: locationId,
                  sequence: DEFAULT_SEQUENCE,
                } as LocationWithSequence)
              const sequenceSelectValue = manualSequenceLocationIds.includes(
                locationId
              )
                ? CUSTOM_SEQUENCE_VALUE
                : getSequencePresetValue(location.sequence)
              const customSequenceValues =
                manualSequenceValuesByLocation[locationId] ??
                sequenceToInputValues(location.sequence)
              const coordinatedLocation = locationWithCoordPhases.find(
                (coordLocation) =>
                  coordLocation.locationIdentifier === locationId
              )
              const coordinatedPhases =
                coordinatedLocation?.coordinatedPhases ??
                DEFAULT_COORDINATED_PHASES
              const coordPhaseSelectValue =
                manualCoordPhaseLocationIds.includes(locationId)
                  ? CUSTOM_COORDINATED_PHASES_VALUE
                  : getCoordinatedPhasePresetValue(coordinatedPhases)
              const customCoordPhaseValue =
                manualCoordPhaseValuesByLocation[locationId] ??
                coordinatedPhases.join(',')
              const isCustomRow =
                sequenceSelectValue === CUSTOM_SEQUENCE_VALUE ||
                coordPhaseSelectValue === CUSTOM_COORDINATED_PHASES_VALUE

              return (
                <TableRow
                  key={locationId}
                  sx={
                    isCustomRow
                      ? (theme) => ({
                          backgroundColor: alpha(
                            theme.palette.primary.main,
                            0.04
                          ),
                          boxShadow: `inset 3px 0 0 ${theme.palette.primary.main}`,
                          '& > .MuiTableCell-root': {
                            borderBottomColor: alpha(
                              theme.palette.primary.main,
                              0.2
                            ),
                            borderTop: '1px solid',
                            borderTopColor: alpha(
                              theme.palette.primary.main,
                              0.2
                            ),
                          },
                          '&:hover': {
                            backgroundColor: alpha(
                              theme.palette.primary.main,
                              0.06
                            ),
                          },
                        })
                      : undefined
                  }
                >
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.location,
                    }}
                  >
                    {locationId}
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.primaryPhase,
                    }}
                  >
                    {row.primaryPhaseDescription ?? '-'}
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.primarySpeed,
                    }}
                  >
                    <ChipCell value={row.primaryMph} />
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.opposingPhase,
                    }}
                  >
                    {row.opposingPhaseDescription ?? '-'}
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.opposingSpeed,
                    }}
                  >
                    <ChipCell value={row.opposingMph} />
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.distance,
                    }}
                  >
                    {index === routeRows.length - 1 ? null : (
                      <ChipCell value={row.distance} />
                    )}
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.sequence,
                    }}
                  >
                    <FormControl variant="outlined" size="small" fullWidth>
                      <Select
                        value={sequenceSelectValue}
                        onChange={(event) =>
                          handleSequencePresetChange(
                            location,
                            locationId,
                            event
                          )
                        }
                        MenuProps={{
                          PaperProps: {
                            sx: { maxHeight: 430 },
                          },
                        }}
                        renderValue={(value) => {
                          const selectedPreset = SEQUENCE_PRESETS.find(
                            (preset) => preset.value === value
                          )

                          return selectedPreset?.label ?? 'Custom'
                        }}
                      >
                        {SEQUENCE_PRESETS.map((preset) => (
                          <MenuItem
                            key={preset.value}
                            value={preset.value}
                            sx={{
                              minHeight: 42,
                              my: SEQUENCE_MENU_ITEM_MY,
                              px: 1.25,
                              py: 0.5,
                            }}
                          >
                            <SequencePresetPreview
                              label={preset.label}
                              sequence={preset.sequence}
                            />
                          </MenuItem>
                        ))}
                        <MenuItem value={CUSTOM_SEQUENCE_VALUE}>
                          <Box sx={{ minWidth: 0 }}>
                            <Typography variant="body2">Custom</Typography>
                            <Typography
                              variant="caption"
                              color="text.secondary"
                            >
                              Enter rings manually
                            </Typography>
                          </Box>
                        </MenuItem>
                      </Select>
                    </FormControl>
                    {sequenceSelectValue === CUSTOM_SEQUENCE_VALUE && (
                      <Box
                        sx={{
                          display: 'grid',
                          gap: 0.5,
                          gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
                          mt: 0.75,
                        }}
                      >
                        {customSequenceValues.map((value, ringIndex) => (
                          <TextField
                            key={ringIndex}
                            label={`Ring ${ringIndex + 1}`}
                            variant="outlined"
                            value={value}
                            size="small"
                            onChange={(event) =>
                              handleSequenceChange(
                                location,
                                locationId,
                                ringIndex,
                                event
                              )
                            }
                          />
                        ))}
                      </Box>
                    )}
                  </TableCell>
                  <TableCell
                    sx={{
                      px: TABLE_CELL_PX,
                      verticalAlign: 'top',
                      width: TABLE_COLUMN_WIDTHS.coordinatedPhases,
                    }}
                  >
                    <FormControl variant="outlined" size="small" fullWidth>
                      <Select
                        value={coordPhaseSelectValue}
                        onChange={(event) =>
                          handleCoordPhasePresetChange(locationId, event)
                        }
                        renderValue={(value) => {
                          const selectedPreset =
                            COORDINATED_PHASE_PRESETS.find(
                              (preset) => preset.value === value
                            )

                          return selectedPreset?.label ?? 'Manual'
                        }}
                      >
                        {COORDINATED_PHASE_PRESETS.map((preset) => (
                          <MenuItem key={preset.value} value={preset.value}>
                            <CoordinatedPhasePreview label={preset.label} />
                          </MenuItem>
                        ))}
                        <MenuItem value={CUSTOM_COORDINATED_PHASES_VALUE}>
                          <Box sx={{ minWidth: 0 }}>
                            <Typography variant="body2">Manual</Typography>
                            <Typography
                              variant="caption"
                              color="text.secondary"
                            >
                              Enter phases manually
                            </Typography>
                          </Box>
                        </MenuItem>
                      </Select>
                    </FormControl>
                    {coordPhaseSelectValue ===
                      CUSTOM_COORDINATED_PHASES_VALUE && (
                      <TextField
                        label="Phases"
                        variant="outlined"
                        value={customCoordPhaseValue}
                        size="small"
                        onChange={(event) =>
                          handleCoordPhaseManualChange(locationId, event)
                        }
                        fullWidth
                        sx={{ mt: 0.75 }}
                      />
                    )}
                  </TableCell>
                </TableRow>
              )
            })
          )}
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default SequenceAndCoordinationComponent
