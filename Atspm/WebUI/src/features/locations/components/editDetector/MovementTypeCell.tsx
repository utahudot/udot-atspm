import DropdownCell from '@/features/locations/components/dropdownCell'
import ForkLeftOutlinedIcon from '@mui/icons-material/ForkLeftOutlined'
import ForkRightOutlinedIcon from '@mui/icons-material/ForkRightOutlined'
import NotInterestedOutlinedIcon from '@mui/icons-material/NotInterestedOutlined'
import StraightOutlinedIcon from '@mui/icons-material/StraightOutlined'
import TurnLeftOutlinedIcon from '@mui/icons-material/TurnLeftOutlined'
import TurnRightOutlinedIcon from '@mui/icons-material/TurnRightOutlined'

export const movementTypeOptions = [
  {
    id: 'NA',
    description: 'Unknown',
    icon: <NotInterestedOutlinedIcon />,
  },
  {
    id: 'L',
    description: 'Left',
    icon: <TurnLeftOutlinedIcon />,
  },
  {
    id: 'TL',
    description: 'Thru-Left',
    icon: <ForkLeftOutlinedIcon />,
  },
  {
    id: 'T',
    description: 'Thru',
    icon: <StraightOutlinedIcon />,
  },
  {
    id: 'TR',
    description: 'Thru-Right',
    icon: <ForkRightOutlinedIcon />,
  },
  {
    id: 'R',
    description: 'Right',
    icon: <TurnRightOutlinedIcon />,
  },
]

interface MovementTypeCellProps {
  value: string
  onUpdate: (value: string) => void
  width?: number | string
}

const MovementTypeCell = ({
  value,
  onUpdate,
  width,
}: MovementTypeCellProps) => {
  return (
    <DropdownCell
      options={movementTypeOptions}
      value={value}
      onUpdate={onUpdate}
      width={width}
    />
  )
}

export default MovementTypeCell
