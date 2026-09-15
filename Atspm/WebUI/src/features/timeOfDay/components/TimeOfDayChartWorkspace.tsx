import { Box } from '@mui/material'
import type { ReactNode } from 'react'
import { memo, useCallback, useEffect, useMemo, useState } from 'react'
import type {
  TimeOfDayAnalysisModel,
  TimeOfDayChartLayerId,
} from '../transformers'
import { withScheduleDifferenceVisibility } from '../transformers'
import type {
  TimeOfDayDetailTab,
  TimeOfDaySidebarTab,
} from './chart/TimeOfDayChartHeader'
import TimeOfDayChartHeader, {
  getTimeOfDayDefaultDetailTab,
  getTimeOfDayDetailTabForKey,
} from './chart/TimeOfDayChartHeader'
import type { TimeOfDayScheduleView } from './chart/TimeOfDayEChart'
import TimeOfDayEChart from './chart/TimeOfDayEChart'
import type { TimeOfDayAnalysisMode } from './chart/TimeOfDayLayersPanel'
import TimeOfDayLayersPanel, {
  getAnalysisModeSeriesSelection,
  getLayerAnalysisMode,
  getSidebarLayers,
} from './chart/TimeOfDayLayersPanel'

const sidebarWidths = {
  layers: 360,
  details: 700,
}
const workspacePaneHeight = 820

const scheduleLayerIdByView: Record<
  TimeOfDayScheduleView,
  TimeOfDayChartLayerId
> = {
  proposed: 'proposed-schedule',
  existing: 'existing-schedule',
}

const sidebarTransition = (theme: {
  transitions: {
    create: (
      properties: string | string[],
      options: { duration: number; easing: string }
    ) => string
    duration: { standard: number }
    easing: { easeInOut: string }
  }
}) =>
  theme.transitions.create('width', {
    duration: theme.transitions.duration.standard,
    easing: theme.transitions.easing.easeInOut,
  })

const SidebarDetails = memo(
  function SidebarDetails({
    active,
    children,
  }: {
    active: boolean
    children: ReactNode
  }) {
    return (
      <Box
        sx={{
          display: active ? 'block' : 'none',
          width: { xs: '100%', md: sidebarWidths.details },
        }}
      >
        {children}
      </Box>
    )
  },
  (previous, next) => !previous.active && !next.active
)

interface TimeOfDayChartWorkspaceProps {
  model: TimeOfDayAnalysisModel
  summary: ReactNode
  renderDetails: (props: {
    detailTab: TimeOfDayDetailTab
    selectedSeries: Record<string, boolean>
    selectedDetailKey?: string
    onSelectDetail: (detailKey: string) => void
    onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
  }) => ReactNode
}

