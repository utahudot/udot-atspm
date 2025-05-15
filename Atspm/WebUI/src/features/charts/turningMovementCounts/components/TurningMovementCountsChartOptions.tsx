import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { TurningMovementCountsChartOptionsDefaults } from '@/features/charts/turningMovementCounts/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import { SelectChangeEvent } from '@mui/material'
import { useEffect, useState } from 'react'

interface TurningMovementCountsChartOptionsProps {
  chartDefaults: TurningMovementCountsChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const TurningMovementCountsChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: TurningMovementCountsChartOptionsProps) => {
  const [binSize, setBinSize] = useState(chartDefaults.binSize?.value)

  const { setYAxisMaxStore } = useChartsStore()

  const [yAxisMax, setYAxisMax] = useState<string | null>(
    chartDefaults.yAxisDefault?.value
  )

  useEffect(() => {
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

  const updateYAxisDefault = (newYAxis: string) => {
    setYAxisMax(newYAxis)

    if (isMeasureDefaultView) {
      handleChartOptionsUpdate({
        value: newYAxis,
        option: chartDefaults.yAxisDefault.option,
        id: chartDefaults.yAxisDefault.id,
      })
    } else {
      setYAxisMaxStore(newYAxis)
    }
  }

  const handleBinSizeChange = (event: SelectChangeEvent<string>) => {
    const newBinSize = event.target.value
    setBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.binSize.id,
      option: chartDefaults.binSize.option,
      value: newBinSize,
    })
  }

  return (
    <>
      <BinSizeDropdown
        value={binSize}
        handleChange={handleBinSizeChange}
        id="turning-movement-counts"
      />
      <YAxisDefaultInput
        value={yAxisMax}
        handleChange={updateYAxisDefault}
        isMeasureDefaultView={isMeasureDefaultView}
      />
    </>
  )
}
