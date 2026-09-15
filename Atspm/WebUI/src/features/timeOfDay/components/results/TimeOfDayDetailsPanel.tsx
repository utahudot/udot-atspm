import type { TimeOfDayResult } from '@/api/reports'
import { Stack } from '@mui/material'
import type { TimeOfDayLocationNumberMap } from '../../transformers'
import {
  getCrossTrafficLocations,
  getLocationPeakEvents,
  getMovementPressures,
} from '../../transformers'
import type { TimeOfDayDetailTab } from '../chart/TimeOfDayChartHeader'
import {
  CrossTrafficLocationList,
  MovementPressureList,
  PeakList,
} from './TimeOfDayDetailTables'

const periods = ['AM', 'Midday', 'PM']
const detailRegionSx = { px: 1.5, pt: 1.75, pb: 2 }

export default function TimeOfDayDetailsPanel({
  result,
  detailTab,
  selectedSeries,
  locationNumberMap,
  selectedDetailKey,
  onSelectDetail,
  onSetSeriesVisibility,
}: {
  result: TimeOfDayResult
  detailTab: TimeOfDayDetailTab
  selectedSeries: Record<string, boolean>
  locationNumberMap: TimeOfDayLocationNumberMap
  selectedDetailKey?: string
  onSelectDetail: (detailKey: string) => void
  onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
}) {
  return (
    <Stack spacing={2}>
      {detailTab === 'signal-peaks' && (
        <Stack
          spacing={2.5}
          role="region"
          aria-label="Peaks"
          sx={detailRegionSx}
        >
          <PeakList
            title="AM Signal Peaks"
            peaks={getLocationPeakEvents(result.planProfile?.peaks, 'AM')}
            seriesVisible={Boolean(selectedSeries['AM Signal Peaks'])}
            selectedDetailKey={selectedDetailKey}
            onSelectDetail={onSelectDetail}
            onSetSeriesVisibility={(visible) =>
              onSetSeriesVisibility(['AM Signal Peaks'], visible)
            }
          />
          <PeakList
            title="PM Signal Peaks"
            peaks={getLocationPeakEvents(result.planProfile?.peaks, 'PM')}
            seriesVisible={Boolean(selectedSeries['PM Signal Peaks'])}
            selectedDetailKey={selectedDetailKey}
            onSelectDetail={onSelectDetail}
            onSetSeriesVisibility={(visible) =>
              onSetSeriesVisibility(['PM Signal Peaks'], visible)
            }
          />
        </Stack>
      )}

      {detailTab === 'cross-traffic' && (
        <Stack
          spacing={2.5}
          role="region"
          aria-label="Cross Traffic"
          sx={detailRegionSx}
        >
          {periods.map((period) => (
            <CrossTrafficLocationList
              key={period}
              title={`${period} Cross Traffic Locations`}
              period={period}
              locations={getCrossTrafficLocations(
                result.splitPressure?.crossTrafficLocations,
                period
              )}
              locationNumberMap={locationNumberMap}
              seriesVisible={Boolean(
                selectedSeries[`${period} Cross Traffic Locations`]
              )}
              selectedDetailKey={selectedDetailKey}
              onSelectDetail={onSelectDetail}
              onSetSeriesVisibility={(visible) =>
                onSetSeriesVisibility(
                  [`${period} Cross Traffic Locations`],
                  visible
                )
              }
            />
          ))}
        </Stack>
      )}

      {detailTab === 'movement-demand' && (
        <Stack
          spacing={2.5}
          role="region"
          aria-label="Movement Demand"
          sx={detailRegionSx}
        >
          {['AM', 'PM'].map((period) => (
            <MovementPressureList
              key={period}
              title={`${period} Movement Demand`}
              period={period}
              movements={getMovementPressures(
                result.splitPressure?.movementPressures,
                period,
                locationNumberMap
              )}
              locationNumberMap={locationNumberMap}
              seriesVisible={Boolean(
                selectedSeries[`${period} Movement Demand`]
              )}
              selectedDetailKey={selectedDetailKey}
              onSelectDetail={onSelectDetail}
              onSetSeriesVisibility={(visible) =>
                onSetSeriesVisibility([`${period} Movement Demand`], visible)
              }
            />
          ))}
        </Stack>
      )}
    </Stack>
  )
}
