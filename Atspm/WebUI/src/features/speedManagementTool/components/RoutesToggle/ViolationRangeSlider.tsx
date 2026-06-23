import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, Slider } from '@mui/material'
import { useEffect, useState } from 'react'
import { isViolationRenderOption } from '@/features/speedManagementTool/enums'
import { DataSource } from '@/features/speedManagementTool/enums'

export const ViolationColors = {
  Low: '#abc746',
  Medium: '#09549c',
  High: '#f5a31b',
}

export default function ViolationRangeSlider() {
  const {
    mediumMin,
    mediumMax,
    setMediumMin,
    setMediumMax,
    sliderMin,
    sliderMax,
    routeRenderOption,
    submittedRouteSpeedRequest,
  } = useStore()
  const values = [mediumMin, mediumMax]
  const [value, setValue] = useState(values)
  const [perc, setPerc] = useState(values.map((val) => (val / sliderMax) * 100))

  useEffect(() => {
    setValue([mediumMin, mediumMax])
    setPerc([mediumMin, mediumMax].map((val) => (val / sliderMax) * 100))
  }, [mediumMin, mediumMax, sliderMax])

  const onChange = (e, tValues) => {
    const [minVal, maxVal] = tValues
    setMediumMin(minVal)
    setMediumMax(maxVal)
    if (maxVal > minVal && maxVal !== minVal) {
      setValue(tValues)
      setPerc(tValues.map((val: number) => (val / sliderMax) * 100))
    }
  }

  const isViolation = isViolationRenderOption(routeRenderOption)
  const isNotClearGuide =
    submittedRouteSpeedRequest.sourceId !== DataSource.ClearGuide

  return (
    <Box
      sx={{
        width: '80%',
        mx: 2,
        mt: 1,
        mb: 2,
        display: isViolation && isNotClearGuide ? 'block' : 'none',
      }}
    >
      <Slider
        disableSwap={false}
        sx={{
          '& .MuiSlider-track': {
            background: ViolationColors.Medium,
            borderColor: 'white',
          },
          '& .MuiSlider-thumb': {
            [`&:nth-of-type(${1}n)`]: {
              background: 'white',
              border: '2px solid grey',
            },
          },
          '& .MuiSlider-valueLabel': {
            top: 30,
            minWidth: 28,
            height: 26,
            px: 0.75,
            fontSize: 12,
            lineHeight: '26px',
            overflow: 'visible',
            transform: 'translateY(0) scale(1)',
          },
          '& .MuiSlider-valueLabel::before': {
            width: 10,
            height: 10,
            top: 0,
            bottom: 'auto',
            transform: 'translate(-50%, -50%) rotate(45deg)',
            zIndex: 0,
          },
          '& .MuiSlider-valueLabelLabel': {
            position: 'relative',
            zIndex: 1,
          },
          '& .MuiSlider-valueLabel.MuiSlider-valueLabelOpen': {
            transform: 'translateY(0) scale(1)',
          },
          '& .MuiSlider-mark': {
            background: 'none',
          },
          '& .MuiSlider-rail': {
            background: `linear-gradient(to right, ${ViolationColors.Low} 0% ${perc[0]}%, ${ViolationColors.Medium} ${perc[0]}% ${perc[1]}%, ${ViolationColors.High} ${perc[1]}% 100%)`,
            opacity: 1,
          },
        }}
        valueLabelDisplay="on"
        value={value}
        min={sliderMin}
        max={sliderMax}
        onChange={onChange}
      />
    </Box>
  )
}
