import {
  ExpandApproachForAggregation,
  ExpandLocationForAggregation,
  ExpandLocationHandler,
} from '@/features/data/aggregate/handlers/expandLocationHandler'
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
  width: '10%',
  paddingRight: 0,
  paddingLeft: 0,
  textAlign: 'right',
})

interface Props {
  handler: ExpandLocationHandler
}

const SelectedLocationsDisplay = ({ handler }: Props) => {
  const renderDetectorCollapsibleRows = (
    location: ExpandLocationForAggregation,
    approach: ExpandApproachForAggregation
  ) => {
    return approach.detectors.map((detector, index) => (
      <TableRow key={index}>
        <TableCell></TableCell>
        <TableCell sx={{ minWidth: '180px' }}>
          {`Detector Channel: ${detector.detChannel} / Lane Number: ${detector.laneNumber} / Lane Type: ${detector.laneType}`}
        </TableCell>
        <CheckboxCell>
          <Checkbox
            checked={detector.exclude}
            onChange={() =>
              handler.updateDetectorExclude(location, approach, detector)
            }
          />
        </CheckboxCell>
      </TableRow>
    ))
  }

  const renderApproachCollapsibleRows = (
    location: ExpandLocationForAggregation
  ) => {
    return location.approaches.map((approach, index) => [
      <TableRow
        key={index}
        sx={{
          backgroundColor: '#f7f7f7', // Light gray background
        }}
      >
        <TableCell>
          <IconButton
            size="small"
            onClick={() => handler.updateApproachOpen(location, approach)}
          >
            {approach.open ? (
              <KeyboardArrowDownIcon />
            ) : (
              <KeyboardArrowUpIcon />
            )}
          </IconButton>
        </TableCell>
        <TableCell>{approach.description}</TableCell>
        <CheckboxCell>
          <Checkbox
            checked={approach.exclude}
            onChange={() => handler.updateApproachExclude(location, approach)}
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
            aria-label="collapse-button"
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
        <TableCell component="th" scope="row" sx={{ fontWeight: 'bold' }}>
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
            aria-label="excludeLocation"
          />
        </CheckboxCell>
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
            }}
          >
            <TableCell aria-label="expandable rows"></TableCell>
            <TableCell>
              <Typography fontWeight="bold">Location</Typography>
            </TableCell>
            <CheckboxCell>
              <Typography fontWeight="bold" marginRight="10px">
                Exclude
              </Typography>
            </CheckboxCell>
          </TableRow>
        </TableHead>
        <TableBody>{renderCollapsibleRows()}</TableBody>
      </Table>
    </Box>
  )
}

export default SelectedLocationsDisplay
