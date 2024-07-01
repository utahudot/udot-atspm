import Locations from '@/pages/locations/index'
import '@fontsource/roboto/300.css'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'
import Head from 'next/head'

export default function Home(): React.ReactNode {
  return (
    <>
      <Head>
        <title>UDOT Automated Traffic Location Performance Measures</title>
        <meta
          name="description"
          content="Automated Traffic Location Performance Metrics show real-time and historical functionality at locationized intersections."
        />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <Locations />
    </>
  )
}
