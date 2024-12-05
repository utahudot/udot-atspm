import { useGetAreas } from '@/features/areas/api/areaApi'
import { useGetJurisdiction } from '@/features/jurisdictions/api/jurisdictionApi'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import SelectLocationNoMap from '@/features/locations/components/selectLocation/SelectLocationNoMap'
import { Location } from '@/features/locations/types/Location'
import { useGetRegion } from '@/features/region/api/regionApi'
import { IssueTypeSelect } from '@/features/watchdog/components/issueTypeSelect'
import { Autocomplete, Box, TextField } from '@mui/material'
import { SyntheticEvent, useState } from 'react'

interface OptionalWatchDogFiltersProps {
  issueType: Record<string, string> | null
  setSelectedIssueType: (issueType: number) => void
  setAreaId: (areaId: number | null) => void
  setRegionId: (regionId: number | null) => void
  setJurisdictionId: (jurisdictionId: number | null) => void
  setLocationIdentifier: (locationIdentifier: string | null) => void
}

const OptionalWatchDogFilters = ({
  issueType,
  setSelectedIssueType,
  setAreaId,
  setRegionId,
  setJurisdictionId,
  setLocationIdentifier,
}: OptionalWatchDogFiltersProps) => {
  const [location, setLocation] = useState<Location | null>(null)

  const { data: areasData } = useGetAreas()
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionsData } = useGetJurisdiction()
  const { data: locationsData } = useLatestVersionOfAllLocations()

  const areas = areasData?.value || []
  const regions = regionsData?.value || []
  const jurisdictions = jurisdictionsData?.value || []
  const locations = locationsData?.value || []

  const handleAreaChange = (_: SyntheticEvent, val: string | null) => {
    const area = areas.find((a) => a.name === val)
    setAreaId(area ? area.id : null)
  }

  const handleRegionChange = (_: SyntheticEvent, val: string | null) => {
    const region = regions.find((r) => r.description === val)
    setRegionId(region ? region.id : null)
  }

  const handleJurisdictionChange = (_: SyntheticEvent, val: string | null) => {
    const jurisdiction = jurisdictions.find((j) => j.name === val)
    setJurisdictionId(jurisdiction ? jurisdiction.id : null)
  }

  const handleLocationChange = (location: Location | null) => {
    setLocation(location)
    setLocationIdentifier(location ? location.locationIdentifier : null)
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Autocomplete
        sx={{ width: '100%' }}
        options={areas.map((area) => area.name)}
        renderInput={(params) => <TextField {...params} label="Area" />}
        onChange={handleAreaChange}
      />
      <Autocomplete
        sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        options={regions.map((region) => region.description)}
        renderInput={(params) => <TextField {...params} label="Region" />}
        onChange={handleRegionChange}
      />
      <Autocomplete
        sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        options={jurisdictions.map((jurisdiction) => jurisdiction.name)}
        renderInput={(params) => <TextField {...params} label="Jurisdiction" />}
        onChange={handleJurisdictionChange}
      />
      <SelectLocationNoMap
        location={location}
        setLocation={handleLocationChange}
        locations={locations}
      />
      <Box sx={{ marginTop: '-25px' }}>
        <IssueTypeSelect
          issueTypeData={issueType}
          setSelectedIssueTypeData={setSelectedIssueType}
        />
      </Box>
    </Box>
  )
}

export default OptionalWatchDogFilters
