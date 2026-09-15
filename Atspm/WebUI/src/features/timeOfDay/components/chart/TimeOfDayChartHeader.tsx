import { Box, Button, ButtonGroup, Tab, Tabs, Typography } from '@mui/material'
import type { TimeOfDayAnalysisModel } from '../../transformers'
import type { TimeOfDayAnalysisMode } from './TimeOfDayLayersPanel'

export type TimeOfDayDetailTab =
  | 'signal-peaks'
  | 'cross-traffic'
  | 'movement-demand'
export type TimeOfDaySidebarTab = 'layers' | TimeOfDayDetailTab

const detailTabsByMode: Record<
  TimeOfDayAnalysisMode,
  Array<{ value: TimeOfDayDetailTab; label: string }>
> = {
  recommendation: [{ value: 'signal-peaks', label: 'Signal Peaks' }],
  pressure: [
    { value: 'cross-traffic', label: 'Cross Traffic' },
    { value: 'movement-demand', label: 'Movement Demand' },
  ],
}

export const getTimeOfDayDetailTabs = (mode: TimeOfDayAnalysisMode) =>
  detailTabsByMode[mode]

export const getTimeOfDayDefaultDetailTab = (
  mode: TimeOfDayAnalysisMode
): TimeOfDayDetailTab => detailTabsByMode[mode][0].value

export const getTimeOfDayDetailTabForKey = (
  detailKey: string
): TimeOfDayDetailTab => {
  if (detailKey.startsWith('crosstraffic:')) return 'cross-traffic'
  if (detailKey.startsWith('movementpressure:')) return 'movement-demand'

  return 'signal-peaks'
}

const sidebarTabSx = {
  minHeight: 40,
  minWidth: 0,
  px: 1.25,
  textTransform: 'none',
}

interface TimeOfDayChartHeaderProps {
  model: TimeOfDayAnalysisModel
  sidebarTab: TimeOfDaySidebarTab
  sidebarWidth: number
  activeMode: TimeOfDayAnalysisMode
  onChangeSidebarTab: (tab: TimeOfDaySidebarTab) => void
  onChangeAnalysisMode: (mode: TimeOfDayAnalysisMode) => void
}

const analysisModeToggleGroupSx = {
  height: '100%',
  '& .MuiButton-root': {
    height: '100%',
    minHeight: 34,
    minWidth: { xs: 112, sm: 140 },
    px: 1.5,
    py: 0,
    borderColor: '#CBD5E1',
    borderRadius: 0,
    borderTop: 0,
    borderBottom: 0,
    color: '#475569',
    fontSize: '0.75rem',
    lineHeight: 1,
    textTransform: 'none',
    whiteSpace: 'nowrap',
    '&:hover': {
      borderColor: '#94A3B8',
      backgroundColor: 'rgba(15, 23, 42, 0.05)',
    },
  },
  '& .MuiButton-root:last-of-type': {
    borderRight: 0,
  },
  '& .MuiButton-root.is-active': {
    borderColor: '#9EC5E8',
    backgroundColor: '#E3F0FB',
    color: '#09549C',
    fontWeight: 600,
    '&:hover': {
      borderColor: '#7FB2DF',
      backgroundColor: '#D2E6F7',
    },
  },
}

export default function TimeOfDayChartHeader({
  model,
  sidebarTab,
  sidebarWidth,
  activeMode,
  onChangeSidebarTab,
  onChangeAnalysisMode,
}: TimeOfDayChartHeaderProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr',
          md: `minmax(0, 1fr) auto ${sidebarWidth}px`,
        },
        width: { md: 'calc(100% - 2px)' },
        ml: { md: '1px' },
        alignItems: 'center',
        bgcolor: 'common.white',
        transition: (theme) =>
          theme.transitions.create('grid-template-columns', {
            duration: theme.transitions.duration.standard,
            easing: theme.transitions.easing.easeInOut,
          }),
        '@media (prefers-reduced-motion: reduce)': {
          transition: 'none',
        },
      }}
    >
      <Box sx={{ minWidth: 0, px: 1.75, py: 1.25 }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'baseline',
            gap: 1,
            flexWrap: 'wrap',
          }}
        >
          <Typography
            component="h2"
            variant="h5"
            sx={{
              fontSize: '1rem',
              fontWeight: 700,
              lineHeight: 1.2,
              minWidth: 0,
            }}
          >
            {model.header.title}
          </Typography>
          {model.header.dateRange && (
            <Typography
              variant="body2"
              sx={{
                color: 'text.secondary',
                fontWeight: 500,
                lineHeight: 1.2,
                whiteSpace: 'nowrap',
              }}
            >
              {' \u2022 '}
              {model.header.dateRange}
            </Typography>
          )}
        </Box>
      </Box>
      <Box
        sx={{
          alignSelf: 'stretch',
          justifySelf: { xs: 'start', md: 'end' },
          display: 'flex',
          alignItems: 'stretch',
          justifyContent: 'flex-end',
        }}
      >
        <ButtonGroup
          size="small"
          variant="outlined"
          aria-label="Time-of-day analysis modes"
          sx={analysisModeToggleGroupSx}
        >
          <Button
            className={
              activeMode === 'recommendation' ? 'is-active' : undefined
            }
            onClick={() => onChangeAnalysisMode('recommendation')}
            aria-pressed={activeMode === 'recommendation'}
          >
            Peaks
          </Button>
          <Button
            className={activeMode === 'pressure' ? 'is-active' : undefined}
            onClick={() => onChangeAnalysisMode('pressure')}
            aria-pressed={activeMode === 'pressure'}
          >
            Movement Demand &amp; Cross Traffic
          </Button>
        </ButtonGroup>
      </Box>
      <Box
        sx={{
          width: { xs: '100%', md: sidebarWidth },
          minWidth: { md: sidebarWidth },
          alignSelf: 'stretch',
          display: 'flex',
          alignItems: 'flex-end',
          borderLeft: { xs: 0, md: '1px solid' },
          borderTop: { xs: '1px solid', md: 0 },
          borderLeftColor: { md: 'divider' },
          borderTopColor: { xs: 'divider' },
          overflow: 'hidden',
          transition: (theme) =>
            theme.transitions.create(['width', 'min-width'], {
              duration: theme.transitions.duration.standard,
              easing: theme.transitions.easing.easeInOut,
            }),
          '@media (prefers-reduced-motion: reduce)': {
            transition: 'none',
          },
        }}
      >
        <Tabs
          value={sidebarTab}
          onChange={(_, value: TimeOfDaySidebarTab) =>
            onChangeSidebarTab(value)
          }
          aria-label="Time-of-day chart sidebar"
          variant="scrollable"
          scrollButtons={false}
          sx={{ px: 1.5, minHeight: 40 }}
        >
          <Tab value="layers" label="Legend" sx={sidebarTabSx} />
          {getTimeOfDayDetailTabs(activeMode).map((tab) => (
            <Tab
              key={tab.value}
              value={tab.value}
              label={tab.label}
              sx={sidebarTabSx}
            />
          ))}
        </Tabs>
      </Box>
    </Box>
  )
}
