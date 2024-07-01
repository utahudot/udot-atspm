import { Typography } from '@mui/material'

interface styleComponentHeaderType {
  header: string
}
export const commonPaperStyle = {
  flexGrow: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
}

export const StyledComponentHeader = ({ header }: styleComponentHeaderType) => {
  return (
    <Typography
      sx={{
        padding: '15px',
        fontSize: '16px',
        fontWeight: 'bold',
        borderRadius: '5px 5px 0 0',
        width: '100%',
        textAlign: 'center',
      }}
    >
      {header}
    </Typography>
  )
}
