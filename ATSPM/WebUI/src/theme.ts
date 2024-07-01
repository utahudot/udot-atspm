import { createTheme } from '@mui/material/styles'
import { createContext, useMemo, useState } from 'react'

type modeOptions = 'light' | 'dark'

export const themeSettings = (mode: modeOptions) => {
  return {
    palette: {
      mode: mode,
      ...(mode === 'dark'
        ? {
            primary: {
              light: '#6b93cc',
              main: '#5a87c6',
              dark: '#517ab2',
            },
            secondary: {
              light: '#0a203d',
              main: '#0b2444',
              dark: '#233a57',
            },
            accent: {
              light: '#777efb',
              main: '#6870fa',
              dark: '#5e65e1',
            },
            orange: {
              main: '#e86924',
              light: '#ea783a',
              dark: '#d15f20',
            },
            background: {
              default: '#141b2d',
              paper: '#1f2a40',
              highlight: '#1F2A40',
            },
          }
        : {
            primary: {
              light: '#0c6ecc',
              main: '#09549c',
              dark: '#063a6c',
            },
            secondary: {
              light: '#0a203d',
              main: '#0b2444',
              dark: '#233a57',
            },
            accent: {
              light: '#777efb',
              main: '#6870fa',
              dark: '#5e65e1',
            },
            orange: {
              main: '#e86924',
              light: '#ea783a',
              dark: '#d15f20',
            },
            background: {
              default: '#f9f9fb',
              paper: '#fff',
              highlight: '#eeeeee',
            },
          }),
    },
    typography: {
      // fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
      // fontSize: '1rem',
      h1: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 40,
      },
      h2: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 32,
      },
      h3: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 24,
      },
      h4: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 20,
      },
      h5: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 16,
      },
      h6: {
        fontFamily: ['Source Sans Pro', 'sans-serif'].join(','),
        fontSize: 14,
      },
    },
  }
}

export const ColorModeContext = createContext({
  toggleColorMode: () => {
    console.warn('toggleColorMode is not implemented.')
  },
})

export const useMode = () => {
  const [mode, setMode] = useState<modeOptions>('light')

  const colorMode = useMemo(
    () => ({
      toggleColorMode: () => {
        setMode((prev) => (prev === 'light' ? 'dark' : 'light'))
      },
    }),
    []
  )

  const theme = useMemo(() => createTheme(themeSettings(mode)), [mode])
  return [theme, colorMode] as const
}
