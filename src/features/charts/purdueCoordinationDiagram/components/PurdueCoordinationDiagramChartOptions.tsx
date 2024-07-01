import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { PurdueCoordinationDiagramChartOptionsDefaults } from '@/features/charts/purdueCoordinationDiagram/types'
import { Default } from '@/features/charts/types'
import { SelectChangeEvent } from '@mui/material'
import { useState } from 'react'

interface PurdueCoordinationDiagramChartOptionsProps {
  chartDefaults: PurdueCoordinationDiagramChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const PurdueCoordinationDiagramChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PurdueCoordinationDiagramChartOptionsProps) => {
  const [binSize, setBinSize] = useState(chartDefaults.binSize?.value)

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
    <BinSizeDropdown
      value={binSize}
      handleChange={handleBinSizeChange}
      id="purdue-coordination-diagram"
    />
  )
}
