import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

import {
  useLocationConfigData,
  useLocationVersions,
} from '@/features/locations/api'
import ApproachesInfo from '@/features/locations/components/ApproachesInfo/approachesInfo'
import DetectorsInfo from '@/features/locations/components/DetectorsInfo/detectorsInfo'
import LocationInfo from '@/features/locations/components/LocationInfo/locationInfo'
import { formatTimestampToDDMMYYYY } from '@/utils/dateTime'

interface LocationsConfigContainerProps {
  locationIdentifier: string
}

function LocationsConfigContainer({
  locationIdentifier,
}: LocationsConfigContainerProps) {
  const [version, setVersion] = useState({ id: '', note: '' })
  const [isLocationInfoExpanded, setIsLocationInfoExpanded] = useState(true)
  const [isApproachesExpanded, setIsApproachesExpanded] = useState(true)
  const [isDetectorsExpanded, setIsDetectorsExpanded] = useState(true)

  const { data: versionData } = useLocationVersions({
    locationId: locationIdentifier,
  })

  useEffect(() => {
    if (versionData) {
      const newestVersion = versionData.reduce((newest, current) => {
        return new Date(newest.start) > new Date(current.start)
          ? newest
          : current
      }, versionData[0])
      setVersion({ id: newestVersion.id, note: newestVersion.note })
    }
  }, [versionData])

  const { data: configData } = useLocationConfigData({
    locationId: version.id,
  })

  const handleChange = (event: SelectChangeEvent) => {
    setVersion({
      id: event.target.value as string,
      note: versionData?.find((v) => v.id === event.target.value)?.note || '',
    })
  }

  if (!configData || !versionData) {
    return (
      <>
        <Typography variant="h4" fontWeight={'bold'} mt={2}>
          Loading...
        </Typography>
        <Box height={'600px'} />
      </>
    )
  }

  const location = configData[0]

  return (
    <Box>
      <Typography variant="h4" fontWeight={'bold'} my={2}>
        Version
      </Typography>
      <Paper sx={{ width: '300px', mb: 2 }}>
        <FormControl fullWidth>
          <InputLabel
            htmlFor={'location-version-select'}
            id={'location-version-select-label'}
          >
            Version
          </InputLabel>
          <Select
            value={version.id}
            label="Version"
            labelId="location-version-select-label"
            id={'location-version-select'}
            inputProps={{ id: 'location-version-select' }}
            onChange={handleChange}
          >
            {versionData.map((version, index) => (
              <MenuItem key={index} value={version.id}>
                {formatTimestampToDDMMYYYY(version.start.replace(/Z$/, ''))} -{' '}
                {version.note}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Paper>
      <Accordion
        expanded={isLocationInfoExpanded}
        onChange={() => setIsLocationInfoExpanded(!isLocationInfoExpanded)}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" fontWeight={'bold'}>
            Location
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <LocationInfo location={location} />
        </AccordionDetails>
      </Accordion>
      <Accordion
        expanded={isApproachesExpanded}
        onChange={() => setIsApproachesExpanded(!isApproachesExpanded)}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" fontWeight={'bold'}>
            Approaches
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <ApproachesInfo location={location} />
        </AccordionDetails>
      </Accordion>
      <Accordion
        expanded={isDetectorsExpanded}
        onChange={() => setIsDetectorsExpanded(!isDetectorsExpanded)}
      >
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" fontWeight={'bold'}>
            Detectors
          </Typography>
        </AccordionSummary>
        <AccordionDetails>
          <DetectorsInfo location={location} />
        </AccordionDetails>
      </Accordion>
    </Box>
  )
}

export default LocationsConfigContainer
