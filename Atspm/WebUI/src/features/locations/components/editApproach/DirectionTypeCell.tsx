import DropdownCell from '@/features/locations/components/dropdownCell'
import { directionTypes } from '@/features/locations/components/editDetector/selectOptions'

interface DirectionTypeCellProps {
  value: string | null
  onUpdate: (id: string) => void
}

function DirectionTypeCell({ value, onUpdate }: DirectionTypeCellProps) {
  const directionOptions = Object.values(directionTypes).map((type) => ({
    id: type.id,
    description: type.description,
    icon: type.icon,
  }))

  return (
    <DropdownCell
      options={directionOptions}
      value={value || 'NA'}
      onUpdate={onUpdate}
    />
  )
}

export default DirectionTypeCell
