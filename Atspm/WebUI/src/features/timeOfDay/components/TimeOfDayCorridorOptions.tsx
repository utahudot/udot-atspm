import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import MultipleLocationsSelect from '@/components/MultipleLocationsSelect/MultipleLocationsSelect'
import {
  DragDropContext,
  Draggable,
  Droppable,
  type DropResult,
} from '@hello-pangea/dnd'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import DragIndicatorIcon from '@mui/icons-material/DragIndicator'
import LocationOnOutlinedIcon from '@mui/icons-material/LocationOnOutlined'
import {
  Box,
  Button,
  Chip,
  IconButton,
  Paper,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import type { TimeOfDayFormState } from '../types'

interface TimeOfDayCorridorOptionsProps {
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
}

export default function TimeOfDayCorridorOptions({
  options,
  onChange,
}: TimeOfDayCorridorOptionsProps) {
  const [focusedLocation, setFocusedLocation] = useState<
    TimeOfDayFormState['selectedLocations'][number] | null
  >(null)
  const [highlightedLocationId, setHighlightedLocationId] = useState<number>()

  const updateLocations = (
    selectedLocations: TimeOfDayFormState['selectedLocations']
  ) => onChange({ ...options, selectedLocations })

  const handleDeleteLocation = (
    locationId?: number,
    identifier?: string | null
  ) => {
    const isDeletedLocation = (
      location: TimeOfDayFormState['selectedLocations'][number]
    ) =>
      (locationId != null && location.id === locationId) ||
      (Boolean(identifier) && location.locationIdentifier === identifier)

    updateLocations(
      options.selectedLocations.filter(
        (location) => !isDeletedLocation(location)
      )
    )

    if (focusedLocation && isDeletedLocation(focusedLocation)) {
      setFocusedLocation(null)
    }
  }

  const handleLocationDragEnd = ({ source, destination }: DropResult) => {
    if (!destination || source.index === destination.index) return

    const selectedLocations = [...options.selectedLocations]
    const [movedLocation] = selectedLocations.splice(source.index, 1)
    selectedLocations.splice(destination.index, 0, movedLocation)
    updateLocations(selectedLocations)
  }

  return (
    <Paper
      sx={{
        display: 'flex',
        flexDirection: 'column',
        width: { xs: '100%', lg: 'min(100%, 1050px)' },
        maxWidth: '100%',
        height: 600,
        alignSelf: 'stretch',
        overflow: 'hidden',
        borderRadius: 1.5,
        '& > :first-of-type': { mb: 0 },
      }}
    >
      <StyledComponentHeader header="Corridor" />
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
          gap: 0,
          flex: 1,
          minHeight: 442,
        }}
      >
        <Box sx={{ minWidth: 0, minHeight: 0, height: '100%', p: 2 }}>
          <MultipleLocationsSelect
            selectedLocations={options.selectedLocations}
            focusedLocation={focusedLocation}
            highlightedLocationId={highlightedLocationId}
            showSelectionModeToggle
            size="small"
            mapHeight={315}
            fillAvailableHeight
            setLocations={updateLocations}
          />
        </Box>
        <Box
          sx={{
            minWidth: 0,
            display: 'flex',
            flexDirection: 'column',
            border: 1,
            borderColor: 'divider',
            borderRadius: 1,
            overflow: 'hidden',
            minHeight: { xs: 180, md: 410 },
            height: { md: 'calc(100% - 32px)' },
            alignSelf: 'start',
            m: 2,
            mt: { xs: 0, md: 2 },
            ml: { md: 0 },
            bgcolor: 'background.paper',
          }}
        >
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 1,
              px: 2,
              py: 0.75,
              bgcolor: 'action.hover',
              borderBottom: 1,
              borderColor: 'divider',
            }}
          >
            <Stack spacing={0.25}>
              <Stack direction="row" alignItems="center" spacing={1}>
                <Typography variant="subtitle2">Selected locations</Typography>
                <Chip
                  size="small"
                  label={options.selectedLocations.length}
                  aria-label={`${options.selectedLocations.length} selected locations`}
                  sx={{ height: 22, minWidth: 28 }}
                />
              </Stack>
              {options.selectedLocations.length > 1 && (
                <Stack
                  direction="row"
                  alignItems="center"
                  spacing={0.5}
                  color="text.secondary"
                >
                  <Typography variant="caption">
                    {options.selectedLocations[0].locationIdentifier}
                  </Typography>
                  <ArrowForwardIcon sx={{ fontSize: 14 }} />
                  <Typography variant="caption">
                    {
                      options.selectedLocations[
                        options.selectedLocations.length - 1
                      ].locationIdentifier
                    }
                  </Typography>
                  <Typography variant="caption">· drag to reorder</Typography>
                </Stack>
              )}
            </Stack>
            <Button
              size="small"
              color="error"
              startIcon={<DeleteOutlineIcon />}
              disabled={!options.selectedLocations.length}
              onClick={() => {
                setFocusedLocation(null)
                setHighlightedLocationId(undefined)
                updateLocations([])
              }}
              sx={{ textTransform: 'none' }}
            >
              Clear all
            </Button>
          </Box>
          {options.selectedLocations.length === 0 ? (
            <Stack
              alignItems="center"
              justifyContent="center"
              spacing={1}
              sx={{
                flex: 1,
                minHeight: 150,
                px: 3,
                py: 4,
                textAlign: 'center',
                color: 'text.secondary',
              }}
            >
              <LocationOnOutlinedIcon sx={{ fontSize: 34, opacity: 0.6 }} />
              <Box>
                <Typography variant="subtitle2" color="text.primary">
                  No locations selected
                </Typography>
                <Typography variant="body2">
                  Add a route, choose a location, or select one on the map.
                </Typography>
              </Box>
            </Stack>
          ) : (
            <DragDropContext onDragEnd={handleLocationDragEnd}>
              <Droppable droppableId="time-of-day-locations">
                {(dropProvided) => (
                  <Box
                    component="ol"
                    ref={dropProvided.innerRef}
                    {...dropProvided.droppableProps}
                    aria-label="Selected time-of-day locations"
                    sx={{
                      m: 0,
                      p: 0,
                      flex: 1,
                      minHeight: 0,
                      listStyle: 'none',
                      overflowY: 'auto',
                    }}
                  >
                    {options.selectedLocations.map((location, index) => {
                      const locationName = [
                        location.primaryName,
                        location.secondaryName,
                      ]
                        .filter(Boolean)
                        .join(' ')
                      const locationLabel =
                        location.locationIdentifier || 'Unknown location'
                      const draggableId = `${location.id ?? 'location'}-${
                        location.locationIdentifier ?? 'unknown'
                      }`

                      return (
                        <Draggable
                          key={draggableId}
                          draggableId={draggableId}
                          index={index}
                        >
                          {(dragProvided, dragSnapshot) => (
                            <Box
                              component="li"
                              ref={dragProvided.innerRef}
                              {...dragProvided.draggableProps}
                              tabIndex={0}
                              onMouseEnter={() =>
                                setHighlightedLocationId(location.id)
                              }
                              onMouseLeave={() =>
                                setHighlightedLocationId(undefined)
                              }
                              onFocus={() =>
                                setHighlightedLocationId(location.id)
                              }
                              onBlur={() => setHighlightedLocationId(undefined)}
                              onClick={() => setFocusedLocation(location)}
                              onKeyDown={(event) => {
                                if (
                                  event.key === 'Enter' ||
                                  event.key === ' '
                                ) {
                                  event.preventDefault()
                                  setFocusedLocation(location)
                                }
                              }}
                              aria-label={`${locationLabel}, position ${
                                index + 1
                              }. Click to show on map.`}
                              sx={{
                                display: 'grid',
                                gridTemplateColumns:
                                  '20px 28px minmax(0, 1fr) 28px',
                                alignItems: 'center',
                                gap: 0.75,
                                px: 0.5,
                                py: 0.5,
                                borderBottom: 1,
                                borderColor: 'divider',
                                bgcolor: dragSnapshot.isDragging
                                  ? 'action.selected'
                                  : focusedLocation?.id === location.id
                                    ? 'action.selected'
                                    : 'background.paper',
                                cursor: 'pointer',
                                outline: 'none',
                                '&:last-of-type': { borderBottom: 0 },
                                '&:hover, &:focus-visible': {
                                  bgcolor: 'action.hover',
                                },
                                ...dragProvided.draggableProps.style,
                              }}
                            >
                              <Tooltip title="Drag to reorder">
                                <IconButton
                                  size="small"
                                  aria-label={`Reorder ${locationLabel}`}
                                  {...dragProvided.dragHandleProps}
                                  onClick={(event) => event.stopPropagation()}
                                  sx={{
                                    ml: -0.5,
                                    color: 'text.disabled',
                                    cursor: 'grab',
                                    '&:active': { cursor: 'grabbing' },
                                  }}
                                >
                                  <DragIndicatorIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                              <Box
                                aria-hidden="true"
                                sx={{
                                  width: 24,
                                  height: 24,
                                  borderRadius: '50%',
                                  display: 'grid',
                                  placeItems: 'center',
                                  bgcolor: 'primary.main',
                                  color: 'primary.contrastText',
                                  typography: 'caption',
                                  fontWeight: 700,
                                }}
                              >
                                {index + 1}
                              </Box>
                              <Box sx={{ minWidth: 0 }}>
                                <Typography
                                  variant="body2"
                                  sx={{ fontWeight: 600, lineHeight: 1.35 }}
                                >
                                  {locationLabel}
                                </Typography>
                                {locationName && (
                                  <Typography
                                    variant="caption"
                                    color="text.secondary"
                                    sx={{
                                      display: 'block',
                                      overflow: 'hidden',
                                      textOverflow: 'ellipsis',
                                      whiteSpace: 'nowrap',
                                    }}
                                  >
                                    {locationName}
                                  </Typography>
                                )}
                              </Box>
                              <Tooltip title="Remove location">
                                <IconButton
                                  size="small"
                                  aria-label={`Remove ${locationLabel}`}
                                  onClick={(event) => {
                                    event.stopPropagation()
                                    handleDeleteLocation(
                                      location.id,
                                      location.locationIdentifier
                                    )
                                  }}
                                  sx={{
                                    color: 'text.secondary',
                                    '&:hover': {
                                      color: 'error.main',
                                      bgcolor: 'action.hover',
                                    },
                                  }}
                                >
                                  <DeleteOutlineIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            </Box>
                          )}
                        </Draggable>
                      )
                    })}
                    {dropProvided.placeholder}
                  </Box>
                )}
              </Droppable>
            </DragDropContext>
          )}
        </Box>
      </Box>
    </Paper>
  )
}
