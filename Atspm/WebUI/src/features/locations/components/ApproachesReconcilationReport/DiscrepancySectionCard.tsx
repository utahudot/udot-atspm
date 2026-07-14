import { Box, Paper, Typography } from '@mui/material'
import React from 'react'

export default function DiscrepancySectionCard({
  title,
  actions,
  children,
}: {
  title: string
  actions?: React.ReactNode
  children: React.ReactNode
}) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Box
        display="flex"
        alignItems="center"
        gap={1}
        flexWrap="wrap"
        sx={{ mb: 1 }}
      >
        <Typography variant="subtitle1" sx={{ marginRight: 'auto' }}>
          {title}
        </Typography>
        {actions}
      </Box>
      {children}
    </Paper>
  )
}
