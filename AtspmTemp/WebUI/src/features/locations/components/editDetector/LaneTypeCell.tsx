import DropdownCell from '@/features/locations/components/dropdownCell'

import DirectionsBikeOutlinedIcon from '@mui/icons-material/DirectionsBikeOutlined'
import DirectionsBusFilledOutlinedIcon from '@mui/icons-material/DirectionsBusFilledOutlined'
import DirectionsCarFilledOutlinedIcon from '@mui/icons-material/DirectionsCarFilledOutlined'
import DirectionsWalkOutlinedIcon from '@mui/icons-material/DirectionsWalkOutlined'
import ExitToAppOutlinedIcon from '@mui/icons-material/ExitToAppOutlined'
import NotInterestedOutlinedIcon from '@mui/icons-material/NotInterestedOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import TrainOutlinedIcon from '@mui/icons-material/TrainOutlined'

export const laneTypeOptions = [
  {
    id: 'NA',
    description: 'Unknown',
    abbreviation: 'NA',
    icon: <NotInterestedOutlinedIcon />,
  },
  {
    id: 'V',
    description: 'Vehicle',
    abbreviation: 'V',
    icon: <DirectionsCarFilledOutlinedIcon />,
  },
  {
    id: 'Bike',
    description: 'Bike',
    abbreviation: 'Bike',
    icon: <DirectionsBikeOutlinedIcon />,
  },
  {
    id: 'Ped',
    description: 'Pedestrian',
    abbreviation: 'Ped',
    icon: <DirectionsWalkOutlinedIcon />,
  },
  {
    id: 'E',
    description: 'Exit',
    abbreviation: 'E',
    icon: <ExitToAppOutlinedIcon />,
  },
  {
    id: 'LRT',
    description: 'Light Rail Transit',
    abbreviation: 'LRT',
    icon: <TrainOutlinedIcon />,
  },
  {
    id: 'Bus',
    description: 'Bus',
    abbreviation: 'Bus',
    icon: <DirectionsBusFilledOutlinedIcon />,
  },
  {
    id: 'HDV',
    description: 'High Occupancy Vehicle',
    abbreviation: 'HDV',
    icon: <PeopleAltOutlinedIcon />,
  },
]

interface LaneTypeCellProps {
  value: string
  onUpdate: (value: string) => void
}

function LaneTypeCell({ value, onUpdate }: LaneTypeCellProps) {
  return (
    <DropdownCell options={laneTypeOptions} value={value} onUpdate={onUpdate} />
  )
}

export default LaneTypeCell
