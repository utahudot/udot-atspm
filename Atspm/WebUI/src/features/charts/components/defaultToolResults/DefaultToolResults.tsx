import TimeSpaceEChart from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceEChart'
import LinkPivotAdjustmentTable from '@/features/tools/link-pivot/components/LinkPivotAdjustmentTable'
import { LinkPivotApproachLinkComponent } from '@/features/tools/link-pivot/components/LinkPivotApproachLinkComponent'
import { RawLinkPivotForTsdData } from '@/features/tools/link-pivot/types'
import { Box, Paper, Tab, Tabs, Typography, useTheme } from '@mui/material'
import { useEffect, useState } from 'react'
import { transformTimeSpaceData } from '../../api'
import { GpxUploadAccordion } from '../../timeSpaceDiagram/shared/components/GpxUploader/GpxUploadAccordion'
import { IgnoreLocationsAccordion } from '../../timeSpaceDiagram/shared/components/IgnoredLocations/IgnoredLocations'
import {
  GpxUploadOptions,
  RawTimeSpaceDiagramResponse,
  TimeSpaceBaseData,
} from '../../timeSpaceDiagram/shared/types'

export interface TimeSpaceChartProps {
  timeSpaceData: RawTimeSpaceDiagramResponse
  linkPivotTsdData: RawLinkPivotForTsdData[]
}

function createEmptyEntry(
  locations: string[],
  primary = false
): GpxUploadOptions {
  return {
    id: '',
    startLocation: locations[0],
    endLocation: locations[locations.length - 1],
    error: null,
    primary,
  }
}

function recomputeTimeSpaceData<T extends TimeSpaceBaseData>(
  baseData: T[],
  ignoredLocations: string[]
): T[] {
  const isIgnored = (id: string) => ignoredLocations.includes(id)

  const recomputeLane = (lane: T[]): T[] => {
    // If nothing ignored, just return non-ignored nodes unchanged
    if (!lane.some((l) => isIgnored(l.locationIdentifier))) {
      return lane
    }

    const recomputed: T[] = []

    for (let i = 0; i < lane.length; i++) {
      const current = lane[i]

      if (isIgnored(current.locationIdentifier)) {
        recomputed.push({
          start: current.start,
          end: current.end,
          locationIdentifier: current.locationIdentifier,
          locationDescription: current.locationDescription,
          phaseType: current.phaseType,
          distanceToNextLocation: current.distanceToNextLocation,
          distanceToPreviousLocation: current.distanceToPreviousLocation,
          phaseNumber: current.phaseNumber,
          phaseNumberSort: current.phaseNumberSort,
          speed: current.speed,
          approachId: current.approachId,
          approachDescription: current.approachDescription,
          calculatedDistanceToNext: 0,
          calculatedDistanceToPrevious: 0,
        } as T)

        continue
      }

      // ---- distance to previous non-ignored ----
      let distanceToPrevious = 0
      for (let j = i - 1; j >= 0; j--) {
        distanceToPrevious += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j].locationIdentifier)) {
          break
        }
      }

      // ---- distance to next non-ignored ----
      let distanceToNext = 0
      for (let j = i; j < lane.length - 1; j++) {
        distanceToNext += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j + 1].locationIdentifier)) {
          break
        }
      }

      recomputed.push({
        ...current,
        calculatedDistanceToPrevious: distanceToPrevious,
        calculatedDistanceToNext: distanceToNext,
      })
    }

    return recomputed
  }

  const primaryLane = baseData.filter((p) => p.phaseType === 'Primary')
  const opposingLane = baseData.filter((p) => p.phaseType === 'Opposing')

  return [...recomputeLane(primaryLane), ...recomputeLane(opposingLane)]
}

function addDefaultValues(
  timeSpaceData: RawTimeSpaceDiagramResponse
): RawTimeSpaceDiagramResponse {
  const data = timeSpaceData.data
  data.forEach((lane) => {
    lane.calculatedDistanceToNext = lane.distanceToNextLocation
    lane.calculatedDistanceToPrevious = lane.distanceToPreviousLocation
  })
  return {
    type: timeSpaceData.type,
    data,
  }
}

