import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { PurdueCoordinationDiagramChartOptionsDefaults } from '@/features/charts/purdueCoordinationDiagram/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import { Box, Paper, SelectChangeEvent, Typography } from '@mui/material'
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
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value ?? 150)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

  const handleBinSizeChange = (event: SelectChangeEvent<string>) => {
    const newBinSize = event.target.value
    setBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.binSize.id,
      option: chartDefaults.binSize.option,
      value: yAxisDefault,
    })
  }

  const handleYAxisChartStoreUpdate = (val: string | null) => {
    setYAxisMaxStore(val)
  }

  const updateYAxisDefault = (event: SelectChangeEvent<string>) => {
    const newYAxis = event.target.value
    setYAxisMax(newYAxis)

    handleChartOptionsUpdate({
      value: newYAxis,
      option: chartDefaults.yAxisDefault.option,
      id: chartDefaults.yAxisDefault.id,
    })
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
      <BinSizeDropdown
        value={binSize}
        handleChange={handleBinSizeChange}
        id="purdue-coordination-diagram"
      />
      {isMeasureDefaultView ? (
        <YAxisDefaultInput
          value={yAxisMax}
          handleChange={updateYAxisDefault}
          isMeasureDefaultView={isMeasureDefaultView}
        />
      ) : (
        <Paper
          sx={{ px: 2, py: 1, my: 1, bgcolor: 'background.default' }}
          elevation={0}
        >
          <Typography variant="caption">Display</Typography>
          <YAxisDefaultInput
            value={yAxisMax}
            handleChange={handleYAxisChartStoreUpdate}
            isMeasureDefaultView={isMeasureDefaultView}
          />
        </Paper>
      )}
    </Box>
  )
}
