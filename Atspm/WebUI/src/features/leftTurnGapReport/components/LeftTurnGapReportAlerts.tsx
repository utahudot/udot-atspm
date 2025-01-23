// components/LeftTurnGapReportAlerts.tsx
import { Alert, Box, CircularProgress } from '@mui/material'
import React from 'react'

interface LeftTurnGapReportAlertsProps {
  isLoading: boolean
  errorMessages: string[]
}

const LeftTurnGapReportAlerts: React.FC<LeftTurnGapReportAlertsProps> = ({
  isLoading,
  errorMessages,
}) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {isLoading && (
        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
          <CircularProgress size={24} />
        </Box>
      )}
      {errorMessages.map((errorMessage, index) => (
        <Alert severity="error" key={index}>
          {errorMessage}
        </Alert>
      ))}
      <Alert severity="warning">
        It is always good practice to review the Split Pattern performance in
        conjunction with using this report
      </Alert>
    </Box>
  )
}

export default LeftTurnGapReportAlerts
