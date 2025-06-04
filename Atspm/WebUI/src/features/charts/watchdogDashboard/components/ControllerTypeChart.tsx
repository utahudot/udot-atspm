import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  Box,
  FormControlLabel,
  Paper,
  Switch,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import React, { useEffect, useState } from 'react'
import transformWatchdogControllerTypeData from '../watchdogControllerType.transformer'
import SunburstLegend from './SunburstLegend'

interface ControllerTypeChartProps {
  data: any // Replace 'any' with the actual type of your controller type data
  isLoading: boolean
}

const ControllerTypeChart: React.FC<ControllerTypeChartProps> = ({
  data,
  isLoading,
}) => {
  const [controllerTypeData, setControllerTypeData] = useState<any>(null)
  const [controllerTypeLegend, setControllerTypeLegend] = useState<
    { name: string; color: string; selected: boolean }[]
  >([])
  const [deselectedControllerTypes, setDeselectedControllerTypes] = useState<
    string[]
  >([])
  const [showUnconfiguredData, setShowUnconfiguredData] = useState(false)
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))

  useEffect(() => {
    if (data) {
      const { sunburst, legendData } = transformWatchdogControllerTypeData(
        data,
        deselectedControllerTypes,
        showUnconfiguredData
      )
      setControllerTypeData(sunburst)
      setControllerTypeLegend(legendData)
    }
  }, [data, deselectedControllerTypes, showUnconfiguredData])

  const toggleControllerTypeLegend = (name: string) => {
    setDeselectedControllerTypes((prev) => {
      if (prev.includes(name)) {
        return prev.filter((item) => item !== name)
      } else {
        return [...prev, name]
      }
    })
  }

  const handleUnconfiguredDataToggle = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setShowUnconfiguredData(event.target.checked)
  }

  return (
    <Paper sx={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
      <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Device Type Breakdown</h2>
      </Box>

      <Box
        sx={{
          display: 'flex',
          flexDirection: isMobile ? 'column' : 'row',
          width: '100%',
          justifyContent: 'space-between',
        }}
      >
        <Box
          sx={{
            flex: isMobile ? 'none' : '0 0 80%',
            width: isMobile ? '100%' : '80%',
            paddingLeft: isMobile ? 0 : '15rem',
          }}
        >
          {!isLoading && controllerTypeData && controllerTypeData.series && (
            <ApacheEChart
              id="watchdog-controller-type-chart"
              option={controllerTypeData}
              loading={isLoading}
              style={{ width: '100%', height: '700px' }}
              hideInteractionMessage
            />
          )}
        </Box>
        <Box
          sx={{
            flex: isMobile ? 'none' : '0 0 20%',
            width: isMobile ? '100%' : '18%',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            paddingLeft: isMobile ? 0 : '33px',
            marginTop: isMobile ? '20px' : 0,
            marginBottom: '1rem',
          }}
        >
          <SunburstLegend
            legendData={controllerTypeLegend}
            onToggle={toggleControllerTypeLegend}
            isMobile={isMobile}
          />
          <Box
            sx={{
              marginTop: isMobile ? '5px' : '20px',
              display: 'flex',
              justifyContent: isMobile ? 'center' : 'flex-start',
              width: '100%',
            }}
          >
            {' '}
            <FormControlLabel
              control={
                <Switch
                  checked={showUnconfiguredData}
                  onChange={handleUnconfiguredDataToggle}
                  color="primary"
                />
              }
              label="Unconfigured issue types"
            />
          </Box>
        </Box>
      </Box>
    </Paper>
  )
}

export default ControllerTypeChart
