import { useGetAreas } from "@/features/areas/api/areaApi";
import { useGetMeasureTypes } from "@/features/charts/api/getMeasureTypes";
import { useGetJurisdiction } from "@/features/jurisdictions/api/jurisdictionApi";
import { useLocationTypes } from "@/features/locations/api/getLocationTypes";
import { Filters } from "@/features/locations/components/selectLocation";
import { useGetRegion } from "@/features/region/api/regionApi";
import { Autocomplete, Box, Paper, TextField, Typography } from "@mui/material";
import { SyntheticEvent, memo } from "react";

type Filters = {
  areaId: number | null;
  regionId: number | null;
  locationTypeId: number | null;
  jurisdictionId: number | null;
  measureTypeId: number | null;
};

type MapFiltersProps = {
  onFilterChange: (filters: Partial<Filters>) => void;
  filters: Filters;
  locationsTotal: number;
  locationsFiltered: number;
};

const MapFilters = ({
  onFilterChange,
  filters,
  locationsTotal,
  locationsFiltered,
}: MapFiltersProps) => {
  const { data: areasData } = useGetAreas();
  const { data: regionsData } = useGetRegion();
  const { data: jurisdictionsData } = useGetJurisdiction();
  const { data: measureTypeData } = useGetMeasureTypes();
  const { data: locationTypeData } = useLocationTypes();
  const areas = areasData?.value;
  const regions = regionsData?.value;
  const locationTypes = locationTypeData?.value;
  const jurisdictions = jurisdictionsData?.value;
  const measureTypes = measureTypeData?.value;

  const handleFilterChange = (key: keyof Filters, value: number | null) => {
    onFilterChange({ [key]: value });
  };

  return (
    <Paper
      sx={{
        display: "flex",
        flexDirection: "column",
        gap: 2,
        padding: 2,
        width: "200px",
      }}
    >
      <Autocomplete
        size="small"
        value={areas?.find((area) => area.id === filters.areaId)?.name || null}
        options={areas?.map((area) => area.name) || []}
        renderInput={(params) => <TextField {...params} label="Area" />}
        onChange={(_: SyntheticEvent, val: string | null) => {
          const id = areas?.find((area) => area.name === val)?.id || null;
          handleFilterChange("areaId", id);
        }}
      />
      <Autocomplete
        size="small"
        value={
          regions?.find((region) => region.id === filters.regionId)
            ?.description || null
        }
        options={regions?.map((region) => region.description) || []}
        renderInput={(params) => (
          <TextField {...params} label="Region/District" />
        )}
        onChange={(_: SyntheticEvent, val: string | null) => {
          const id =
            regions?.find((region) => region.description === val)?.id || null;
          handleFilterChange("regionId", id);
        }}
      />
      <Autocomplete
        size="small"
        value={
          jurisdictions?.find(
            (jurisdiction) => jurisdiction.id === filters.jurisdictionId
          )?.name || null
        }
        options={jurisdictions?.map((jurisdiction) => jurisdiction.name) || []}
        renderInput={(params) => <TextField {...params} label="Jurisdiction" />}
        onChange={(_: SyntheticEvent, val: string | null) => {
          const id =
            jurisdictions?.find((jurisdiction) => jurisdiction.name === val)
              ?.id || null;
          handleFilterChange("jurisdictionId", id);
        }}
      />
      <Autocomplete
        size="small"
        value={
          measureTypes?.find(
            (measureType) => measureType.id === filters.measureTypeId
          )?.name || null
        }
        options={
          measureTypes
            ?.filter((measureType) => measureType.showOnWebsite)
            .map((measureType) => measureType.name) || []
        }
        renderInput={(params) => <TextField {...params} label="Measure Type" />}
        onChange={(_: SyntheticEvent, val: string | null) => {
          const id =
            measureTypes?.find((measureType) => measureType.name === val)?.id ||
            null;
          handleFilterChange("measureTypeId", id);
        }}
      />
      <Autocomplete
        size="small"
        value={
          locationTypes?.find(
            (locationType) => locationType.id === filters.locationTypeId
          )?.name || null
        }
        options={locationTypes?.map((locationType) => locationType.name) || []}
        renderInput={(params) => (
          <TextField {...params} label="Location Type" />
        )}
        onChange={(_: SyntheticEvent, val: string | null) => {
          const id =
            locationTypes?.find((locationType) => locationType.name === val)
              ?.id || null;
          handleFilterChange("locationTypeId", id);
        }}
      />
      <Box display={"flex"} justifyContent={"space-between"}>
        <Typography variant="caption">Results</Typography>
        <Typography variant="caption">
          {locationsFiltered} / {locationsTotal}
        </Typography>
      </Box>
    </Paper>
  );
};

export default memo(MapFilters);
