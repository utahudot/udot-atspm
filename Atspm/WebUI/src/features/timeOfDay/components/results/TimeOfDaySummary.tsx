import type { TimeOfDayResult } from '@/api/reports'
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import ShuffleIcon from '@mui/icons-material/Shuffle'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import WbSunnyOutlinedIcon from '@mui/icons-material/WbSunnyOutlined'
import { Box, Typography } from '@mui/material'

interface SummaryItem {
  label: string
  value: string
}

interface TimeOfDaySummaryProps {
  result: TimeOfDayResult
  peakItems: SummaryItem[]
}

interface PeakDisplayValue {
  time: string
  value: string
  unit: string
}

const peakDisplayBySource: Record<
  string,
  { label: string; color: string; Icon: typeof WbSunnyOutlinedIcon }
> = {
  'AM Corridor Peak': {
    label: 'AM peak',
    color: '#ef6c00',
    Icon: WbSunnyOutlinedIcon,
  },
  'PM Corridor Peak': {
    label: 'PM peak',
    color: '#1565c0',
    Icon: DarkModeOutlinedIcon,
  },
  'Peak Cross Traffic': {
    label: 'Peak cross traffic',
    color: '#2e7d32',
    Icon: ShuffleIcon,
  },
}

const parsePeakValue = (sourceValue: string): PeakDisplayValue => {
  const separator = ' - '
  const separatorIndex = sourceValue.indexOf(separator)
  const isTimeOnly = /^\d{1,2}:\d{2}$/.test(sourceValue)
  const time =
    separatorIndex >= 0
      ? sourceValue.slice(0, separatorIndex)
      : isTimeOnly
        ? sourceValue
        : ''
  const measurement =
    separatorIndex >= 0
      ? sourceValue.slice(separatorIndex + separator.length)
      : isTimeOnly
        ? ''
        : sourceValue
  const measurementParts = measurement.match(
    /^([+-]?(?:\d[\d,]*)(?:\.\d+)?)\s*(.*)$/
  )

  return {
    time,
    value: measurementParts?.[1] ?? measurement,
    unit: measurementParts?.[2] ?? '',
  }
}

const getActionableReviewThreshold = (
  thresholds?: Record<string, number> | null
) => {
  const normalizedThresholds = new Map(
    Object.entries(thresholds ?? {}).map(([name, value]) => [
      name.toLowerCase().replace(/[^a-z]/g, ''),
      value,
    ])
  )
  const splitReviewThreshold = normalizedThresholds.get('splitreview') ?? 35
  const shoulderReviewThreshold =
    normalizedThresholds.get('shoulderreview') ?? 45

  return Math.min(splitReviewThreshold, shoulderReviewThreshold)
}

