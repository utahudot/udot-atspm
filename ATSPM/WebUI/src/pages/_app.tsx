import Layout from '@/components/layout'
import { MOCKING_ENABLED } from '@/config'
import '@/styles/globals.css'
import { ColorModeContext, useMode } from '@/theme'
import '@fontsource/roboto/300.css'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'
import { CssBaseline, ThemeProvider } from '@mui/material'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AppProps } from 'next/app'
import Head from 'next/head'
import { useState } from 'react'
import { Hydrate, QueryClient, QueryClientProvider } from 'react-query'

if (MOCKING_ENABLED) {
  import('@/mocks').then(({ initMocks }) => initMocks())
}

export default function App({ Component, pageProps }: AppProps) {
  const [theme, colorMode] = useMode()
  const [queryClient] = useState(() => new QueryClient())

  return (
    <QueryClientProvider client={queryClient}>
      <Hydrate state={pageProps.dehydratedState}>
        <LocalizationProvider dateAdapter={AdapterDateFns}>
          <ColorModeContext.Provider value={colorMode}>
            <ThemeProvider theme={theme}>
              <Layout>
                <div>
                  <CssBaseline />
                  <Head>
                    <meta
                      name="viewport"
                      content="width=device-width, minimum-scale=1, maximum-scale=5"
                    />
                  </Head>
                  <Component {...pageProps} />
                </div>
              </Layout>
            </ThemeProvider>
          </ColorModeContext.Provider>
        </LocalizationProvider>
      </Hydrate>
    </QueryClientProvider>
  )
}