export default function TimeOfDayChartWorkspace({
  model,
  summary,
  renderDetails,
}: TimeOfDayChartWorkspaceProps) {
  const [activeMode, setActiveMode] =
    useState<TimeOfDayAnalysisMode>('recommendation')
  const [sidebarTab, setSidebarTab] = useState<TimeOfDaySidebarTab>('layers')
  const [hasOpenedDetails, setHasOpenedDetails] = useState(false)
  const [selectedSeries, setSelectedSeries] = useState<Record<string, boolean>>(
    () => model.defaultSelectedSeries
  )
  const [selectedDetailKey, setSelectedDetailKey] = useState<string>()

  useEffect(() => {
    setActiveMode('recommendation')
    setSidebarTab('layers')
    setHasOpenedDetails(false)
    setSelectedSeries(model.defaultSelectedSeries)
    setSelectedDetailKey(undefined)
  }, [model])

  const setSeriesVisibility = useCallback(
    (seriesNames: string[], visible: boolean) => {
      setSelectedSeries((current) => {
        const next = { ...current }
        seriesNames.forEach((seriesName) => {
          next[seriesName] = visible
        })

        return withScheduleDifferenceVisibility(model.layers, next, seriesNames)
      })
    },
    [model.layers]
  )

  const toggleScheduleView = useCallback(
    (view: TimeOfDayScheduleView) => {
      const scheduleLayer = model.layers.find(
        (layer) => layer.id === scheduleLayerIdByView[view]
      )
      if (!scheduleLayer?.available) return

      setSelectedSeries((current) => {
        const next = { ...current }
        const allVisible = scheduleLayer.seriesNames.every(
          (seriesName) => current[seriesName] === true
        )
        scheduleLayer.seriesNames.forEach((seriesName) => {
          next[seriesName] = !allVisible
        })

        return withScheduleDifferenceVisibility(
          model.layers,
          next,
          scheduleLayer.seriesNames
        )
      })
    },
    [model.layers]
  )

  const changeAnalysisMode = useCallback(
    (mode: TimeOfDayAnalysisMode) => {
      if (mode === activeMode) return

      setActiveMode(mode)
      setSidebarTab((tab) =>
        tab === 'layers' ? tab : getTimeOfDayDefaultDetailTab(mode)
      )
      setSelectedSeries((currentSelection) =>
        getAnalysisModeSeriesSelection(model.layers, [mode], currentSelection)
      )
    },
    [activeMode, model.layers]
  )

  const selectDetail = useCallback(
    (detailKey: string) => {
      const target = model.detailTargets[detailKey]
      if (target) {
        const targetLayer = model.layers.find(
          (layer) => layer.id === target.layerId
        )
        const targetMode = targetLayer
          ? getLayerAnalysisMode(targetLayer)
          : undefined
        if (targetMode) changeAnalysisMode(targetMode)
        setSeriesVisibility([target.seriesName], true)
      }
      setSelectedDetailKey(detailKey)
      setHasOpenedDetails(true)
      setSidebarTab(getTimeOfDayDetailTabForKey(detailKey))
    },
    [changeAnalysisMode, model.detailTargets, model.layers, setSeriesVisibility]
  )

  const changeSidebarTab = useCallback((tab: TimeOfDaySidebarTab) => {
    if (tab !== 'layers') setHasOpenedDetails(true)
    setSidebarTab(tab)
  }, [])

  const showPercentAxis = model.percentSeriesNames.some(
    (seriesName) => selectedSeries[seriesName]
  )
  const sidebarLayers = useMemo(
    () => getSidebarLayers(model.layers, [activeMode]),
    [activeMode, model.layers]
  )
  const sidebarWidth =
    sidebarTab === 'layers' ? sidebarWidths.layers : sidebarWidths.details
  const detailTab =
    sidebarTab === 'layers'
      ? getTimeOfDayDefaultDetailTab(activeMode)
      : sidebarTab
  const selectedDetail = selectedDetailKey
    ? model.detailTargets[selectedDetailKey]
    : undefined
  const details = useMemo(
    () =>
      renderDetails({
        detailTab,
        selectedSeries,
        selectedDetailKey,
        onSelectDetail: selectDetail,
        onSetSeriesVisibility: setSeriesVisibility,
      }),
    [
      detailTab,
      renderDetails,
      selectDetail,
      selectedDetailKey,
      selectedSeries,
      setSeriesVisibility,
    ]
  )

  return (
    <Box sx={{ width: '100%' }}>
      <TimeOfDayChartHeader
        model={model}
        sidebarTab={sidebarTab}
        sidebarWidth={sidebarWidth}
        activeMode={activeMode}
        onChangeSidebarTab={changeSidebarTab}
        onChangeAnalysisMode={changeAnalysisMode}
      />
      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', md: 'row' },
          height: { xs: 'auto', md: workspacePaneHeight },
          minHeight: 0,
          border: '1px solid',
          borderColor: 'divider',
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            display: 'flex',
            flex: 1,
            flexDirection: 'column',
            width: '100%',
            height: { xs: workspacePaneHeight, md: '100%' },
            minWidth: 0,
            minHeight: 0,
            overflow: 'hidden',
          }}
        >
          {summary}
          <Box sx={{ flex: 1, minHeight: 0, overflowX: 'auto' }}>
            <Box sx={{ width: '100%', height: '100%', minWidth: 600 }}>
              <TimeOfDayEChart
                option={model.option}
                selectedSeries={selectedSeries}
                showPercentAxis={showPercentAxis}
                selectedDetail={selectedDetail}
                onSelectDetail={selectDetail}
                onToggleScheduleView={toggleScheduleView}
              />
            </Box>
          </Box>
        </Box>
        <Box
          component="aside"
          aria-label="Time-of-day chart controls"
          sx={{
            width: { xs: '100%', md: sidebarWidth },
            height: { xs: workspacePaneHeight, md: '100%' },
            flexShrink: 0,
            minHeight: 0,
            borderLeft: { xs: 0, md: '1px solid' },
            borderTop: { xs: '1px solid', md: 0 },
            borderLeftColor: { md: 'divider' },
            borderTopColor: { xs: 'divider' },
            display: 'flex',
            flexDirection: 'column',
            bgcolor: 'common.white',
            overflow: 'hidden',
            transition: sidebarTransition,
            '@media (prefers-reduced-motion: reduce)': {
              transition: 'none',
            },
          }}
        >
          <Box
            sx={{
              flex: 1,
              minHeight: 0,
              overflowY: 'auto',
              overflowX: 'hidden',
              overscrollBehavior: 'contain',
              scrollbarGutter: 'stable',
            }}
          >
            <Box
              sx={{
                display: sidebarTab === 'layers' ? 'block' : 'none',
                width: { xs: '100%', md: sidebarWidths.layers },
                boxSizing: 'border-box',
                p: 1.25,
              }}
            >
              <TimeOfDayLayersPanel
                layers={sidebarLayers}
                selectedSeries={selectedSeries}
                onSetSeriesVisibility={setSeriesVisibility}
              />
            </Box>
            {hasOpenedDetails && (
              <SidebarDetails active={sidebarTab !== 'layers'}>
                {details}
              </SidebarDetails>
            )}
          </Box>
        </Box>
      </Box>
    </Box>
  )
}
