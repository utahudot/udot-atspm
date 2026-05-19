import { useEnv } from '@/hooks/useEnv'
import { Box, Typography } from '@mui/material'

export default function Sponsor() {
  const { data: env } = useEnv()

  const imagePath = env?.POWERED_BY_IMAGE_URL || ''

  if (!imagePath) return null

  return (
    <Box sx={{ width: '200px' }}>
      <Typography
        fontStyle={'italic'}
        fontWeight={400}
        sx={{ padding: '10px' }}
      >
        Powered By
      </Typography>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img alt="Sponsor" src={imagePath} style={{ width: '200px' }} />
    </Box>
  )
}
