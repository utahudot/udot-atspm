import { ApproachDelayChartOptionsDefaults } from '@/features/charts/approachDelay/types'
import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { Default } from '@/features/charts/types'
import { SelectChangeEvent } from '@mui/material'
import { useState } from 'react'

interface ApproachDelayChartOptionsProps {
  chartDefaults: ApproachDelayChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const ApproachDelayChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: ApproachDelayChartOptionsProps) => {
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
      id="approach-delay"
    />
  )
}
