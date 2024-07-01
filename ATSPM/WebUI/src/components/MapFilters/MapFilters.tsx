import { useGetAreas } from '@/features/areas/api/areaApi'
import { useGetMeasureTypes } from '@/features/charts/api/getMeasureTypes'
import { useGetJurisdiction } from '@/features/jurisdictions/api/jurisdictionApi'
import { useLocationTypes } from '@/features/locations/api/getLocationTypes'
import { useGetRegion } from '@/features/region/api/regionApi'
import { Autocomplete, Box, Paper, TextField, Typography } from '@mui/material'
import { SyntheticEvent, memo } from 'react'

type MapFiltersProps = {
  setSelectedAreaId: (id: number | null) => void
  setSelectedRegionId: (id: number | null) => void
  setSelectedLocationTypeId: (id: number | null) => void
  setSelectedJurisdictionId: (id: number | null) => void
  setSelectedMeasureTypeId: (id: number | null) => void
  selectedAreaId: number | null
  selectedRegionId: number | null
  selectedLocationTypeId: number | null
  selectedJurisdictionId: number | null
  selectedMeasureTypeId: number | null
  locationsTotal: number
  locationsFiltered: number
}

const MapFilters = ({
  setSelectedAreaId,
  setSelectedRegionId,
  setSelectedLocationTypeId,
  setSelectedJurisdictionId,
  setSelectedMeasureTypeId,
  selectedAreaId,
  selectedRegionId,
  selectedLocationTypeId,
  selectedJurisdictionId,
  selectedMeasureTypeId,
  locationsTotal,
  locationsFiltered,
}: MapFiltersProps) => {
  const { data: areasData } = useGetAreas()
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionsData } = useGetJurisdiction()
  const { data: measureTypeData } = useGetMeasureTypes()
  const { data: locationTypeData } = useLocationTypes()
  const areas = areasData?.value
  const regions = regionsData?.value
  const locationTypes = locationTypeData?.value
  const jurisdictions = jurisdictionsData?.value
  const measureTypes = measureTypeData?.value

  const handleRegionChange = (_: SyntheticEvent, val: string | null) => {
    const id = regions?.find((region) => region.description === val)?.id
    setSelectedRegionId(id || null)
  }

  const handleLocationTypeChange = (_: SyntheticEvent, val: string | null) => {
    const id = locationTypes?.find((locationType) => locationType.name === val)
      ?.id
    setSelectedLocationTypeId(id || null)
  }

  const handleJurisdictionChange = (_: SyntheticEvent, val: string | null) => {
    const id = jurisdictions?.find((jurisdiction) => jurisdiction.name === val)
      ?.id
    setSelectedJurisdictionId(id || null)
  }

  const handleAreaChange = (_: SyntheticEvent, val: string | null) => {
    const id = areas?.find((area) => area.name === val)?.id
    setSelectedAreaId(id || null)
  }
  const handleMeasureTypeChange = (_: SyntheticEvent, val: string | null) => {
    const id = measureTypes?.find((measureType) => measureType.name === val)?.id
    setSelectedMeasureTypeId(id || null)
  }

  return (
    <Paper
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        padding: 2,
        width: '200px',
      }}
    >
      <Autocomplete
        size="small"
        value={areas?.find((area) => area.id === selectedAreaId)?.name || null}
        options={areas?.map((area) => area.name) || []}
        renderInput={(params) => <TextField {...params} label="Area" />}
        onChange={handleAreaChange}
      />
      <Autocomplete
        size="small"
        value={
          regions?.find((region) => region.id === selectedRegionId)
            ?.description || null
        }
        options={regions?.map((region) => region.description) || []}
        renderInput={(params) => (
          <TextField {...params} label="Region/District" />
        )}
        onChange={handleRegionChange}
      />
      <Autocomplete
        size="small"
        value={
          jurisdictions?.find(
            (jurisdiction) => jurisdiction.id === selectedJurisdictionId
          )?.name || null
        }
        options={jurisdictions?.map((jurisdiction) => jurisdiction.name) || []}
        renderInput={(params) => <TextField {...params} label="Jurisdiction" />}
        onChange={handleJurisdictionChange}
      />
      <Autocomplete
        size="small"
        value={
          measureTypes?.find(
            (measureType) => measureType.id === selectedMeasureTypeId
          )?.name || null
        }
        options={
          measureTypes
            ?.filter((measureType) => measureType.showOnWebsite)
            .map((measureType) => measureType.name) || []
        }
        renderInput={(params) => <TextField {...params} label="Measure Type" />}
        onChange={handleMeasureTypeChange}
      />
      <Autocomplete
        size="small"
        value={
          locationTypes?.find(
            (locationType) => locationType.id === selectedLocationTypeId
          )?.name || null
        }
        options={locationTypes?.map((locationType) => locationType.name) || []}
        renderInput={(params) => (
          <TextField {...params} label="Location Type" />
        )}
        onChange={handleLocationTypeChange}
      />
      <Box display={'flex'} justifyContent={'space-between'}>
        <Typography variant="caption">Results</Typography>
        <Typography variant="caption">
          {locationsFiltered} / {locationsTotal}
        </Typography>
      </Box>
    </Paper>
  )
}

export default memo(MapFilters)
