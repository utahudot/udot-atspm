import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { Default } from '@/features/charts/types'
import { Alert, SelectChangeEvent } from '@mui/material'
import { useState } from 'react'

interface PrioritySummaryChartOptionsProps {
  chartDefaults: any
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const PrioritySummaryChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PrioritySummaryChartOptionsProps) => {
  const [binSize, setBinSize] = useState(15)

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
      {binSize === undefined ? (
        <Alert severity="error" sx={{ mt: 1 }}>
          Bin Size default value not found.
        </Alert>
      ) : (
        <BinSizeDropdown
          value={binSize}
          handleChange={handleBinSizeChange}
          id="approach-volume"
        />
      )}
    </>
  )
}
