import Header from '@/components/header'
import SpeedManagementMap from '@/features/speedManagementTool/components/speedManagementMap'
import { Box } from '@mui/material'
import Head from 'next/head'

const SpeedManagementTool = () => {
  const title = 'Speed Management Tool'
  return (
    <>
      <Head>
        <title>{title}</title>
      </Head>
      <Box sx={{ mb: 2 }}>
        <Header title="Speed Management Tool" />
      </Box>
      <SpeedManagementMap />
    </>
  )
}

export default SpeedManagementTool
