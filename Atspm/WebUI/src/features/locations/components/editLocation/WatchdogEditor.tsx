import { LocationConfigHandler } from '@/features/locations/components/editLocation/editLocationConfigHandler'
import { Box, FormControlLabel, Paper, Switch, Typography } from '@mui/material'

interface WatchdogEditorProps {
  handler: LocationConfigHandler
}

const watchdogOptions = [
  {
    label: 'Record Count',
    description:
      'Report phases with record counts below a configured threshold over the previous day.',
  },
  {
    label: 'Force Off Thresholds',
    description:
      'Report phases where the percentage of force offs exceeds a configured threshold within a specified number of activations during certain early morning hours.',
  },
  {
    label: 'Max Out Thresholds',
    description:
      'Report signals where the percentage of max outs exceeds a configured threshold within a specified number of activations during certain early morning hours.',
  },
  {
    label: 'Low Detector Counts',
    description:
      'Report phases with vehicle detector counts below a configured threshold during the previous day’s peak time period.',
  },
  {
    label: 'Stuck Ped',
    description:
      'Report phases with pedestrian activations exceeding a configured threshold during certain early morning hours.',
  },
  {
    label: 'Unconfigured Approach',
    description:
      'Identifies and processes incoming event data from controllers that lack a corresponding configuration for the specified approach.',
  },
  {
    label: 'Unconfigured Detector',
    description:
      'Identifies and processes incoming event data from controllers that lack a corresponding configuration for the specified detector.',
  },
]

const WatchdogEditor = ({ handler }: WatchdogEditorProps) => {
  const handleWatchdogSwitchChange =
    (index: number) => (event: React.ChangeEvent<HTMLInputElement>) => {
      handler.handleLocationEdit(`watchdog_${index}`, event.target.checked)
    }

  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h4" fontWeight="bold" component="p" sx={{ mb: 2 }}>
        Watchdog Options
      </Typography>
      {watchdogOptions.map((option, index) => (
        <Box
          key={index}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            mb: 2,
          }}
        >
          <Box sx={{ flex: 1 }}>
            <Typography variant="subtitle1" fontWeight="bold">
              {option.label}
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary' }}>
              {option.description}
            </Typography>
          </Box>
          <FormControlLabel
            control={
              <Switch
                checked={!!handler.expandedLocation[`watchdog_${index}`]}
                onChange={handleWatchdogSwitchChange(index)}
              />
            }
            label=""
            sx={{ margin: 0 }}
          />
        </Box>
      ))}
    </Paper>
  )
}

export default WatchdogEditor
