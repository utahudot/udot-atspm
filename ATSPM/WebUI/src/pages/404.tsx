// pages/404.tsx
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import React from 'react'

const Custom404: React.FC = () => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        height: '55vh',
        textAlign: 'center',
      }}
    >
      <Typography variant="h1" component="h1" gutterBottom>
        404 - Page Not Found
      </Typography>
      <Typography variant="body1" gutterBottom>
        Oops! The page you're looking for doesn't exist.
      </Typography>
      <Button
        sx={{ marginTop: '25px' }}
        variant="contained"
        color="primary"
        href="/"
      >
        Go Home
      </Button>
    </Box>
  )
}

export default Custom404
