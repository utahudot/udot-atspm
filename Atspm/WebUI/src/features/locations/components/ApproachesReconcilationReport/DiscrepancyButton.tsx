// DiscrepancyButton.tsx
import CheckBoxIcon from '@mui/icons-material/CheckBox'
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank'
import { Button } from '@mui/material'
import { useTheme } from '@mui/material/styles'

export interface DiscrepancyButtonProps {
  item: { id: string | number; label: string | number }
  selected?: boolean
  onToggle: () => void
  width?: number
}

const DiscrepancyButton = ({
  item,
  selected,
  onToggle,
  width,
}: DiscrepancyButtonProps) => {
  const theme = useTheme()

  return (
    <Button
      variant="outlined"
      onClick={onToggle}
      size="small"
      startIcon={
        selected ? (
          <CheckBoxIcon fontSize="small" color="info" />
        ) : (
          <CheckBoxOutlineBlankIcon fontSize="small" />
        )
      }
      sx={{
        width: width ?? 60,
        margin: 0.5,
        justifyContent: 'flex-start',
        color: theme.palette.grey[700],
        borderColor: selected
          ? theme.palette.primary.main
          : theme.palette.grey[700],
        backgroundColor: selected
          ? theme.palette.action.selected
          : 'transparent',
        '& .MuiButton-startIcon': { marginRight: 0.75 },
      }}
      disableElevation
    >
      {item.label}
    </Button>
  )
}

export default DiscrepancyButton
