import { ArrivalsOnRedChartOptionsDefaults } from '@/features/charts/arrivalsOnRed/types'
import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { Default } from '@/features/charts/types'
import { Alert, SelectChangeEvent } from '@mui/material'
import { useState } from 'react'

interface ArrivalsOnRedChartOptionsProps {
  chartDefaults: ArrivalsOnRedChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const ArrivalsOnRedChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: ArrivalsOnRedChartOptionsProps) => {
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
    <>
      {binSize === undefined ? (
        <Alert severity="error" sx={{ mt: 1 }}>
          Bin Size default value not found.
        </Alert>
      ) : (
        <BinSizeDropdown
          value={binSize}
          handleChange={handleBinSizeChange}
          id="arrivals-on-red"
        />
      )}
    </>
  )
}