export default function TimeSpaceChart({
  timeSpaceData,
  linkPivotTsdData,
}: TimeSpaceChartProps) {
  const theme = useTheme()
  const [activeTab, setActiveTab] = useState(0)

  const [baseTimeSpaceData, setBaseTimeSpaceData] =
    useState<RawTimeSpaceDiagramResponse>(addDefaultValues(timeSpaceData))

  const [transformedData, setTransformedData] = useState(() =>
    transformTimeSpaceData(timeSpaceData)
  )

  const locations = timeSpaceData.data
    .filter((p) => p.phaseType === 'Primary')
    .map((p) => p.locationIdentifier)

  const [gpxEntries, setGpxEntries] = useState<GpxUploadOptions[]>([
    createEmptyEntry(locations),
  ])
  const [ignoredLocations, setIgnoredLocation] = useState<string[]>([])

  useEffect(() => {
    const recalculatedData =
      ignoredLocations.length > 0
        ? recomputeTimeSpaceData(baseTimeSpaceData.data, ignoredLocations)
        : baseTimeSpaceData.data

    const updatedResponse: RawTimeSpaceDiagramResponse = {
      type: baseTimeSpaceData.type,
      data: recalculatedData,
    }

    setTransformedData(transformTimeSpaceData(updatedResponse))
  }, [ignoredLocations, baseTimeSpaceData])

  return (
    <Box
      sx={{
        overflow: 'hidden',
        width: '100%',
        position: 'absolute',
        left: 0,
      }}
    >
      {/* 🔹 Tabs Outside Paper */}
      <Tabs
        value={activeTab}
        onChange={(_, newValue) => setActiveTab(newValue)}
        sx={{ mt: 2 }}
      >
        <Tab label="Time Space Chart" />
        <Tab label="Link Pivot" />
      </Tabs>

      {/* 🔹 Default Tab — Existing Paper Layout */}
      {activeTab === 0 && (
        <Paper
          sx={{
            p: 0,
            mt: 2,
            marginLeft: '2px',
            backgroundColor: 'white',
          }}
        >
          <Box
            sx={{
              display: 'flex',
              width: '100%',
              minHeight: '100%',
            }}
          >
            {/* LEFT SIDE — GPX OPTIONS */}
            <Box
              sx={{
                width: '20%',
                minWidth: 260,
                borderRight: '1px solid',
                borderColor: 'divider',
                p: 2,
              }}
            >
              <GpxUploadAccordion
                locations={locations}
                entries={gpxEntries}
                setEntries={setGpxEntries}
              />
              <IgnoreLocationsAccordion
                locations={locations}
                ignoredLocations={ignoredLocations}
                setIgnoredLocations={setIgnoredLocation}
              />
            </Box>

            {/* RIGHT SIDE — CHART */}
            <Box
              sx={{
                width: '80%',
                p: 2,
              }}
            >
              <TimeSpaceEChart
                id="time-space-chart"
                option={transformedData.data.chart}
                theme={theme.palette.mode}
                style={{
                  width: '100%',
                  height:
                    locations.length < 5
                      ? locations.length * 200 + 160 + 'px'
                      : locations.length * 150 + 160 + 'px',
                  position: 'relative',
                }}
                gpxEntries={gpxEntries}
                ignoredLocations={ignoredLocations}
              />
            </Box>
          </Box>
        </Paper>
      )}

      {/* 🔹 Second Tab — LinkPivot (Outside Paper) */}
      {activeTab === 1 && (
        <Box>
          {linkPivotTsdData.map((pivot) => (
            <Box key={pivot.direction} sx={{ mb: 6 }}>
              <Typography variant="h4" fontWeight="bold" sx={{ my: 3 }}>
                {pivot.direction} Direction
              </Typography>

              {/* Adjustments */}
              <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
                Adjustments
              </Typography>
              <Paper sx={{ mb: 3 }}>
                <LinkPivotAdjustmentTable
                  data={pivot.data.adjustments}
                  cycleLength={baseTimeSpaceData.data[0].cycleLength}
                />
              </Paper>

              {/* Approach Link Comparison */}
              <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
                Approach Link Comparison
              </Typography>
              <Paper>
                <LinkPivotApproachLinkComponent
                  data={pivot.data.approachLinks}
                  corridorSummary={pivot.data}
                  lpHandler={null}
                />
              </Paper>
            </Box>
          ))}
        </Box>
      )}
    </Box>
  )
}
