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

const staticDirectionTypes = {
  NA: { id: 'NA', icon: <QuestionMarkIcon />, description: 'Unknown' },
  NB: { id: 'NB', icon: <NorthOutlinedIcon />, description: 'Northbound' },
  SB: { id: 'SB', icon: <SouthOutlinedIcon />, description: 'Southbound' },
  EB: { id: 'EB', icon: <EastOutlinedIcon />, description: 'Eastbound' },
  WB: { id: 'WB', icon: <WestOutlinedIcon />, description: 'Westbound' },
  NE: { id: 'NE', icon: <NorthEastOutlinedIcon />, description: 'Northeast' },
  NW: { id: 'NW', icon: <NorthWestOutlinedIcon />, description: 'Northwest' },
  SE: { id: 'SE', icon: <SouthEastOutlinedIcon />, description: 'Southeast' },
  SW: { id: 'SW', icon: <SouthWestOutlinedIcon />, description: 'Southwest' },
}

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
