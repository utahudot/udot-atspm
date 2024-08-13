import { Typography, useTheme } from '@mui/material'

interface HeaderProps {
  title: string
  subtitle?: string
}

export default function Header({ title, subtitle }: HeaderProps) {
  const theme = useTheme()
  return (
    <>
      <Typography variant="h2" component="h1" fontWeight="bold">
        {title}
      </Typography>
      {subtitle && (
        <Typography variant="h5" color={theme.palette.primary.light}>
          {subtitle}
        </Typography>
      )}
    </>
  )
}
