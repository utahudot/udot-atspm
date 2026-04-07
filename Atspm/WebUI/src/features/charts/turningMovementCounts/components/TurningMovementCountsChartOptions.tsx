import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { TurningMovementCountsChartOptionsDefaults } from '@/features/charts/turningMovementCounts/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import { Alert, Box, Checkbox, SelectChangeEvent, Typography } from '@mui/material'
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
  const [combineThruRight, setCombineThruRight] = useState(
    chartDefaults.combineThruRight?.value === 'TRUE'
  )

  const { setYAxisMaxStore } = useChartsStore()

  const [yAxisMax, setYAxisMax] = useState<string | null>(
    chartDefaults.yAxisDefault?.value
  )

  useEffect(() => {
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

  useEffect(() => {
    const defaultCombineThruRight = chartDefaults.combineThruRight?.value === 'TRUE'
    setCombineThruRight(defaultCombineThruRight)
  }, [chartDefaults.combineThruRight?.value])

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

  const handleCombineThruRightChange = (
    event: React.ChangeEvent<HTMLInputElement>,
    checked: boolean
  ) => {
    const value = checked ? 'TRUE' : 'FALSE'
    setCombineThruRight(checked)

    handleChartOptionsUpdate({
      id: chartDefaults.combineThruRight?.id ?? -1,
      option: chartDefaults.combineThruRight?.option ?? 'combineThruRight',
      value,
    })
  }

  const combineThruRightCheckbox = (
    <Checkbox
      checked={combineThruRight}
      onChange={handleCombineThruRightChange}
    />
  )

  const combineThruRightControl = (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
      }}
    >
      <Typography sx={{ color: 'black' }}>
        Combine Thru and Thru-Right
      </Typography>
      {combineThruRightCheckbox}
    </Box>
  )

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 1,
        flex: '1 1 0%',
        minHeight: 0,
      }}
    >
      <BinSizeDropdown
        value={binSize}
        handleChange={handleBinSizeChange}
        id="turning-movement-counts"
      />
      {!isMeasureDefaultView ? combineThruRightControl : null}
      <YAxisDefaultInput
        value={yAxisMax}
        handleChange={updateYAxisDefault}
        isMeasureDefaultView={isMeasureDefaultView}
      />
      {isMeasureDefaultView ? (
        chartDefaults.combineThruRight ? (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              mt: 1,
              mr: '29.3px',
            }}
          >
            <Typography sx={{ color: 'black' }}>
              Combine Thru and Thru-Right Default
            </Typography>
            {combineThruRightCheckbox}
          </Box>
        ) : (
          <Alert
            severity="error"
            sx={{
              mt: 2,
              '& .MuiAlert-message': {
                width: '100%',
              },
            }}
          >
            A Combine Thru and Thru-Right value is not found for this Measure
            Default.
          </Alert>
        )
      ) : null}
    </Box>
  )
}
