import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { TurningMovementCountsChartOptionsDefaults } from '@/features/charts/turningMovementCounts/types'
import { Default } from '@/features/charts/types'
import { SelectChangeEvent } from '@mui/material'
import { useState } from 'react'

interface TurningMovementCountsChartOptionsProps {
  chartDefaults: TurningMovementCountsChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const TurningMovementCountsChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: TurningMovementCountsChartOptionsProps) => {
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
      id="turning-movement-counts"
    />
  )
}
