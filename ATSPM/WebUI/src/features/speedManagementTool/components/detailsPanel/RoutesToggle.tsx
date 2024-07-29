import Subtitle from '@/features/speedManagementTool/components/subtitle'
import useStore, {
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/speedManagementStore'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import {
  Box,
  FormControlLabel,
  Grid,
  Radio,
  RadioGroup,
  Tooltip,
  Typography,
} from '@mui/material'
const TooltipContent = ({
  label,
  tooltip,
}: {
  label: string
  tooltip: string
}) => {
  return (
    <Typography variant="body1">
      {label}
      <Tooltip title={tooltip}>
        <Box component="span" sx={{ verticalAlign: 'super' }}>
          <InfoOutlinedIcon fontSize="inherit" />
        </Box>
      </Tooltip>
    </Typography>
  )
}

function RenderOptionSelector() {
  const {
    routeRenderOption,
    setRouteRenderOption,
    submittedRouteSpeedRequest,
  } = useStore()

  const handleOptionClick = (option: RouteRenderOption) => {
    setRouteRenderOption(option)
  }

  return (
    <div style={{ padding: '10px' }}>
      <Box textAlign="center">
        <Subtitle>Display Route Options</Subtitle>
      </Box>
      <Box display="flex" justifyContent="center">
        <Box display="flex" alignItems="center">
          <Typography fontSize="11px" color="primary">
            Current Data Source:
          </Typography>
          <Typography fontSize="11px" color="primary" ml={0.5}>
            {DataSource[submittedRouteSpeedRequest.sourceId]}
          </Typography>
        </Box>
      </Box>
      <Grid container spacing={1} sx={{ mt: 0.5 }}>
        <Grid item xs={12}>
          <RadioGroup
            name="routeRenderOption"
            value={routeRenderOption}
            onChange={(event) =>
              handleOptionClick(event.target.value as RouteRenderOption)
            }
          >
            <Box display={'flex'} alignItems={'center'}>
              <FormControlLabel
                value={RouteRenderOption.Posted_Speed}
                control={<Radio size="small" />}
                label="Posted Speed"
              />
            </Box>
            <FormControlLabel
              value={RouteRenderOption.Average_Speed}
              control={<Radio size="small" />}
              label="Average Speed"
            />
            <FormControlLabel
              value={RouteRenderOption.Percentile_85th}
              control={<Radio size="small" />}
              label={
                <TooltipContent
                  label="85th Percentile"
                  tooltip="Available on ATSPM and PeMS"
                />
              }
              disabled={
                submittedRouteSpeedRequest.sourceId === DataSource.ClearGuide
              }
            />
            <FormControlLabel
              value={RouteRenderOption.Percentile_95th}
              control={<Radio size="small" />}
              label={
                <TooltipContent
                  label="95th Percentile"
                  tooltip="Available on PeMS"
                />
              }
              disabled={submittedRouteSpeedRequest.sourceId !== DataSource.PeMS}
            />
            <FormControlLabel
              value={RouteRenderOption.Violations}
              control={<Radio size="small" />}
              label={
                <TooltipContent
                  label="Estimated Violations per Hour"
                  tooltip="Available on ATSPM and PeMS"
                />
              }
              disabled={
                submittedRouteSpeedRequest.sourceId === DataSource.ClearGuide
              }
            />
          </RadioGroup>
        </Grid>
      </Grid>
    </div>
  )
}

export default RenderOptionSelector
