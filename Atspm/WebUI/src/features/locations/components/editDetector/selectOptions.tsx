import DirectionsBikeOutlinedIcon from '@mui/icons-material/DirectionsBikeOutlined'
import DirectionsBusFilledOutlinedIcon from '@mui/icons-material/DirectionsBusFilledOutlined'
import DirectionsCarFilledOutlinedIcon from '@mui/icons-material/DirectionsCarFilledOutlined'
import DirectionsWalkOutlinedIcon from '@mui/icons-material/DirectionsWalkOutlined'
import EastOutlinedIcon from '@mui/icons-material/EastOutlined'
import ExitToAppOutlinedIcon from '@mui/icons-material/ExitToAppOutlined'
import ForkLeftOutlinedIcon from '@mui/icons-material/ForkLeftOutlined'
import ForkRightOutlinedIcon from '@mui/icons-material/ForkRightOutlined'
import QuestionMarkIcon from '@mui/icons-material/HelpOutline'
import NorthEastOutlinedIcon from '@mui/icons-material/NorthEastOutlined'
import NorthOutlinedIcon from '@mui/icons-material/NorthOutlined'
import NorthWestOutlinedIcon from '@mui/icons-material/NorthWestOutlined'
import NotInterestedOutlinedIcon from '@mui/icons-material/NotInterestedOutlined'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import SouthEastOutlinedIcon from '@mui/icons-material/SouthEastOutlined'
import SouthOutlinedIcon from '@mui/icons-material/SouthOutlined'
import SouthWestOutlinedIcon from '@mui/icons-material/SouthWestOutlined'
import StraightOutlinedIcon from '@mui/icons-material/StraightOutlined'
import TrainOutlinedIcon from '@mui/icons-material/TrainOutlined'
import TurnLeftOutlinedIcon from '@mui/icons-material/TurnLeftOutlined'
import TurnRightOutlinedIcon from '@mui/icons-material/TurnRightOutlined'
import WestOutlinedIcon from '@mui/icons-material/WestOutlined'

export const movementType = [
  { id: 'NA', description: 'Unknown', icon: <NotInterestedOutlinedIcon /> },
  { id: 'L', description: 'Left', icon: <TurnLeftOutlinedIcon /> },
  { id: 'TL', description: 'Thru-Left', icon: <ForkLeftOutlinedIcon /> },
  { id: 'T', description: 'Thru', icon: <StraightOutlinedIcon /> },
  { id: 'TR', description: 'Thru-Right', icon: <ForkRightOutlinedIcon /> },
  { id: 'R', description: 'Right', icon: <TurnRightOutlinedIcon /> },
]

export const hardwareTypes = [
  { id: 'NA', description: 'Unknown' },
  { id: 'WavetronixMatrix', description: 'Wavetronix Matrix' },
  { id: 'WavetronixAdvance', description: 'Wavetronix Advance' },
  { id: 'InductiveLoops', description: 'Inductive Loops' },
  { id: 'Sensys', description: 'Sensys' },
  { id: 'Video', description: 'Video' },
  { id: 'FLIRThermalCamera', description: 'FLIR: Thermal Camera' },
  { id: 'LiDar', description: 'LiDAR' },
]

export const laneTypes = [
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

export const directionTypes = {
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

export const hardwareTypeOptions = hardwareTypes.map((ht) => ({
  value: ht.id,
  label: ht.description,
}))

export const movementTypeOptions = movementType.map((mt) => ({
  value: mt.id,
  label: mt.description,
  icon: mt.icon,
}))

export const laneTypeOptions = laneTypes.map((lt) => ({
  value: lt.id,
  label: lt.description,
  icon: lt.icon,
}))

export const directionTypeOptions = Object.values(directionTypes).map((dt) => ({
  value: dt.id,
  label: dt.description,
  icon: dt.icon,
}))
