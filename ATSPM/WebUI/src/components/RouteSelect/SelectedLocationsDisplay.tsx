import {
  ExpandApproachForAggregation,
  ExpandLocationForAggregation,
  ExpandLocationHandler,
} from '@/features/data/aggregate/handlers/expandLocationHandler'
import CloseIcon from '@mui/icons-material/Close'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp'
import {
  Box,
  Checkbox,
  FormControlLabel,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { styled } from '@mui/material/styles'

const CheckboxCell = styled(TableCell)({
  width: '10%', // Set a fixed width for checkbox cells
  paddingRight: 0, // Remove padding to ensure alignment
  paddingLeft: 0, // Remove padding to ensure alignment
  textAlign: 'center', // Center the checkbox within the cell
})

interface props {
  handler: ExpandLocationHandler
}

const SelectedLocationsDisplay = ({ handler }: props) => {
  const renderDetectorCollapsibleRows = (
    location: ExpandLocationForAggregation,
    approach: ExpandApproachForAggregation
  ) => {
    return approach.detectors.map((detector, index) => (
      <TableRow key={index}>
        <TableCell></TableCell>
        <TableCell
          style={{
            maxWidth: '200px',
          }}
        >
          {`Detector Channel: ${detector.detChannel} / Lane Number: ${detector.laneNumber} / Lane Type: ${detector.laneType}`}
        </TableCell>
        <CheckboxCell>
          <Checkbox
            checked={approach.exclude}
            onChange={() =>
              handler.updateDetectorExclude(location, approach, detector)
            }
            style={{ marginRight: 25 }}
          />
        </CheckboxCell>
      </TableRow>
    ))
  }

  const renderApproachCollapsibleRows = (
    location: ExpandLocationForAggregation
  ) => {
    return location.approaches.map((approach, index) => [
      <TableRow key={index}>
        <TableCell>
          <IconButton
            size="small"
            onClick={() => handler.updateApproachOpen(location, approach)}
            style={{ marginLeft: 10 }}
          >
            {approach.open ? (
              <KeyboardArrowDownIcon />
            ) : (
              <KeyboardArrowUpIcon />
            )}
          </IconButton>
        </TableCell>
        <TableCell>{approach.description}</TableCell>
        <CheckboxCell sx={{ marginRight: 10 }}>
          <Checkbox
            checked={approach.exclude}
            onChange={() => handler.updateApproachExclude(location, approach)}
            style={{ marginRight: 10 }}
          />
        </CheckboxCell>
      </TableRow>,
      approach.open && renderDetectorCollapsibleRows(location, approach),
    ])
  }

  const renderCollapsibleRows = () => {
    return handler.updatedLocations.map((location, index) => [
      <TableRow key={index}>
        <TableCell>
          <IconButton
            aria-label='collapse-button'
            size="small"
            onClick={() => handler.updateLocationOpen(location)}
          >
            {location.open ? (
              <KeyboardArrowDownIcon />
            ) : (
              <KeyboardArrowUpIcon />
            )}
          </IconButton>
        </TableCell>
        <TableCell component="th" scope="row">
          {`${location.locationIdentifier} - ${location.primaryName} ${location.secondaryName}`}
        </TableCell>
        <CheckboxCell>
          <FormControlLabel
            control={
          <Checkbox
            checked={location.exclude}
            onChange={() => handler.updateLocationExclude(location)}
          />
            }
            aria-label="excludeLocation" />
        </CheckboxCell>
        <TableCell style={{ textAlign: 'center' }}>
          <IconButton
            aria-label="x-button"
            size="small"
            onClick={() => handler.deleteLocation(location)}
          >
            <CloseIcon />
          </IconButton>
        </TableCell>
      </TableRow>,
      location.open && renderApproachCollapsibleRows(location),
    ])
  }

  return (
    <Box
      sx={{
        marginTop: '25px',
        maxHeight: '235px',
        overflowY: 'auto',
      }}
    >
      <Table size="small" aria-label="collapsible table">
        <TableHead>
          <TableRow
            sx={{
              '& .MuiTableCell-body': {
                fontSize: '1rem',
                borderRight: '1px solid #e0e0e0',
                lineHeight: 'inherit',
              },
            }}>
            <TableCell aria-label="expandable rows"></TableCell>
            <TableCell>
              <Typography fontWeight="bold">
                Location
              </Typography>
            </TableCell>
            <CheckboxCell>
              <Typography fontWeight="bold" marginRight="30px">
                Exclude
              </Typography>
            </CheckboxCell>
            <TableCell width="100px">
              <Typography fontWeight="bold">
                Remove Location
              </Typography>
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>{renderCollapsibleRows()}</TableBody>
      </Table>
    </Box>
  )
}

export default SelectedLocationsDisplay
