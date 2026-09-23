import { ReportManagementType } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
  useTheme,
} from '@mui/material'
import { DataSource } from '../../enums'
import { ERBaseHandler } from '../ExportableReports/components/handlers/ExportableReportsHandler'

interface Props {
  handler: ERBaseHandler
}

export default function SourceSelectOptions(props: Props) {
  const { handler } = props
  const theme = useTheme()
  const selectedSourceId = handler.sourceId[0]
  const isLeadershipManagement =
    handler.managementType === ReportManagementType.Leadership

  return (
    <>
      <Box sx={{ mt: 1, display: 'flex', gap: 1 }}>
        <FormControl fullWidth size="medium">
          <InputLabel>Management Type</InputLabel>
          <Select
            label="Management Type"
            value={handler.managementType}
            onChange={(event) => {
              const managementType = event.target.value as ReportManagementType
              handler.updateManagementType(managementType)

              if (
                managementType === ReportManagementType.Leadership &&
                selectedSourceId === DataSource.ClearGuide
              ) {
                handler.updateSourceId([DataSource.ATSPM])
              }
            }}
          >
            <MenuItem value={ReportManagementType.Engineer}>Engineer</MenuItem>
            <MenuItem value={ReportManagementType.Leadership}>
              Leadership
            </MenuItem>
          </Select>
        </FormControl>

        {/* <FormControl fullWidth size="medium">
          <InputLabel>Granularity</InputLabel>
          <Select
            label="Granularity"
            value={handler.speedDataType}
            onChange={(event) =>
              handler.updateSpeedDataType(event.target.value as SpeedDataType)
            }
          >
            <MenuItem value={SpeedDataType.H}>Hourly</MenuItem>
            <MenuItem value={SpeedDataType.M}>Monthly</MenuItem>
          </Select>
        </FormControl> */}
      </Box>
      <Box sx={{ mt: 2, mb: 2 }}>
        <Typography
          fontSize="12px"
          fontWeight="light"
          color={theme.palette.grey[600]}
        >
          Source
        </Typography>
        <Box display="flex">
          <ToggleButtonGroup
            sx={{ width: '100%' }}
            value={selectedSourceId}
            size="small"
            exclusive
            onChange={(_, value: DataSource) => {
              if (
                value !== null &&
                !(
                  isLeadershipManagement &&
                  value === DataSource.ClearGuide
                )
              ) {
                handler.updateSourceId([value])
              }
            }}
          >
            <ToggleButton
              value={DataSource.ClearGuide}
              disabled={isLeadershipManagement}
              sx={{ textTransform: 'none', flex: 1 }}
            >
              ClearGuide
            </ToggleButton>
            <ToggleButton
              value={DataSource.ATSPM}
              sx={{ textTransform: 'none', flex: 1 }}
            >
              ATSPM
            </ToggleButton>
            <ToggleButton
              value={DataSource.PeMS}
              sx={{ textTransform: 'none', flex: 1 }}
            >
              PeMS
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>
      </Box>
    </>
  )
}
