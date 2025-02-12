import { Alert, Box, Checkbox, FormControl, Typography } from '@mui/material'
import { useState } from 'react'
import { Default } from '../../types'
import { RampMeteringChartOptionsDefaults } from '../types'

interface GreenTimeUtilizationChartOptionsProps {
  chartDefaults: RampMeteringChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const RampMeteringChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: GreenTimeUtilizationChartOptionsProps) => {
  const [combineLanes, setCombineLanes] = useState(
    chartDefaults.combineLanes.value
  )

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement>,
    checked: boolean
  ) => {
    const combineLanesValue = checked
    const val = combineLanesValue === true ? 'TRUE' : 'FALSE'
    setCombineLanes(val)

    handleChartOptionsUpdate({
      id: chartDefaults.combineLanes.id,
      option: chartDefaults.combineLanes.option,
      value: val,
    })
  }

  const visuallyHidden = {
    position: 'absolute',
    width: '1px',
    height: '1px',
    padding: 0,
    margin: '-1px',
    overflow: 'hidden',
    clip: 'rect(0, 0, 0, 0)',
    border: 0,
  }

  return (
    <>
      {combineLanes === undefined ? (
        <Alert severity="error" sx={{ mt: 1 }}>
          Combine Lanes default value not found.
        </Alert>
      ) : (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
          }}
        >
          <Typography>Combine Lanes</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <FormControl sx={{ width: '60px' }}>
              <label htmlFor="combine-lanes" style={visuallyHidden}>
                Combine Lanes
              </label>
              <Checkbox
                checked={combineLanes === 'TRUE' ? true : false}
                onChange={handleChange}
              />
            </FormControl>
          </Box>
        </Box>
      )}
    </>
  )
}
