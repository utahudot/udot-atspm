import AddIcon from '@mui/icons-material/Add'
import { Button, ButtonProps } from '@mui/material'

interface AddButtonProps extends ButtonProps {
  label: string
  onClick: () => void
}

const AddButton = ({ label, onClick, ...rest }: AddButtonProps) => {
  return (
    <Button
      name={label}
      variant="contained"
      color="success"
      startIcon={<AddIcon />}
      onClick={onClick}
      {...rest}
    >
      {label}
    </Button>
  )
}

export default AddButton
