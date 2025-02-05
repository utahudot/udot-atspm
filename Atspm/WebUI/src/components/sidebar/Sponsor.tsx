import { getEnv } from '@/utils/getEnv'
import { Box, Typography } from '@mui/material'
import { useEffect, useState } from 'react'

export default function Sponsor() {
  const [imagePath, setImagePath] = useState('')

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      if (!env?.SPONSOR_IMAGE_URL) {
        return
      }
      setImagePath(env.SPONSOR_IMAGE_URL)
    }

    fetchEnv()
  }, [])

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