export default function TimeOfDaySummary({
  result,
  peakItems,
}: TimeOfDaySummaryProps) {
  const splitPressure = result.splitPressure
  const reviewText = splitPressure?.reviewText?.trim()
  const peakCrossTrafficPercent = splitPressure?.peakCrossTrafficPercent
  const hasActionableReview =
    peakCrossTrafficPercent !== undefined &&
    peakCrossTrafficPercent !== null &&
    Number.isFinite(peakCrossTrafficPercent) &&
    peakCrossTrafficPercent >=
      getActionableReviewThreshold(splitPressure?.thresholdPercentByName)
  const hasMeasuredPeaks = peakItems.length > 0
  if (!hasMeasuredPeaks && !reviewText) return null

  return (
    <Box
      component="section"
      aria-label="Chart summary"
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: 'minmax(0, 1fr)',
          lg:
            hasMeasuredPeaks && reviewText
              ? 'minmax(0, 1fr) minmax(300px, 0.65fr)'
              : 'minmax(0, 1fr)',
        },
        minWidth: 0,
        borderBottom: '1px solid',
        borderColor: 'divider',
        bgcolor: 'common.white',
      }}
    >
      {hasMeasuredPeaks && (
        <Box
          aria-label="Measured peaks"
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: 'repeat(auto-fit, minmax(120px, 1fr))',
              sm: `repeat(${peakItems.length}, minmax(0, 1fr))`,
            },
            minWidth: 0,
          }}
        >
          {peakItems.map((item, index) => {
            const displayValue = parsePeakValue(item.value)
            const display = peakDisplayBySource[item.label] ?? {
              label: item.label,
              color: '#546e7a',
              Icon: ShuffleIcon,
            }
            const Icon = display.Icon

            return (
              <Box
                key={item.label}
                sx={{
                  minWidth: 0,
                  px: { xs: 1.25, sm: 1.5 },
                  py: 1,
                  bgcolor: '#f9f9fb',
                  borderRight: {
                    xs: 0,
                    sm: index < peakItems.length - 1 ? '1px solid' : 0,
                  },
                  borderRightColor: { sm: 'divider' },
                }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 0.75,
                    minWidth: 0,
                  }}
                >
                  <Icon
                    aria-hidden
                    sx={{
                      flex: '0 0 auto',
                      color: display.color,
                      fontSize: 17,
                    }}
                  />
                  <Typography
                    variant="caption"
                    sx={{
                      minWidth: 0,
                      overflow: 'hidden',
                      color: 'text.secondary',
                      fontWeight: 500,
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {display.label}
                  </Typography>
                </Box>
                <Typography
                  component="div"
                  variant="body2"
                  sx={{
                    display: 'flex',
                    alignItems: 'baseline',
                    flexWrap: 'wrap',
                    columnGap: 0.5,
                    mt: 0.25,
                    minWidth: 0,
                    fontVariantNumeric: 'tabular-nums',
                  }}
                >
                  {displayValue.value && (
                    <Box
                      component="span"
                      sx={{
                        color: 'text.primary',
                        fontWeight: 700,
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {displayValue.value}
                    </Box>
                  )}
                  {displayValue.unit && (
                    <Box
                      component="span"
                      sx={{
                        color: 'text.secondary',
                        fontWeight: 400,
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {displayValue.unit}
                    </Box>
                  )}
                  {displayValue.time && (
                    <Box
                      component="span"
                      sx={{
                        ml: 'auto',
                        color: 'text.secondary',
                        fontWeight: 500,
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {displayValue.time}
                    </Box>
                  )}
                </Typography>
              </Box>
            )
          })}
        </Box>
      )}

      {reviewText && (
        <Box
          role={hasActionableReview ? 'alert' : 'status'}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-start',
            minWidth: 0,
            px: { xs: 1.25, sm: 1.5 },
            py: 1,
            borderTop: {
              xs: hasMeasuredPeaks ? '1px solid' : 0,
              lg: 0,
            },
            borderTopColor: { xs: 'divider' },
            borderLeft: {
              lg: hasMeasuredPeaks ? '1px solid' : 0,
            },
            borderLeftColor: { lg: 'divider' },
            bgcolor: hasActionableReview ? '#FFF9ED' : '#F4F8FC',
          }}
        >
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'auto minmax(0, 1fr)',
              alignItems: 'start',
              gap: 1,
              width: '100%',
              maxWidth: '100%',
              minWidth: 0,
            }}
          >
            {hasActionableReview ? (
              <WarningAmberOutlinedIcon
                aria-hidden
                sx={{ mt: 0.05, color: 'warning.main', fontSize: 20 }}
              />
            ) : (
              <InfoOutlinedIcon
                aria-hidden
                sx={{ mt: 0.05, color: 'info.main', fontSize: 20 }}
              />
            )}
            <Box sx={{ minWidth: 0 }}>
              <Typography
                component="h3"
                sx={{
                  fontSize: '0.78rem',
                  fontWeight: 800,
                  lineHeight: 1.25,
                }}
              >
                {hasActionableReview ? 'Review' : 'Review status'}
              </Typography>
              <Typography
                sx={{
                  mt: 0.2,
                  color: 'text.secondary',
                  fontSize: '0.75rem',
                  lineHeight: 1.35,
                }}
              >
                {reviewText}
              </Typography>
            </Box>
          </Box>
        </Box>
      )}
    </Box>
  )
}
