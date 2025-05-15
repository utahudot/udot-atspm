import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { PurdueCoordinationDiagramChartOptionsDefaults } from '@/features/charts/purdueCoordinationDiagram/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import { Box, SelectChangeEvent } from '@mui/material'
import { useEffect, useState } from 'react'

interface PurdueCoordinationDiagramChartOptionsProps {
  chartDefaults: PurdueCoordinationDiagramChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const PurdueCoordinationDiagramChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: PurdueCoordinationDiagramChartOptionsProps) => {
  const [binSize, setBinSize] = useState(chartDefaults.binSize?.value)
  const [yAxisMax, setYAxisMax] = useState<string | null>(
    chartDefaults.yAxisDefault?.value ?? null
  )
  const { setYAxisMaxStore } = useChartsStore()

  useEffect(() => {
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

  const handleBinSizeChange = (event: SelectChangeEvent<string>) => {
    const newBinSize = event.target.value
    setBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.binSize.id,
      option: chartDefaults.binSize.option,
      value: newBinSize,
    })
  }

  const updateYAxisDefault = (nexYAxis: string) => {
    setYAxisMax(nexYAxis)
    if (isMeasureDefaultView) {
      handleChartOptionsUpdate({
        value: nexYAxis,
        option: chartDefaults.yAxisDefault.option,
        id: chartDefaults.yAxisDefault.id,
      })
    } else {
      setYAxisMaxStore(nexYAxis)
    }
  }

  return (
    <Box
      sx={{ display: 'flex', flexDirection: 'column', gap: 1, flex: '1 1 0%' }}
    >
      <BinSizeDropdown
        value={binSize}
        handleChange={handleBinSizeChange}
        id="purdue-coordination-diagram"
      />
      <YAxisDefaultInput
        value={yAxisMax}
        handleChange={updateYAxisDefault}
        isMeasureDefaultView={isMeasureDefaultView}
      />
    </Box>
  )
}
