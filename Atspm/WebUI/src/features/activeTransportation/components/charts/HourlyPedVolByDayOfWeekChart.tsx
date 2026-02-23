import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformHourlyPedVolByDayOfWeekTransformer from '@/features/charts/pedat/hourlyPedVolByDayOfWeekTransformer'
import { Paper } from '@mui/material'
import { useMemo } from 'react'

const dayOrder = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
]

const HourlyPedVolByDayOfWeekChart = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
  const combinedData = dayOrder.map((day, i) => {
    const dayVolume =
      data
        ?.map(
          (loc) =>
            loc.averageVolumeByDayOfWeek?.find((d) => d.index === i)?.volume ||
            0
        )
        .reduce((a, b) => a + b, 0) || 0

    return { day, averageVolume: dayVolume }
  })
  const base = transformHourlyPedVolByDayOfWeekTransformer(combinedData || [])

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )

  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <ApacheEChart
        id="hourly-ped-vol-day-of-week"
        option={option}
        style={{ width: '100%', height: '400px' }}
      />
    </Paper>
  )
}

export default HourlyPedVolByDayOfWeekChart
