import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformHourlyPedVolByHourOfDay from '@/features/charts/pedat/avgHourlyPedVolByHour'
import { Paper } from '@mui/material'
import { useMemo } from 'react'

const HourlyPedVolByHourOfDayChart = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
  const combinedData = Array.from({ length: 24 }, (_, hour) => {
    const averageVolume =
      data?.reduce(
        (sum, loc) =>
          sum +
          (loc.averageVolumeByHourOfDay?.find((d) => d.index === hour)
            ?.volume ?? 0),
        0
      ) ?? 0

    return { hour, averageVolume }
  })

  const base = transformHourlyPedVolByHourOfDay(combinedData || [])

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )
  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <ApacheEChart
        id="hourly-ped-vol-hour-of-day"
        option={option}
        style={{ width: '100%', height: '400px' }}
        hideInteractionMessage
      />
    </Paper>
  )
}

export default HourlyPedVolByHourOfDayChart
