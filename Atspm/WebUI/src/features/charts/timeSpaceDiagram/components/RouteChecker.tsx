import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import {
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'

interface Row {
  locationIdentifier: string
  primaryPhaseDescription: string | null
  primaryMph: number | null
  opposingPhaseDescription: string | null
  opposingMph: number | null
  distance: number | null
}

interface Props {
  data: Row[]
}

const ChipCell = ({ value }: { value: number | null }) => (
  <Chip
    // label={value ?? 'N/A'}
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

const RouteChecker = ({ data }: Props) => (
  <TableContainer>
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>Location</TableCell>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>
            Primary Phase
          </TableCell>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>Speed (mph)</TableCell>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>
            Opposing Phase
          </TableCell>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>Speed (mph)</TableCell>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>Distance</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {data.length === 0 ? (
          <TableRow>
            <TableCell colSpan={6} align="center">
              No locations on route
            </TableCell>
          </TableRow>
        ) : (
          data.map((r, idx) => (
            <TableRow key={r.locationIdentifier}>
              <TableCell>{r.locationIdentifier}</TableCell>
              <TableCell>{r.primaryPhaseDescription ?? '—'}</TableCell>
              <TableCell>
                <ChipCell value={r.primaryMph} />
              </TableCell>
              <TableCell>{r.opposingPhaseDescription ?? '—'}</TableCell>
              <TableCell>
                <ChipCell value={r.opposingMph} />
              </TableCell>
              <TableCell>
                {idx === data.length - 1 ? null : (
                  <ChipCell value={r.distance} />
                )}
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  </TableContainer>
)

export default RouteChecker
