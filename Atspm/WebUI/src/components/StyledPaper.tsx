// In StyledPaper.tsx
import Paper from '@mui/material/Paper'

export const StyledPaper = ({ children, sx }) => {
  return <Paper sx={{ ...sx }}>{children}</Paper>
}
