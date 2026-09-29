import type { CalendarDayAvailability } from '@/features/dataAvailability/types'
import { Badge, Box, Tooltip, Typography } from '@mui/material'
import type { ReactNode } from 'react'

interface DayAvailabilityIndicatorProps {
  availability?: CalendarDayAvailability
  isMissing?: boolean
  children: ReactNode
}

export default function DayAvailabilityIndicator({
  availability,
  isMissing = false,
  children,
}: DayAvailabilityIndicatorProps) {
  const missingLocationCount = availability
    ? Math.max(
        0,
        availability.totalLocationCount - availability.availableLocationCount
      )
    : 0
  const isFullRouteMissing =
    isMissing ||
    (Boolean(availability) && availability?.availableLocationCount === 0)
  const hasPartialRouteAvailability =
    Boolean(availability) && !isFullRouteMissing && missingLocationCount > 0
  const missingLocationLabel = `${missingLocationCount} ${
    missingLocationCount === 1 ? 'location' : 'locations'
  } missing data`

  if (!isFullRouteMissing && !hasPartialRouteAvailability) {
    return <>{children}</>
  }

  const badgeContent = isFullRouteMissing ? (
    <span
      role="img"
      aria-label="No data available"
      style={{
        color: 'red',
        fontSize: '0.65rem',
        transform: 'translate(-50%, 50%)',
      }}
    >
      ✖
    </span>
  ) : (
    <span
      role="img"
      aria-label={missingLocationLabel}
      style={{
        color: '#ed6c02',
        fontSize: '0.65rem',
        transform: 'translate(-50%, 50%)',
      }}
    >
      ◐
    </span>
  )
  const tooltipTitle = isFullRouteMissing ? (
    'No data available'
  ) : availability ? (
    <DayAvailabilityTooltip availability={availability} />
  ) : (
    ''
  )

  return (
    <Tooltip
      title={tooltipTitle}
      enterDelay={500}
      arrow
      disableInteractive
      componentsProps={{
        tooltip: {
          sx: {
            bgcolor: 'background.paper',
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: 3,
            color: 'text.primary',
          },
        },
        arrow: { sx: { color: 'background.paper' } },
      }}
    >
      <Badge overlap="circular" badgeContent={badgeContent}>
        {children}
      </Badge>
    </Tooltip>
  )
}

function DayAvailabilityTooltip({
  availability,
}: {
  availability: CalendarDayAvailability
}) {
  return (
    <Box sx={{ py: 0.25 }}>
      <Typography variant="caption" sx={{ display: 'block', fontWeight: 700 }}>
        {availability.availableLocationCount} of{' '}
        {availability.totalLocationCount} locations have data
      </Typography>
      <Box
        sx={{ display: 'flex', flexDirection: 'column', gap: 0.25, mt: 0.5 }}
      >
        {availability.locations.map((location) => (
          <Box key={location.locationIdentifier}>
            <Typography
              variant="caption"
              component="span"
              sx={{
                alignItems: 'center',
                color: location.hasData ? 'success.dark' : 'error.dark',
                display: 'inline-flex',
                gap: 0.75,
              }}
            >
              <Box component="span" sx={{ width: '1em' }} aria-hidden>
                {location.hasData ? '✓' : '✖'}
              </Box>
              {location.locationIdentifier}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  )
}
