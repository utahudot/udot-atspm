import type { Plan } from '@/api/reports'
import { Alert, Box, Stack, Tooltip, Typography } from '@mui/material'
import { alpha } from '@mui/material/styles'
import type { ReactNode } from 'react'
import { formatPlanNumber } from '../../transformers'
import type {
  ScheduleException,
  ScheduleLocation,
} from './timeOfDayScheduleModel'
import {
  formatClockTime,
  freePlanColor,
  getScheduleIntervals,
  getScheduleSummary,
} from './timeOfDayScheduleModel'

interface ScheduleComparisonProps {
  proposedSchedule: Plan[]
  commonSchedule: Plan[]
  commonLocations: ScheduleLocation[]
  exceptions: ScheduleException[]
  unavailableLocations: ScheduleLocation[]
  colorMap: Map<string, string>
}

const categoryColumnWidth = 280
const labelColumnWidth = 248
const minimumTimelineWidth = 1200
const minimumComparisonWidth =
  categoryColumnWidth + labelColumnWidth + minimumTimelineWidth
const timeTicks = Array.from({ length: 25 }, (_, hour) => ({
  minutes: hour * 60,
  label: `${String(hour).padStart(2, '0')}:00`,
}))

const formatScheduleLocation = ({
  identifier,
  description,
}: ScheduleLocation) => {
  const locationDescription = description
    ?.replace(new RegExp(`^#?${identifier}\\s*[-–—:]\\s*`), '')
    .trim()

  return locationDescription
    ? `#${identifier} — ${locationDescription}`
    : `#${identifier}`
}

function ScheduleRail({
  schedule,
  colorMap,
  ariaLabel,
  proposedBoundaryMinutes = [],
  emptyLabel,
}: {
  schedule: Plan[]
  colorMap: Map<string, string>
  ariaLabel: string
  proposedBoundaryMinutes?: number[]
  emptyLabel?: string
}) {
  const intervals = getScheduleIntervals(schedule)

  return (
    <Box
      role="img"
      aria-label={`${ariaLabel}: ${emptyLabel ?? getScheduleSummary(schedule)}`}
      sx={{
        position: 'relative',
        flex: 1,
        alignSelf: 'stretch',
        minHeight: 40,
        overflow: 'hidden',
        bgcolor: '#F1F5F9',
        '&::after': {
          position: 'absolute',
          zIndex: 2,
          inset: 0,
          content: '""',
          backgroundImage:
            'repeating-linear-gradient(to right, transparent 0, transparent calc(4.1666667% - 1px), rgba(100, 116, 139, 0.16) calc(4.1666667% - 1px), rgba(100, 116, 139, 0.16) 4.1666667%)',
          pointerEvents: 'none',
        },
      }}
    >
      {intervals.map(({ plan, startMinutes, endMinutes }, index) => {
        const planName = formatPlanNumber(plan.planNumber)
        const isNumberedPlan = /^\d+$/.test(planName)
        const color = colorMap.get(planName) ?? freePlanColor
        const intervalLabel = `${planName}, ${formatClockTime(
          startMinutes
        )} to ${formatClockTime(endMinutes)}${
          plan.planDescription ? `, ${plan.planDescription}` : ''
        }`

        return (
          <Tooltip
            key={`${planName}-${startMinutes}-${endMinutes}-${index}`}
            title={intervalLabel}
          >
            <Box
              component="span"
              aria-hidden
              sx={{
                position: 'absolute',
                insetBlock: 0,
                left: `${(startMinutes / 1440) * 100}%`,
                width: `${((endMinutes - startMinutes) / 1440) * 100}%`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                minWidth: 2,
                overflow: 'hidden',
                bgcolor: alpha(color, planName === 'FREE' ? 0.14 : 0.2),
                color,
                fontSize: '0.7rem',
                lineHeight: 1,
              }}
            >
              <Box
                component="span"
                data-plan-shape={isNumberedPlan ? 'circle' : 'pill'}
                sx={{
                  position: 'relative',
                  zIndex: 4,
                  display: 'inline-flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  width: isNumberedPlan ? 20 : 'auto',
                  height: isNumberedPlan ? 20 : 'auto',
                  px: isNumberedPlan ? 0 : 0.8,
                  py: isNumberedPlan ? 0 : 0.3,
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                  borderRadius: isNumberedPlan ? '50%' : 0.75,
                  bgcolor: color,
                  color: 'common.white',
                  fontSize: isNumberedPlan ? '0.65rem' : 'inherit',
                }}
              >
                {planName}
              </Box>
            </Box>
          </Tooltip>
        )
      })}
      {emptyLabel && intervals.length === 0 && (
        <Typography
          component="span"
          variant="caption"
          sx={{
            position: 'absolute',
            zIndex: 4,
            inset: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'text.secondary',
            fontStyle: 'italic',
          }}
        >
          {emptyLabel}
        </Typography>
      )}
      {proposedBoundaryMinutes.map((minutes) => (
        <Box
          key={minutes}
          data-testid="proposed-boundary-guide"
          aria-hidden
          sx={{
            position: 'absolute',
            insetBlock: 0,
            left: `${(minutes / 1440) * 100}%`,
            borderLeft: '2px dashed',
            borderColor: 'rgba(71, 85, 105, 0.72)',
            pointerEvents: 'none',
            zIndex: 3,
          }}
        />
      ))}
    </Box>
  )
}

