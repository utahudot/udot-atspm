// pages/_error.tsx
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import { NextPage, NextPageContext } from 'next'

interface ErrorProps {
  statusCode: number // statusCode is always a number
}

const ErrorPage: NextPage<ErrorProps> = ({ statusCode }) => {
  let message =
    statusCode === 500
      ? "Sorry! There's an internal server error."
      : 'An unexpected error occurred.'

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        textAlign: 'center',
      }}
    >
      <Typography variant="h1" component="h1" gutterBottom>
        Error {statusCode}
      </Typography>
      <Typography variant="body1" gutterBottom>
        {message}
      </Typography>
      <Button variant="contained" color="primary" href="/">
        Go Home
      </Button>
    </Box>
  )
}

ErrorPage.getInitialProps = async ({
  res,
  err,
}: NextPageContext): Promise<ErrorProps> => {
  const statusCode = res?.statusCode || err?.statusCode || 500 // Provide a default value
  return { statusCode }
}

export default ErrorPage
