import DropdownCell from '@/features/locations/components/dropdownCell'
import { hardwareTypes } from '@/features/locations/components/editDetector/selectOptions'

interface HardwareTypeCellProps {
  value: string
  onUpdate: (id: string) => void
}

function HardwareTypeCell({ value, onUpdate }: HardwareTypeCellProps) {
  return (
    <DropdownCell options={hardwareTypes} value={value} onUpdate={onUpdate} />
  )
}

export default HardwareTypeCell
