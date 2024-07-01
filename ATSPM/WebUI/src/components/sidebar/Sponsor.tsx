import { Box, Typography, useTheme } from '@mui/material'
import Image from 'next/image'

export default function Sponsor() {
  const theme = useTheme()
  const colorMode = theme.palette.mode
  const imagePath = `/images/udot${colorMode === 'dark' ? '-white' : ''}.png`
  return (
    <Box sx={{ width: '60%' }}>
      <Typography
        fontStyle={'italic'}
        fontWeight={400}
        sx={{ padding: '10px' }}
      >
        Powered By
      </Typography>
      <Image
        alt="Sponsor"
        src={imagePath}
        width={0}
        height={0}
        sizes="100vw"
        style={{ width: '100%', height: 'auto' }}
      />
    </Box>
  )
}
