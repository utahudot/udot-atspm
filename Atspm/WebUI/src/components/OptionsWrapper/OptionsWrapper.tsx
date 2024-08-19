import { Box, Paper, Typography } from '@mui/material'

interface OptionsWrapperProps {
  children: React.ReactNode
  header: string
}

function OptionsWrapper({ children, header }: OptionsWrapperProps) {
  return (
    <Paper>
      <Box
        sx={{
          p: 1,
          pl: 2,
          mb: 1,
          backgroundColor: '#f5f5f5',
          width: '100%',
          textAlign: 'left',
        }}
      >
        <Typography variant="subtitle2">{header}</Typography>
      </Box>
      <Box p={2}>{children}</Box>
    </Paper>
  )
}

export default OptionsWrapper
