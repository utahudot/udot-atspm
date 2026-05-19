import Layout from '@/components/layout'
import { RuntimeEnvProvider } from '@/contexts/RuntimeEnvContext'
import { FeatureFlagProvider } from '@/feature-flags/FeatureFlagContext'
import { initializeAxiosInstances } from '@/lib/axios'
import '@/styles/globals.css'
import { ColorModeContext, useMode } from '@/theme'
import { getEnv, type EnvVariables } from '@/utils/getEnv'
import '@fontsource/roboto/300.css'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'
import { CssBaseline, ThemeProvider } from '@mui/material'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFnsV3'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AppProps } from 'next/app'
import Head from 'next/head'
import { NuqsAdapter } from 'nuqs/adapters/next/pages'
import { useEffect, useState } from 'react'
import { Hydrate, QueryClient, QueryClientProvider } from 'react-query'
import { ReactQueryDevtools } from 'react-query/devtools'

export default function App({ Component, pageProps }: AppProps) {
  const [theme, colorMode] = useMode()
  const [queryClient] = useState(() => new QueryClient())
  const [isAxiosInitialized, setIsAxiosInitialized] = useState(false)
  const [runtimeEnv, setRuntimeEnv] = useState<EnvVariables | null>(null)
  const [initializationError, setInitializationError] = useState<string | null>(
    null
  )

  useEffect(() => {
    let cancelled = false

    const initialize = async () => {
      try {
        const env = await getEnv()
        await initializeAxiosInstances(env)

        if (!cancelled) {
          setRuntimeEnv(env)
          setIsAxiosInitialized(true)
        }
      } catch {
        if (!cancelled) {
          setInitializationError('Unable to load application configuration.')
        }
      }
    }

    initialize()

    return () => {
      cancelled = true
    }
  }, [])

  if (initializationError) {
    return (
      <AppBootstrapMessage title="Configuration Error" message={initializationError} />
    )
  }

  if (!isAxiosInitialized || !runtimeEnv) {
    return <AppBootstrapMessage title="Loading" />
  }

  return (
    <QueryClientProvider client={queryClient}>
      <RuntimeEnvProvider env={runtimeEnv}>
        <Hydrate state={pageProps.dehydratedState}>
          <LocalizationProvider dateAdapter={AdapterDateFns}>
            <NuqsAdapter>
              <FeatureFlagProvider>
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
              </FeatureFlagProvider>
            </NuqsAdapter>
          </LocalizationProvider>
        </Hydrate>
      </RuntimeEnvProvider>
    </QueryClientProvider>
  )
}

function AppBootstrapMessage({
  message,
  title,
}: {
  message?: string
  title: string
}) {
  return (
    <div
      style={{
        alignItems: 'center',
        display: 'flex',
        flexDirection: 'column',
        fontFamily: 'Roboto, Arial, sans-serif',
        gap: '8px',
        justifyContent: 'center',
        minHeight: '100vh',
      }}
    >
      <strong>{title}</strong>
      {message && <span>{message}</span>}
    </div>
  )
}
