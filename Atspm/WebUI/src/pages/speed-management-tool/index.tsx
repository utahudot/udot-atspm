import Header from '@/components/header'
import SM_Map from '@/features/speedManagementTool/components/SM_Map'
import Head from 'next/head'

const SpeedManagementTool = () => {
  const title = 'Speed Management Tool'
  return (
    <>
      <Head>
        <title>{title}</title>
      </Head>
      <Header title="Speed Management Tool" />
      <SM_Map />
    </>
  )
}

export default SpeedManagementTool
