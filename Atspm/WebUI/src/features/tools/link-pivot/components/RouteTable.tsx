import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'

interface SimpleRow {
  locationIdentifier: string
  primaryName: string
  secondaryName: string
}

interface SimpleProps {
  data: SimpleRow[]
}

const RouteTable = ({ data }: SimpleProps) => (
  <TableContainer>
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell sx={{ fontSize: '0.75rem', px: 1 }}>Location</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {data.length === 0 ? (
          <TableRow>
            <TableCell colSpan={3} align="center">
              No data available
            </TableCell>
          </TableRow>
        ) : (
          data.map((row) => (
            <TableRow key={row.locationIdentifier}>
              <TableCell sx={{ px: 1 }}>
                <Typography variant="body2">
                  {row.locationIdentifier}
                </Typography>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  </TableContainer>
)

export default RouteTable
