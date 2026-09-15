import type { TimeOfDayResult } from '@/api/reports'
import { Alert, Box, Chip, Paper, Stack, Typography } from '@mui/material'
import { useMemo } from 'react'
import TimeOfDayScheduleComparison from './schedules/TimeOfDayScheduleComparison'
import {
  buildTimeOfDaySchedulesModel,
  getScheduleColorMap,
} from './schedules/timeOfDayScheduleModel'

export {
  buildTimeOfDaySchedulesModel,
  type TimeOfDaySchedulesModel,
} from './schedules/timeOfDayScheduleModel'

export default function TimeOfDaySchedules({
  result,
}: {
  result: TimeOfDayResult
}) {
  const model = useMemo(() => buildTimeOfDaySchedulesModel(result), [result])
  const colorMap = useMemo(
    () =>
      getScheduleColorMap([
        model.proposedSchedule,
        model.commonSchedule,
        ...model.exceptions.map((exception) => exception.schedule),
      ]),
    [model]
  )
  const hasScheduleData =
    model.proposedSchedule.length > 0 ||
    model.commonSchedule.length > 0 ||
    model.exceptions.length > 0 ||
    model.unavailableLocations.length > 0
  const unavailableRecommendationReason =
    model.proposedSchedule.length === 0
      ? result.recommendation?.summaryText?.trim()
      : undefined

  return (
    <Paper sx={{ p: 0, bgcolor: 'common.white' }}>
      <Stack spacing={0}>
        <Box sx={{ px: { xs: 2, sm: 3 }, pt: { xs: 2, sm: 3 }, pb: 0 }}>
          <Typography variant="h5" component="h2">
            Schedules
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Proposed and existing plan schedules across the selected locations.
            Dashed guides mark the proposed schedule&apos;s time boundaries on
            every existing row.
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75, mt: 1.5 }}>
            <Chip
              size="small"
              label={`${model.totalLocations} selected ${
                model.totalLocations === 1 ? 'location' : 'locations'
              }`}
              sx={{
                bgcolor: '#F3F4F6',
                border: '1px solid',
                borderColor: '#E5E7EB',
              }}
            />
            <Chip
              size="small"
              variant="outlined"
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.7 }}>
                  <Box
                    aria-hidden
                    sx={{
                      width: 6,
                      height: 6,
                      borderRadius: '50%',
                      bgcolor: '#F59E0B',
                    }}
                  />
                  <Box component="span">
                    {model.exceptions.length} schedule{' '}
                    {model.exceptions.length === 1 ? 'exception' : 'exceptions'}
                  </Box>
                </Box>
              }
              sx={{
                color: '#B45309',
                borderColor: '#FCD59A',
                bgcolor: '#FFF9ED',
              }}
            />
          </Box>
        </Box>

        {unavailableRecommendationReason && (
          <Alert severity="warning" sx={{ mx: { xs: 2, sm: 3 }, mt: 2 }}>
            {unavailableRecommendationReason}
          </Alert>
        )}

        {!hasScheduleData ? (
          <Alert severity="warning" sx={{ mx: { xs: 2, sm: 3 }, mb: 3 }}>
            No current schedule data available.
          </Alert>
        ) : (
          <Box sx={{ px: { xs: 2, sm: 3 }, pb: 2 }}>
            <TimeOfDayScheduleComparison
              proposedSchedule={model.proposedSchedule}
              commonSchedule={model.commonSchedule}
              commonLocations={model.commonLocations}
              exceptions={model.exceptions}
              unavailableLocations={model.unavailableLocations}
              colorMap={colorMap}
            />
          </Box>
        )}
      </Stack>
    </Paper>
  )
}