function ScheduleRow({
  label,
  schedule,
  colorMap,
  ariaLabel,
  proposedBoundaryMinutes,
  emptyLabel,
  minHeight = 41,
  labelWidth = labelColumnWidth,
  fillHeight = false,
}: {
  label: ReactNode
  schedule: Plan[]
  colorMap: Map<string, string>
  ariaLabel: string
  proposedBoundaryMinutes?: number[]
  emptyLabel?: string
  minHeight?: number
  labelWidth?: number
  fillHeight?: boolean
}) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: `${labelWidth}px minmax(${minimumTimelineWidth}px, 1fr)`,
        minHeight,
        flex: fillHeight ? 1 : undefined,
        overflow: 'hidden',
      }}
    >
      <Box
        data-schedule-row-label
        sx={{
          display: 'flex',
          alignItems: 'center',
          minWidth: 0,
          px: 2,
          py: 0.5,
          bgcolor: 'common.white',
        }}
      >
        {label}
      </Box>
      <ScheduleRail
        schedule={schedule}
        colorMap={colorMap}
        ariaLabel={ariaLabel}
        proposedBoundaryMinutes={proposedBoundaryMinutes}
        emptyLabel={emptyLabel}
      />
    </Box>
  )
}

type ScheduleGroupTone = 'proposed' | 'common' | 'exception' | 'unavailable'

const scheduleGroupTones = {
  proposed: {
    border: '#93C5FD',
    railBackground: '#DBEAFE',
    text: '#1D4ED8',
  },
  common: {
    border: '#A7F3D0',
    railBackground: '#D1FAE5',
    text: '#047857',
  },
  exception: {
    border: '#FED7AA',
    railBackground: '#FFEDD5',
    text: '#C2410C',
  },
  unavailable: {
    border: '#CBD5E1',
    railBackground: '#E2E8F0',
    text: '#475569',
  },
} as const

function ScheduleGroup({
  tone,
  label,
  description,
  ariaLabel,
  children,
}: {
  tone: ScheduleGroupTone
  label: string
  description: string
  ariaLabel: string
  children: ReactNode
}) {
  const colors = scheduleGroupTones[tone]
  const headingId = `time-of-day-${tone}-schedule-heading`
  const descriptionId = `time-of-day-${tone}-schedule-description`

  return (
    <Box
      component="section"
      aria-label={ariaLabel}
      aria-describedby={descriptionId}
      data-testid={`schedule-${tone}-group`}
      sx={{
        display: 'grid',
        gridTemplateColumns: `${categoryColumnWidth}px minmax(0, 1fr)`,
        overflow: 'hidden',
        borderRadius: 2,
        bgcolor: 'common.white',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'flex-start',
          flexDirection: 'column',
          gap: 0.25,
          px: 2,
          py: 1,
          borderRight: '1px solid',
          borderColor: colors.border,
          bgcolor: colors.railBackground,
        }}
      >
        <Typography
          id={headingId}
          component="h3"
          variant="subtitle2"
          sx={{ flexShrink: 0, color: colors.text, fontWeight: 800 }}
        >
          {label}
        </Typography>
        <Typography
          id={descriptionId}
          variant="caption"
          sx={{ color: 'text.secondary', lineHeight: 1.45 }}
        >
          {description}
        </Typography>
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          minWidth: 0,
          '& > * + *': {
            borderTop: '1px solid',
            borderColor: 'common.white',
          },
          '& [data-schedule-row-label]': {
            bgcolor: colors.railBackground,
          },
        }}
      >
        {children}
      </Box>
    </Box>
  )
}
function ScheduleLocationLabel({ location }: { location: ScheduleLocation }) {
  const label = formatScheduleLocation(location)

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1.25,
        minWidth: 0,
        width: '100%',
      }}
    >
      <Typography
        variant="caption"
        title={label}
        noWrap
        sx={{ color: '#172033' }}
      >
        {label}
      </Typography>
    </Box>
  )
}

