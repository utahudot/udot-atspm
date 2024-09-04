import React from 'react'

interface LegendItem {
  name: string
  color: string
  selected: boolean
}

interface ChartLegendProps {
  legendData: LegendItem[]
  onToggle: (name: string) => void
}

const SunburstLegend: React.FC<ChartLegendProps> = ({ legendData, onToggle }) => {
  return (
    <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center', marginBottom: '10px' }}>
      {legendData.map((item) => (
        <div
          key={item.name}
          style={{
            cursor: 'pointer',
            margin: '0 10px',
            display: 'flex',
            alignItems: 'center',
            opacity: item.selected ? 1 : 0.5,
          }}
          onClick={() => onToggle(item.name)}
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
    </div>
  )
}

export default SunburstLegend