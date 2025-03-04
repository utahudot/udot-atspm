import {
  TspErrorState,
  TspLocation,
} from '@/pages/reports/transit-signal-priority'
import {
  DragDropContext,
  Draggable,
  Droppable,
  DropResult,
} from '@hello-pangea/dnd'
import DeleteIcon from '@mui/icons-material/Delete'
import DragIndicatorIcon from '@mui/icons-material/DragIndicator'
import {
  Box,
  Button,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'

interface Props {
  locations: TspLocation[]
  onLocationDelete: (location: TspLocation) => void
  onDeleteAllLocations: (locations: TspLocation[]) => void
  onLocationsReorder: (result: DropResult) => void
  onUpdateLocation: (updatedLocation: TspLocation) => void
  errorState: TspErrorState
}

export default function MultipleLocationsDisplay({
  locations,
  onLocationDelete,
  onDeleteAllLocations,
  onLocationsReorder,
  onUpdateLocation,
  errorState,
}: Props) {
  function handlePhaseChange(location: TspLocation, selectedPhases: number[]) {
    onUpdateLocation({ ...location, designatedPhases: selectedPhases })
  }

  if (!locations.length) {
    return (
      <Typography variant="body2" color="textSecondary" sx={{ width: '600px' }}>
        No locations selected
      </Typography>
    )
  }

  return (
    <>
      <DragDropContext onDragEnd={onLocationsReorder}>
        <Box display="flex" justifyContent="flex-end">
          <Button
            startIcon={<DeleteIcon />}
            variant="outlined"
            color="error"
            size="small"
            onClick={() => onDeleteAllLocations(locations)}
            sx={{ mb: 1 }}
          >
            Remove All
          </Button>
        </Box>
        <Droppable droppableId="locations">
          {(provided) => (
            <Box
              sx={{
                maxHeight: '505px',
                width: '600px',
                overflowY: 'auto',
              }}
              ref={provided.innerRef}
              {...provided.droppableProps}
            >
              <Table size="small" aria-label="draggable table" stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell>
                      <Typography variant="subtitle2">
                        Selected Locations
                      </Typography>
                    </TableCell>
                    <TableCell align="center">Designated Phases</TableCell>
                    <TableCell />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {locations.map((location, index) => {
                    const locationPhaseOptions = Array.from(
                      new Set(
                        location.approaches?.map(
                          (a) => a.protectedPhaseNumber
                        ) || []
                      )
                    ).sort((a, b) => a - b)

                    const isInError =
                      errorState.type === 'MISSING_PHASES' &&
                      errorState.locationIDs.has(String(location.id))

                    return (
                      <Draggable
                        key={location.id}
                        draggableId={
                          location.locationIdentifier ?? index.toString()
                        }
                        index={index}
                      >
                        {(provided) => (
                          <TableRow
                            hover
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                          >
                            <TableCell
                              sx={{ pl: 0.5 }}
                              {...provided.dragHandleProps}
                            >
                              <Box display="flex" alignItems="center">
                                <DragIndicatorIcon />
                                <Divider
                                  orientation="vertical"
                                  variant="fullWidth"
                                  flexItem
                                />
                                <Box ml={1}>
                                  {`${location.locationIdentifier} - ${location.primaryName} ${location.secondaryName}`}
                                </Box>
                              </Box>
                            </TableCell>
                            <TableCell align="center">
                              <FormControl
                                variant="outlined"
                                size="small"
                                sx={{ minWidth: 150 }}
                                error={isInError}
                                fullWidth
                              >
                                <InputLabel id={`phase-label-${location.id}`}>
                                  Phases
                                </InputLabel>
                                <Select
                                  labelId={`phase-label-${location.id}`}
                                  multiple
                                  label="Phases"
                                  value={location.designatedPhases || []}
                                  onChange={(e) =>
                                    handlePhaseChange(
                                      location,
                                      e.target.value as number[]
                                    )
                                  }
                                  renderValue={(selected) =>
                                    (selected as number[]).join(', ')
                                  }
                                >
                                  {locationPhaseOptions.length > 0 ? (
                                    locationPhaseOptions.map((phase) => (
                                      <MenuItem key={phase} value={phase}>
                                        {phase}
                                      </MenuItem>
                                    ))
                                  ) : (
                                    <MenuItem disabled>
                                      No Phases Available
                                    </MenuItem>
                                  )}
                                </Select>
                              </FormControl>
                            </TableCell>
                            <TableCell align="right">
                              <IconButton
                                color="error"
                                onClick={() => onLocationDelete(location)}
                              >
                                <DeleteIcon />
                              </IconButton>
                            </TableCell>
                          </TableRow>
                        )}
                      </Draggable>
                    )
                  })}
                  {provided.placeholder}
                </TableBody>
              </Table>
            </Box>
          )}
        </Droppable>
      </DragDropContext>
    </>
  )
}
