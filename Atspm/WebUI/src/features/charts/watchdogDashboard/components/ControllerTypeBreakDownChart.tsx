import React, { useEffect, useState } from 'react'
import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import transformWatchdogControllerTypeData from './watchdogControllerType.transformer'
import { FormControlLabel, Switch } from '@mui/material'

interface ControllerTypeBreakdownChartProps {
  data: any // Replace 'any' with the actual type of your controller type data
  isLoading: boolean
}

const ControllerTypeBreakdownChart: React.FC<ControllerTypeBreakdownChartProps> = ({ data, isLoading }) => {
  const [controllerTypeData, setControllerTypeData] = useState<any>(null)
  const [controllerTypeLegend, setControllerTypeLegend] = useState<
    { name: string; color: string; selected: boolean }[]
  >([])
  const [deselectedControllerTypes, setDeselectedControllerTypes] = useState<string[]>([])
  const [showUnconfiguredData, setShowUnconfiguredData] = useState(false)

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
    setDeselectedControllerTypes(prev => {
      if (prev.includes(name)) {
        return prev.filter(item => item !== name)
      } else {
        return [...prev, name]
      }
    })
  }

  const handleUnconfiguredDataToggle = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowUnconfiguredData(event.target.checked)
  }

  return (
    <>
      <div style={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Controller Type Breakdown</h2>
      </div>

      <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center', marginBottom: '10px' }}>
        {controllerTypeLegend.map((item) => (
          <div
            key={item.name}
            style={{
              cursor: 'pointer',
              margin: '0 10px',
              display: 'flex',
              alignItems: 'center',
              opacity: item.selected ? 1 : 0.5,
            }}
            onClick={() => toggleControllerTypeLegend(item.name)}
          >
            <span
              style={{
                display: 'inline-block',
                width: '25px',
                height: '14px',
                borderRadius: '15%',
                backgroundColor: item.color,
                marginRight: '5px',
              }}
            ></span>
            {item.name}
          </div>
        ))}
              <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '10px' }}>
        <FormControlLabel
          control={
            <Switch
              checked={showUnconfiguredData}
              onChange={handleUnconfiguredDataToggle}
              color="primary"
            />
          }
          label="Unconfigured Data"
        />
      </div>
      </div>
      {!isLoading && controllerTypeData && controllerTypeData.series && (
        <ApacheEChart
          id="watchdog-controller-type-chart"
          option={controllerTypeData}
          loading={isLoading}
          style={{ width: '100%', height: '100%' }}
          hideInteractionMessage
        />
      )}
    </>
  )
}

export default ControllerTypeBreakdownChart