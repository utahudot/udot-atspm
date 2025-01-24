import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { PurdueCoordinationDiagramChartOptionsDefaults } from '@/features/charts/purdueCoordinationDiagram/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import { Box, Divider, SelectChangeEvent, Typography } from '@mui/material'
import { useEffect, useState } from 'react'

interface PurdueCoordinationDiagramChartOptionsProps {
  chartDefaults: PurdueCoordinationDiagramChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const PurdueCoordinationDiagramChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PurdueCoordinationDiagramChartOptionsProps) => {
  const [binSize, setBinSize] = useState(chartDefaults.binSize?.value)
  const { yAxisDefault, setYAxisDefault } = useChartsStore()

  useEffect(() => {
    if (chartDefaults.yAxisDefault?.value) {
      setYAxisDefault(chartDefaults.yAxisDefault.value)
    }
  }, [chartDefaults.yAxisDefault?.value, setYAxisDefault])

  const handleBinSizeChange = (event: SelectChangeEvent<string>) => {
    const newBinSize = event.target.value
    setBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.binSize.id,
      option: chartDefaults.binSize.option,
      value: yAxisDefault,
    })
  }
  const handleYAxisDefaultChange = (event: SelectChangeEvent<string>) => {
    const newYAxisDefault = event.target.value
    setYAxisDefault(newYAxisDefault)

    handleChartOptionsUpdate({
      id: chartDefaults.yAxisDefault.id,
      option: chartDefaults.yAxisDefault.option,
      value: newYAxisDefault,
    })
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
      <BinSizeDropdown
        value={binSize}
        handleChange={handleBinSizeChange}
        id="purdue-coordination-diagram"
      />
      <Divider sx={{ mt: 2, mb: 2 }}>
        <Typography variant="caption">Display</Typography>
      </Divider>
      <YAxisDefaultInput
        value={yAxisDefault}
        handleChange={handleYAxisDefaultChange}
      />
    </Box>
  )
}
