import SpeedManagementMap from '@/features/speedManagementTool/components/speedManagementMap'
import Head from 'next/head'

const SpeedManagementTool = () => {
  const title = 'Speed Management Tool'
  return (
    <>
      <Head>
        <title>{title}</title>
      </Head>
      <SpeedManagementMap />
    </>
  )
}

export default SpeedManagementTool
