import Layout from '@/components/layout'
import { initializeAxiosInstances } from '@/lib/axios'
import '@/styles/globals.css'
import { ColorModeContext, useMode } from '@/theme'
import '@fontsource/roboto/300.css'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'
import { CssBaseline, ThemeProvider } from '@mui/material'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFnsV3'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AppProps } from 'next/app'
import Head from 'next/head'
import { useEffect, useState } from 'react'
import { Hydrate, QueryClient, QueryClientProvider } from 'react-query'
import { ReactQueryDevtools } from 'react-query/devtools'

export default function App({ Component, pageProps }: AppProps) {
  const [theme, colorMode] = useMode()
  const [queryClient] = useState(() => new QueryClient())
  const [isAxiosInitialized, setIsAxiosInitialized] = useState(false)

  useEffect(() => {
    const initialize = async () => {
      await initializeAxiosInstances()
      setIsAxiosInitialized(true)
    }
    initialize()
  }, [])

  if (!isAxiosInitialized) {
    return null
  }

  return (
    <QueryClientProvider client={queryClient}>
      <Hydrate state={pageProps.dehydratedState}>
        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <ColorModeContext.Provider value={colorMode}>
            <ThemeProvider theme={theme}>
              <Layout>
                <CssBaseline />
                <Head>
                  <meta
                    name="viewport"
                    content="width=device-width, minimum-scale=1, maximum-scale=5"
                  />
                </Head>
                <Component {...pageProps} />
                {process.env.NODE_ENV === 'development' && (
                  <ReactQueryDevtools initialIsOpen={false} />
                )}
              </Layout>
            </ThemeProvider>
          </ColorModeContext.Provider>
        </LocalizationProvider>
      </Hydrate>
    </QueryClientProvider>
  )
}
