import Header from '@/components/header'
import SM_Map from '@/features/speedManagementTool/components/SM_Map'
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
      <SM_Map />
    </>
  )
}

export default SpeedManagementTool
