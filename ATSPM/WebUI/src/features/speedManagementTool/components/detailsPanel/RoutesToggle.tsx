import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import {
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
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
    <>
      <StyledComponentHeader header={'Render Options'} />
      <Box sx={{ p: 2, pt: 0 }}>
        <Grid container spacing={1}>
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
                disabled={
                  submittedRouteSpeedRequest.sourceId !== DataSource.PeMS
                }
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
      </Box>
    </>
  )
}

export default RenderOptionSelector
