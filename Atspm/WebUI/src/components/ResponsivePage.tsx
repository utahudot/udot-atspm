import { useSidebarStore } from '@/stores/sidebar'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import { Box, IconButton, Tooltip, Typography, useTheme } from '@mui/material'
import Head from 'next/head'
import Header from './header'

interface ResponsivePageLayoutProps {
  children: React.ReactNode
  title: string
  subtitle?: string
  noBottomMargin?: boolean
  hideTitle?: boolean
  useFullWidth?: boolean
  width?: string | number
  hasRightSidebar?: boolean
}

export const ResponsivePageLayout = ({
  children,
  title,
  subtitle,
  noBottomMargin,
  hideTitle,
  useFullWidth,
  hasRightSidebar,
  width = '100%',
}: ResponsivePageLayoutProps) => {
  const theme = useTheme()
  const { toggleRightSidebar } = useSidebarStore()

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        margin: 'auto',
        width,
        maxWidth: useFullWidth ? 'inherit' : '1550px',
        minWidth: '375px',
        [theme.breakpoints.down('md')]: {},
      }}
    >
      <Head>
        <title>{`${title} - ATSPM`}</title>
      </Head>

      {!hideTitle && (
        <Box sx={{ mb: noBottomMargin ? 0 : 3 }}>
          {subtitle && (
            <Typography
              variant="overline"
              sx={{
                display: 'block',
                lineHeight: 1.2,
                mb: 0.5,
                color: 'text.secondary',
                letterSpacing: 0.6,
              }}
            >
              {subtitle}
            </Typography>
          )}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Header title={title} />
            {/* ✅ opens the sidebar */}
            {hasRightSidebar && (
              <Tooltip title="Page info">
                <IconButton
                  aria-label="Open page info"
                  onClick={toggleRightSidebar}
                  sx={{ mb: 2, p: 0 }}
                >
                  <InfoOutlinedIcon />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        </Box>
      )}

      {children}
    </Box>
  )
}
