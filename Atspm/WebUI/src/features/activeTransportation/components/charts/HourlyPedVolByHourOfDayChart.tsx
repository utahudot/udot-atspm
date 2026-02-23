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
  const combinedData = [...Array(23)].map((_, hour) => {
    const averageVolume = data
      ?.map((loc) => {
        return loc.averageVolumeByHourOfDay?.[hour]?.volume || 0
      })
      .reduce((a, b) => a + b, 0)

    const hourOfDay = hour + 1
    return { hour: hourOfDay, averageVolume: averageVolume || 0 }
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
      />
    </Paper>
  )
}

export default HourlyPedVolByHourOfDayChart