function TimeAxis({ position }: { position: 'top' | 'bottom' }) {
  return (
    <Box
      aria-hidden
      sx={{
        display: 'grid',
        gridTemplateColumns: `${
          categoryColumnWidth + labelColumnWidth
        }px minmax(${minimumTimelineWidth}px, 1fr)`,
      }}
    >
      <Box />
      <Box
        sx={{
          position: 'relative',
          height: 30,
          mt: position === 'bottom' ? 0.5 : 0,
          mb: position === 'top' ? 0.5 : 0,
        }}
      >
        {timeTicks.map((tick, index) => (
          <Typography
            key={tick.minutes}
            variant="caption"
            color="text.secondary"
            sx={{
              position: 'absolute',
              top: position === 'bottom' ? 2 : 'auto',
              bottom: position === 'top' ? 2 : 'auto',
              left:
                index === timeTicks.length - 1
                  ? 'auto'
                  : `${(tick.minutes / 1440) * 100}%`,
              right: index === timeTicks.length - 1 ? 0 : 'auto',
              transform:
                index === 0 || index === timeTicks.length - 1
                  ? 'none'
                  : 'translateX(-50%)',
              fontSize: '0.68rem',
              fontVariantNumeric: 'tabular-nums',
              whiteSpace: 'nowrap',
            }}
          >
            {tick.label}
          </Typography>
        ))}
      </Box>
    </Box>
  )
}
export default function TimeOfDayScheduleComparison({
  proposedSchedule,
  commonSchedule,
  commonLocations,
  exceptions,
  unavailableLocations,
  colorMap,
}: ScheduleComparisonProps) {
  const proposedBoundaryMinutes = [
    ...new Set(
      getScheduleIntervals(proposedSchedule)
        .flatMap(({ startMinutes, endMinutes }) => [startMinutes, endMinutes])
        .filter((minutes) => minutes > 0 && minutes < 1440)
    ),
  ].sort((left, right) => left - right)
  const hasScheduleRows =
    proposedSchedule.length > 0 ||
    commonSchedule.length > 0 ||
    exceptions.length > 0 ||
    unavailableLocations.length > 0

  if (!hasScheduleRows) {
    return <Alert severity="warning">No schedule data available.</Alert>
  }

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <Stack spacing={0} sx={{ minWidth: minimumComparisonWidth }}>
        <TimeAxis position="top" />

        <Stack spacing={0.75}>
          {proposedSchedule.length > 0 && (
            <ScheduleGroup
              tone="proposed"
              label="Proposed"
              description="The recommended schedule."
              ariaLabel="Proposed schedule"
            >
              <ScheduleRow
                label={
                  <Typography
                    component="em"
                    variant="caption"
                    sx={{ fontStyle: 'italic' }}
                  >
                    Proposal
                  </Typography>
                }
                schedule={proposedSchedule}
                colorMap={colorMap}
                ariaLabel="Proposed schedule"
                proposedBoundaryMinutes={proposedBoundaryMinutes}
                fillHeight
              />
            </ScheduleGroup>
          )}

          {commonSchedule.length > 0 && (
            <ScheduleGroup
              tone="common"
              label="Common"
              description="The existing schedule used by the largest group of selected locations."
              ariaLabel={`Common schedule — ${commonLocations.length} ${
                commonLocations.length === 1 ? 'location' : 'locations'
              }`}
            >
              {commonLocations.map((location) => (
                <ScheduleRow
                  key={location.identifier}
                  label={<ScheduleLocationLabel location={location} />}
                  schedule={commonSchedule}
                  colorMap={colorMap}
                  ariaLabel={`Common existing schedule for ${formatScheduleLocation(
                    location
                  )}`}
                  proposedBoundaryMinutes={proposedBoundaryMinutes}
                />
              ))}
            </ScheduleGroup>
          )}

          {exceptions.length > 0 && (
            <ScheduleGroup
              tone="exception"
              label="Exceptions"
              description="Selected locations with an existing schedule that differs from the common schedule."
              ariaLabel={`Exceptions — ${exceptions.length} ${
                exceptions.length === 1 ? 'location' : 'locations'
              }`}
            >
              {exceptions.map(({ location, schedule }) => (
                <ScheduleRow
                  key={location.identifier}
                  label={<ScheduleLocationLabel location={location} />}
                  schedule={schedule}
                  colorMap={colorMap}
                  ariaLabel={`Existing schedule for ${formatScheduleLocation(
                    location
                  )}`}
                  proposedBoundaryMinutes={proposedBoundaryMinutes}
                />
              ))}
            </ScheduleGroup>
          )}

          {unavailableLocations.length > 0 && (
            <ScheduleGroup
              tone="unavailable"
              label="Unavailable"
              description="No current schedule data is available for these selected locations."
              ariaLabel={`Unavailable schedules — ${unavailableLocations.length} ${
                unavailableLocations.length === 1 ? 'location' : 'locations'
              }`}
            >
              {unavailableLocations.map((location) => (
                <ScheduleRow
                  key={location.identifier}
                  label={<ScheduleLocationLabel location={location} />}
                  schedule={[]}
                  colorMap={colorMap}
                  ariaLabel={`No current schedule for ${formatScheduleLocation(
                    location
                  )}`}
                  emptyLabel="No data"
                />
              ))}
            </ScheduleGroup>
          )}
        </Stack>

        <TimeAxis position="bottom" />
      </Stack>
    </Box>
  )
}
