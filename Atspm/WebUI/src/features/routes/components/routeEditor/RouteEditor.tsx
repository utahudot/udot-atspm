import { Location } from '@/features/locations/types'
import RouteRow from '@/features/routes/components/routeEditor/RouteRow'
import { Route, RouteLocation } from '@/features/routes/types'
import { DragDropContext, DropResult, Droppable } from '@hello-pangea/dnd'
import ArrowRightAltIcon from '@mui/icons-material/ArrowRightAlt'
import SaveIcon from '@mui/icons-material/Save'
import {
  Alert,
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { memo } from 'react'

interface RouteEditorProps {
  route: Route
  location?: Location | null
  hasErrors: boolean
  onAddRoute: () => void
  onDragEnd: (result: DropResult) => void
  handleDistanceChange: (link: RouteLocation, distance: number) => void
  handleDirectionUpdate: (updatedLink: RouteLocation) => void
  handleDeleteLink: (link: RouteLocation) => void
  handleSaveRoute: () => void
}

const RouteEditor = ({
  route,
  location,
  hasErrors,
  onAddRoute,
  onDragEnd,
  handleDistanceChange,
  handleDirectionUpdate,
  handleDeleteLink,
  handleSaveRoute,
}: RouteEditorProps) => {
  const onDeleteLinkClick = (linkId: string) => {
    const linkToDelete = route.routeLocations.find(
      (link) => link.locationIdentifier === linkId
    )
    if (linkToDelete) {
      handleDeleteLink(linkToDelete)
    }
  }

  return (
    <Box sx={{ minWidth: '800px', width: '1000px', marginX: 2 }}>
      <Box display={'flex'} justifyContent={'space-between'} marginBottom={2}>
        <Button
          variant="contained"
          color="success"
          onClick={onAddRoute}
          disabled={!location}
        >
          <ArrowRightAltIcon sx={{ marginRight: 1 }} />
          Add to Route
        </Button>
        <Box display={'flex'}>
          {hasErrors && (
            <Alert severity="error" sx={{ mr: 2 }}>
              The highlighted fields are required.
            </Alert>
          )}
          <Button variant="contained" color="success" onClick={handleSaveRoute}>
            <SaveIcon sx={{ mr: 1 }} />
            Save Route
          </Button>
        </Box>
      </Box>
      <DragDropContext onDragEnd={onDragEnd}>
        <Droppable droppableId="droppable">
          {(provided) => (
            <TableContainer
              component={Paper}
              {...provided.droppableProps}
              ref={provided.innerRef}
            >
              <Table sx={{ minWidth: 650 }} aria-label="route table">
                <TableHead>
                  <TableRow>
                    <TableCell>Location</TableCell>
                    <TableCell width={'200px'}>
                      <Box
                        display={'flex'}
                        flexDirection={'column'}
                        alignItems={'center'}
                        width={'100%'}
                      >
                        Primary Direction
                        <Box
                          display={'flex'}
                          justifyContent={'space-around'}
                          width={'100%'}
                        >
                          <Typography variant="caption">Dir.</Typography>
                          <Typography variant="caption">Phase</Typography>
                          <Typography variant="caption">Overlap</Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box
                        display={'flex'}
                        flexDirection={'column'}
                        alignItems={'center'}
                        width={'100%'}
                      >
                        Opposing Direction
                        <Box
                          display={'flex'}
                          justifyContent={'space-around'}
                          width={'100%'}
                        >
                          <Typography variant="caption">Dir.</Typography>
                          <Typography variant="caption">Phase</Typography>
                          <Typography variant="caption">Overlap</Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>Distance to Next</TableCell>
                    <TableCell sx={{ width: '10px' }}>Delete</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {route.routeLocations.map((link, index) => (
                    <RouteRow
                      hasErrors={hasErrors}
                      key={link.locationIdentifier}
                      link={link}
                      index={index}
                      route={route}
                      handleDirectionUpdate={handleDirectionUpdate}
                      handleDistanceChange={handleDistanceChange}
                      onDeleteLinkClick={onDeleteLinkClick}
                    />
                  ))}
                  {provided.placeholder}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </Droppable>
      </DragDropContext>
    </Box>
  )
}

export default memo(RouteEditor)
