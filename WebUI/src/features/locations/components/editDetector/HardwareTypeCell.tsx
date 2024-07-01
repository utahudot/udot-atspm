import DropdownCell from '@/features/locations/components/dropdownCell'

export const hardwareTypeOptions = [
  { id: 'NA', description: 'Unknown' },
  { id: 'WavetronixMatrix', description: 'Wavetronix Matrix' },
  { id: 'WavetronixAdvance', description: 'Wavetronix Advance' },
  { id: 'InductiveLoops', description: 'Inductive Loops' },
  { id: 'Sensys', description: 'Sensys' },
  { id: 'Video', description: 'Video' },
  { id: 'FLIRThermalCamera', description: 'FLIR: Thermal Camera' },
]

interface HardwareTypeCellProps {
  value: string
  onUpdate: (id: string) => void
}

function HardwareTypeCell({ value, onUpdate }: HardwareTypeCellProps) {
  return (
    <DropdownCell
      options={hardwareTypeOptions}
      value={value}
      onUpdate={onUpdate}
    />
  )
}

export default HardwareTypeCell
