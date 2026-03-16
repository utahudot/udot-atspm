import DropdownCell from '@/features/locations/components/dropdownCell'
import EastOutlinedIcon from '@mui/icons-material/EastOutlined'
import QuestionMarkIcon from '@mui/icons-material/HelpOutline'
import NorthEastOutlinedIcon from '@mui/icons-material/NorthEastOutlined'
import NorthOutlinedIcon from '@mui/icons-material/NorthOutlined'
import NorthWestOutlinedIcon from '@mui/icons-material/NorthWestOutlined'
import SouthEastOutlinedIcon from '@mui/icons-material/SouthEastOutlined'
import SouthOutlinedIcon from '@mui/icons-material/SouthOutlined'
import SouthWestOutlinedIcon from '@mui/icons-material/SouthWestOutlined'
import WestOutlinedIcon from '@mui/icons-material/WestOutlined'

export const staticDirectionTypes = {
  NA: {
    id: 'NA',
    icon: <QuestionMarkIcon />,
    chartSvg: null,
    description: 'Unknown',
  },
  NB: {
    id: 'NB',
    icon: <NorthOutlinedIcon />,
    chartSvg: { family: 'cardinal', rotationDeg: 0 },
    description: 'Northbound',
  },
  SB: {
    id: 'SB',
    icon: <SouthOutlinedIcon />,
    chartSvg: { family: 'cardinal', rotationDeg: 180 },
    description: 'Southbound',
  },
  EB: {
    id: 'EB',
    icon: <EastOutlinedIcon />,
    chartSvg: { family: 'cardinal', rotationDeg: 90 },
    description: 'Eastbound',
  },
  WB: {
    id: 'WB',
    icon: <WestOutlinedIcon />,
    chartSvg: { family: 'cardinal', rotationDeg: -90 },
    description: 'Westbound',
  },
  NE: {
    id: 'NE',
    icon: <NorthEastOutlinedIcon />,
    chartSvg: { family: 'diagonal', rotationDeg: 90 },
    description: 'Northeast',
  },
  NW: {
    id: 'NW',
    icon: <NorthWestOutlinedIcon />,
    chartSvg: { family: 'diagonal', rotationDeg: 0 },
    description: 'Northwest',
  },
  SE: {
    id: 'SE',
    icon: <SouthEastOutlinedIcon />,
    chartSvg: { family: 'diagonal', rotationDeg: 180 },
    description: 'Southeast',
  },
  SW: {
    id: 'SW',
    icon: <SouthWestOutlinedIcon />,
    chartSvg: { family: 'diagonal', rotationDeg: -90 },
    description: 'Southwest',
  },
} as const

interface DirectionTypeCellProps {
  value: string | null
  onUpdate: (id: string) => void
}

function DirectionTypeCell({ value, onUpdate }: DirectionTypeCellProps) {
  const directionOptions = Object.values(staticDirectionTypes).map((type) => ({
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
