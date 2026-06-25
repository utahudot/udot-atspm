import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import {
  DataSource,
  RouteRenderOption,
  isViolationRenderOption,
} from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import {
  Box,
  Divider,
  FormControlLabel,
  Grid,
  Radio,
  RadioGroup,
  Tooltip,
  Typography,
} from '@mui/material'
import ViolationRangeSlider from './ViolationRangeSlider'

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

function RenderOptionSelector({
  setAnchorEl,
}: {
  setAnchorEl: (element: HTMLButtonElement | null) => void
}) {
  const {
    routeRenderOption,
    setRouteRenderOption,
    submittedRouteSpeedRequest,
  } = useStore()

  const handleOptionClick = (option: RouteRenderOption) => {
    setRouteRenderOption(option)
    if (!isViolationRenderOption(option)) {
      setTimeout(() => setAnchorEl(null), 700)
    }
  }

  return (
    <>
      <StyledComponentHeader header={'Segment Display Options'} />
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
              <Divider sx={{ my: 1 }} />
              <FormControlLabel
                value={RouteRenderOption.Percent_Violations}
                control={<Radio size="small" />}
                label={
                  <TooltipContent
                    label="% Violations"
                    tooltip="Percent of volume traveling 2 mph or more over the speed limit"
                  />
                }
              />
              <FormControlLabel
                value={RouteRenderOption.Percent_Extreme_Violations}
                control={<Radio size="small" />}
                label={
                  <TooltipContent
                    label="% Extreme Violations"
                    tooltip="Percent of volume traveling 10 mph or more over the speed limit"
                  />
                }
              />
              <ViolationRangeSlider />
              {/* <FormControlLabel
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
              /> */}
            </RadioGroup>
          </Grid>
        </Grid>
      </Box>
    </>
  )
}

export default RenderOptionSelector
