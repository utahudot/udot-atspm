import DirectionSelect from '@/features/routes/components/routeEditor/DirectionsSelect'
import DistanceInput from '@/features/routes/components/routeEditor/DistanceInput'
import { Route, RouteLocation } from '@/features/routes/types'
import { Draggable } from '@hello-pangea/dnd'
import DeleteIcon from '@mui/icons-material/Delete'
import DragIndicatorIcon from '@mui/icons-material/DragIndicator'
import {
  Box,
  Divider,
  IconButton,
  TableCell,
  TableRow,
  Tooltip,
  useTheme,
} from '@mui/material'
import React from 'react'

interface RouteRowProps {
  link: RouteLocation
  index: number
  route: Route
  hasErrors: boolean
  setHasErrors: React.Dispatch<React.SetStateAction<boolean>>
  handleDirectionUpdate: (updatedLink: RouteLocation) => void
  handleDistanceChange: (link: RouteLocation, distance: number) => void
  onDeleteLinkClick: (linkId: string) => void
}

const RouteRow = ({
  link,
  index,
  route,
  hasErrors,
  setHasErrors,
  handleDirectionUpdate,
  handleDistanceChange,
  onDeleteLinkClick,
}: RouteRowProps) => {
  const theme = useTheme()

  return (
    <Draggable draggableId={link.locationIdentifier} index={index}>
      {(provided, snapshot) => (
        <TableRow
          ref={provided.innerRef}
          {...provided.draggableProps}
          {...provided.dragHandleProps}
          style={{
            ...provided.draggableProps.style,
            backgroundColor: snapshot.isDragging
              ? theme.palette.action.hover
              : 'inherit',
          }}
        >
          <TableCell sx={{ pl: 0.5 }}>
            <Box display="flex" alignItems="center">
              <DragIndicatorIcon />
              <Divider orientation="vertical" variant="fullWidth" flexItem />
              <Box ml={1}>
                #{link.locationIdentifier} - {link.primaryName} &{' '}
                {link.secondaryName}
              </Box>
            </Box>
          </TableCell>
          <TableCell align="center" sx={{ width: '180px' }}>
            <DirectionSelect
              hasErrors={hasErrors}
              updateType="primary"
              link={link}
              onUpdate={handleDirectionUpdate}
            />
          </TableCell>
          <TableCell align="center" sx={{ width: '180px' }}>
            <DirectionSelect
              hasErrors={hasErrors}
              updateType="opposing"
              link={link}
              onUpdate={handleDirectionUpdate}
            />
          </TableCell>
          <TableCell align="center" sx={{ pr: 0 }}>
            <DistanceInput
              hasErrors={hasErrors}
              link={link}
              nextLink={route.routeLocations[index + 1]}
              handleDistanceChange={handleDistanceChange}
            />
          </TableCell>
          <TableCell align="center" sx={{ width: '10px', px: 1 }}>
            <Tooltip title="Delete Location">
              <IconButton
                color="error"
                aria-label="delete"
                onClick={() => onDeleteLinkClick(link.locationIdentifier)}
              >
                <DeleteIcon />
              </IconButton>
            </Tooltip>
          </TableCell>
        </TableRow>
      )}
    </Draggable>
  )
}

export default RouteRow
