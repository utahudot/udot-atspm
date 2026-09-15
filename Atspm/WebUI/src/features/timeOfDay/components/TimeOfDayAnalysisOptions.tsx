import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import { useSidebarStore } from '@/stores/sidebar'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import {
  Box,
  Button,
  Divider,
  ListItemButton,
  Paper,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import type { TimeOfDaySchedulePreset } from '../measureDefaults'
import type { TimeOfDayDataSourceOption, TimeOfDayFormState } from '../types'
import { timeOfDayDataSourceLabels } from '../types'
import TimeOfDayAdvancedSidebar, {
  type AnalysisSidebar,
} from './TimeOfDayAdvancedSidebar'
import TimeOfDayDirectionSelector from './TimeOfDayDirectionSelector'
import TimeOfDaySchedulePresetSelect from './TimeOfDaySchedulePresetSelect'

interface TimeOfDayAnalysisOptionsProps {
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
  schedulePresets?: TimeOfDaySchedulePreset[]
}

const advancedSettings = [
  { sidebar: 'occupancy', label: 'Occupancy and Review' },
] satisfies { sidebar: AnalysisSidebar; label: string }[]

export default function TimeOfDayAnalysisOptions({
  options,
  onChange,
  schedulePresets = [],
}: TimeOfDayAnalysisOptionsProps) {
  const { openRightSidebar, closeRightSidebar } = useSidebarStore()
  const [activeSidebar, setActiveSidebar] =
    useState<AnalysisSidebar>('schedule')

  useEffect(
    () => () => {
      closeRightSidebar()
    },
    [closeRightSidebar]
  )

  const openAnalysisSidebar = (sidebar: AnalysisSidebar) => {
    setActiveSidebar(sidebar)
    openRightSidebar()
  }

  return (
    <>
      <Paper
        sx={{
          width: { xs: '100%', sm: 360 },
          display: 'flex',
          flexDirection: 'column',
          alignSelf: 'stretch',
        }}
      >
        <StyledComponentHeader header="Analysis Parameters" />
        <Box sx={{ p: 2, pt: 1.5 }}>
          <Stack spacing={2}>
            <Box component="section">
              <Divider sx={{ mb: 1.5 }}>
                <Typography variant="caption">Data Source</Typography>
              </Divider>
              <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                <ToggleButtonGroup
                  exclusive
                  color="primary"
                  size="small"
                  value={options.dataSource}
                  onChange={(
                    _,
                    dataSource: TimeOfDayDataSourceOption | null
                  ) => {
                    if (dataSource) {
                      onChange({ ...options, dataSource })
                    }
                  }}
                  sx={{
                    '& .MuiToggleButton-root': {
                      py: 0.5,
                      px: 1.5,
                      textTransform: 'none',
                      fontWeight: 500,
                    },
                  }}
                >
                  {Object.entries(timeOfDayDataSourceLabels).map(
                    ([value, label]) => (
                      <ToggleButton key={value} value={value}>
                        {label}
                      </ToggleButton>
                    )
                  )}
                </ToggleButtonGroup>
              </Box>
            </Box>

            <Box component="section">
              <Divider sx={{ mb: 1.5 }}>
                <Typography variant="caption">Primary Directions</Typography>
              </Divider>
              <TimeOfDayDirectionSelector
                options={options}
                onChange={onChange}
              />
            </Box>

            <Box component={'section'}>
              <Divider sx={{ mb: 1.5 }}>
                <Typography variant={'caption'}>
                  Roadway Type Thresholds
                </Typography>
              </Divider>
              <Stack direction="row" spacing={1} alignItems="flex-start">
                <TimeOfDaySchedulePresetSelect
                  options={options}
                  onChange={onChange}
                  presets={schedulePresets}
                />
                <Button
                  variant="outlined"
                  endIcon={<ChevronRightIcon />}
                  onClick={() => openAnalysisSidebar('schedule')}
                  aria-label="Customize roadway type thresholds"
                  sx={{
                    height: 40,
                    minWidth: 88,
                    px: 1.5,
                    textTransform: 'none',
                    fontWeight: 600,
                  }}
                >
                  Customize
                </Button>
              </Stack>
            </Box>

            <Box component="section">
              <Divider sx={{ mb: 1.5 }}>
                <Typography variant="caption">Advanced Settings</Typography>
              </Divider>
              <Stack spacing={1}>
                {advancedSettings.map(({ sidebar, label }) => (
                  <ListItemButton
                    key={sidebar}
                    onClick={() => openAnalysisSidebar(sidebar)}
                    sx={{
                      px: 1.25,
                      py: 1.25,
                      borderRadius: 1,
                      bgcolor: 'grey.100',
                      '&:hover': {
                        bgcolor: 'grey.200',
                      },
                    }}
                  >
                    <Typography variant="subtitle2" sx={{ flex: 1 }}>
                      {label}
                    </Typography>
                    <ChevronRightIcon color="action" />
                  </ListItemButton>
                ))}
              </Stack>
            </Box>
          </Stack>
        </Box>
      </Paper>

      <TimeOfDayAdvancedSidebar
        activeSidebar={activeSidebar}
        options={options}
        onChange={onChange}
        schedulePresets={schedulePresets}
      />
    </>
  )
}
