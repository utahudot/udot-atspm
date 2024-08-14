import { Typography } from '@mui/material'

interface SubtitleProps {
  children: React.ReactNode
}

const Subtitle = ({ children }: SubtitleProps) => {
  return (
    <Typography fontSize={'13px'} color={'primary'} fontWeight={'bold'}>
      {children}
    </Typography>
  )
}

export default Subtitle
