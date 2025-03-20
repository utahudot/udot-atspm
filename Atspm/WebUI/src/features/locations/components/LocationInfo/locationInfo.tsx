import { Location } from '@/features/locations/types'
import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import { Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material'

interface LocationInfoProps {
  location: Location
}

function LocationInfo({ location }: LocationInfoProps) {
  return (
    <Table>
      <TableHead>
        <TableRow sx={{ alignContent: 'right' }}>
          <TableCell>Primary Name</TableCell>
          <TableCell>Secondary Name</TableCell>
          <TableCell>Latitude</TableCell>
          <TableCell>Longitude</TableCell>
          <TableCell>Region</TableCell>
          <TableCell>Jursidiction</TableCell>
          <TableCell align="center">Display On Map</TableCell>
          <TableCell align="center">All Peds are 1:1</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        <TableRow>
          <TableCell>{location.primaryName}</TableCell>
          <TableCell>{location.secondaryName}</TableCell>
          <TableCell>{location.latitude}</TableCell>
          <TableCell>{location.longitude}</TableCell>
          <TableCell>{location.region?.description}</TableCell>
          <TableCell>{location.jurisdiction?.name}</TableCell>
          <TableCell align="center">
            {location.chartEnabled ? (
              <CheckBoxIcon />
            ) : (
              <CheckBoxOutlineBlankIcon />
            )}
          </TableCell>
          <TableCell align="center">
            {location.pedsAre1to1 ? (
              <CheckBoxIcon />
            ) : (
              <CheckBoxOutlineBlankIcon />
            )}
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  )
}

export default LocationInfo
