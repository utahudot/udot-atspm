import { useGetAreas } from '@/features/areas/api/areaApi'
import areaDto from '@/features/areas/types/areaDto'
import { useGetJurisdiction } from '@/features/jurisdictions/api/jurisdictionApi'
import jurisdictionDto from '@/features/jurisdictions/types/jurisdictionDto'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import SelectLocationNoMap from '@/features/locations/components/selectLocation/SelectLocationNoMap'
import { Location } from '@/features/locations/types/Location'
import { useGetRegion } from '@/features/region/api/regionApi'
import { regionDto } from '@/features/region/types/regionDto'
import { IssueTypeSelect } from '@/features/watchdog/components/issueTypeSelect'
import { Autocomplete, Box, TextField } from '@mui/material'
import { useEffect, useState } from 'react'

interface OptionalWatchDogFiltersProps {
  issueType: string
  setSelectedIssueType: (issueType: string) => void
  setAreaId: (areaId: number | null) => void
  setRegionId: (regionId: number | null) => void
  setJurisdictionId: (jurisdictionId: number | null) => void
}

export const OptionalWatchDogFilters = ({
  issueType,
  setSelectedIssueType,
  setAreaId,
  setRegionId,
  setJurisdictionId,
}: OptionalWatchDogFiltersProps) => {
  const [areas, setAreas] = useState<any[]>([])
  const [regions, setRegions] = useState<any[]>([])
  const [jurisdictions, setJurisdictions] = useState<any[]>([])
  const { data: areasData } = useGetAreas()
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionsData } = useGetJurisdiction()
  const { data } = useLatestVersionOfAllLocations()
  const [location, setLocation] = useState<Location | null>(null)
  const [locations, setLocations] = useState<Location[]>([])

  useEffect(() => {
    if (data) {
      setLocations(data.value)
    }
  }, [data])

  useEffect(() => {
    if (areasData?.value) {
      setAreas(areasData?.value)
    }
  }, [areasData?.value])

  useEffect(() => {
    if (regionsData?.value) {
      setRegions(regionsData?.value)
    }
  }, [regionsData?.value])

  useEffect(() => {
    if (jurisdictionsData?.value) {
      setJurisdictions(jurisdictionsData?.value)
    }
  }, [jurisdictionsData?.value])

  const handleRegionChange = (event: any, val: any) => {
    if (val) {
      const id = regions?.find((r: Region) => r.description === val)?.id
      if (id) {
        setRegionId(id)
      } else {
        setRegionId(null)
      }
    } else {
      setRegionId(null)
    }
  }

  const handleJurisdictionChange = (event: any, val: any) => {
    if (val) {
      const id = jurisdictions?.find((j: Jurisdiction) => j.name === val)?.id
      if (id) {
        setJurisdictionId(id)
      } else {
        setJurisdictionId(null)
      }
    } else {
      setJurisdictionId(null)
    }
  }

  const handleAreaChange = (event: any, val: any) => {
    if (val) {
      const id = areas?.find((a: Area) => a.name === val)?.id
      if (id) {
        setAreaId(id)
      } else {
        setAreaId(null)
      }
    } else {
      setAreaId(null)
    }
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Autocomplete
        sx={{
          width: '100%',
          boxSizing: 'border-box',
        }}
        options={areas?.map((area: areaDto) => area.name) || []}
        renderInput={(params) => <TextField {...params} label="Area" />}
        onChange={handleAreaChange}
      />
      <Autocomplete
        sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        options={regions?.map((region: regionDto) => region.description) || []}
        renderInput={(params) => <TextField {...params} label="Region" />}
        onChange={handleRegionChange}
      />
      <Autocomplete
        sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        options={
          jurisdictions?.map(
            (jurisdiction: jurisdictionDto) => jurisdiction.name
          ) || []
        }
        renderInput={(params) => <TextField {...params} label="Jurisdiction" />}
        onChange={handleJurisdictionChange}
      />
      <SelectLocationNoMap
        location={location}
        setLocation={setLocation}
        locations={locations}
      />
      <Box
        sx={{
          marginTop: '-25px',
        }}
      >
        <IssueTypeSelect
          issueTypeData={issueType}
          setSelectedIssueTypeData={setSelectedIssueType}
        />
      </Box>
    </Box>
  )
}
