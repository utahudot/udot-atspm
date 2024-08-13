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
} from '@mui/material'

interface RouteCheckerProps {
  data: {
    locationIdentifier: string
    approachDescription: string | null
    mph: number | null
    distance: number | null
  }[]
}

const RouteChecker = ({ data }: RouteCheckerProps) => {
  const length = data.length

  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Location</TableCell>
            <TableCell>Phase</TableCell>
            <TableCell>Speed Limit</TableCell>
            <TableCell>Distance</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {length === 0 ? (
            <TableRow>
              <TableCell colSpan={4} align="center">
                No locations on route
              </TableCell>
            </TableRow>
          ) : (
            data.map((item, index) => (
              <TableRow key={index}>
                <TableCell>{item.locationIdentifier}</TableCell>
                <TableCell>{item.approachDescription}</TableCell>
                <TableCell>
                  <Chip
                    label={
                      item.mph !== null ? item.mph.toLocaleString() : 'N/A'
                    }
                    size="small"
                    icon={
                      item.mph !== null ? (
                        <CheckIcon color="success" />
                      ) : (
                        <CloseIcon color="error" />
                      )
                    }
                    color={item.mph !== null ? 'success' : 'error'}
                  />
                </TableCell>
                <TableCell>
                  {index + 1 - length === 0 ? null : (
                    <Chip
                      label={
                        item.distance !== null
                          ? item.distance.toLocaleString()
                          : 'N/A'
                      }
                      size="small"
                      icon={
                        item.distance !== null ? (
                          <CheckIcon color="success" />
                        ) : (
                          <CloseIcon color="error" />
                        )
                      }
                      color={item.distance !== null ? 'success' : 'error'}
                    />
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  )
}

export default RouteChecker
