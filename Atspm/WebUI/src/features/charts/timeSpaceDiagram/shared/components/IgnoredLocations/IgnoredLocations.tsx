import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Typography,
} from '@mui/material'
import { IgnoreLocationsComponent } from './IgnoreLocationsComponent'

interface IgnoreLocationsProp {
  locations: string[]
  ignoredLocations: string[]
  setIgnoredLocations: React.Dispatch<React.SetStateAction<string[]>>
}

export const IgnoreLocationsAccordion = (prop: IgnoreLocationsProp) => {
  return (
    <Accordion defaultExpanded elevation={0}>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography fontWeight={600}>Ignored Locations</Typography>
      </AccordionSummary>

      <AccordionDetails>
        <IgnoreLocationsComponent
          locations={prop.locations}
          ignoredLocations={prop.ignoredLocations}
          setIgnoredLocations={prop.setIgnoredLocations}
        />
      </AccordionDetails>
    </Accordion>
  )
}
