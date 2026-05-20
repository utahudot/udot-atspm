import type { Device } from '@/api/config'
import type { DeviceEventDownload } from '@/api/data'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import LanIcon from '@mui/icons-material/Lan'
import RadioButtonUncheckedIcon from '@mui/icons-material/RadioButtonUnchecked'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Modal,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'

type CombinedDeviceEvent = Device & Partial<DeviceEventDownload>
type DeviceWithFirmware = Device & { firmware?: string | null }
type VerificationStage = 'idle' | 'saving' | 'checking'

interface DevicesWizardModalProps {
  open: boolean
  onClose: () => void
  onSaveAndClose: () => void
  devices: CombinedDeviceEvent[] | undefined
  onResync: () => void
  isResyncing: boolean
  verificationStage?: VerificationStage
  ipChanges: Record<number, string>
  setIpChanges: React.Dispatch<React.SetStateAction<Record<number, string>>>
}

const getDeviceName = (device: Device) => {
  const product = device.deviceConfiguration?.product
  const productName = [product?.manufacturer, product?.model]
    .filter(Boolean)
    .join(' - ')
  const firmware = (device as DeviceWithFirmware).firmware
  const deviceName = [productName, firmware].filter(Boolean).join(' ')

  return deviceName || `Device ${device.id ?? ''}`.trim()
}

const formatCount = (count?: number) =>
  typeof count === 'number' && count >= 0 ? count.toLocaleString() : 'Pending'

const getVerificationStatus = (
  device: CombinedDeviceEvent,
  options: {
    hasPendingIpChange: boolean
    isCheckingEvents: boolean
    isSavingIpChanges: boolean
  }
) => {
  const beforeWorkflowEventCount = device.beforeWorkflowEventCount
  const afterWorkflowEventCount = device.afterWorkflowEventCount
  const changeInEventCount = device.changeInEventCount
  const { hasPendingIpChange, isCheckingEvents, isSavingIpChanges } = options

  if (device.loggingEnabled === false) {
    return {
      detail: 'Skipped because logging is disabled for this device.',
      icon: <RadioButtonUncheckedIcon color="disabled" fontSize="small" />,
      label: 'Logging disabled',
    }
  }

  if (isSavingIpChanges && hasPendingIpChange) {
    return {
      detail: 'Updating the saved IP address before verification.',
      icon: <CircularProgress size={18} />,
      label: 'Saving IP',
    }
  }

  if (isCheckingEvents) {
    return {
      detail: 'Connecting to the device and downloading recent event logs.',
      icon: <CircularProgress size={18} />,
      label: 'Downloading logs',
    }
  }

  const hasVerificationResult =
    typeof beforeWorkflowEventCount === 'number' &&
    typeof afterWorkflowEventCount === 'number' &&
    typeof changeInEventCount === 'number'

  if (!hasVerificationResult) {
    return {
      detail: 'Run verification to check this device.',
      icon: <RadioButtonUncheckedIcon color="disabled" fontSize="small" />,
      label: 'Pending',
    }
  }

  const hasConnectionFailure =
    beforeWorkflowEventCount < 0 ||
    afterWorkflowEventCount < 0 ||
    changeInEventCount < 0

  if (hasConnectionFailure) {
    return {
      detail: 'Unable to connect or download logs. Confirm the IP address and network route.',
      icon: <ErrorOutlineIcon color="error" fontSize="small" />,
      label: 'Unable to connect',
    }
  }

  if (beforeWorkflowEventCount === 0 && changeInEventCount === 0) {
    return {
      detail: 'Connected, but no new logs were found.',
      icon: <CheckCircleOutlineIcon color="success" fontSize="small" />,
      label: 'Connected',
    }
  }

  const details: string[] = []
  if (beforeWorkflowEventCount > 0) {
    details.push(
      `${beforeWorkflowEventCount.toLocaleString()} existing rows found.`
    )
  }
  if (changeInEventCount > 0) {
    details.push(`${changeInEventCount.toLocaleString()} new rows downloaded.`)
  }

  if (beforeWorkflowEventCount > 0 || changeInEventCount > 0) {
    return {
      detail: details.join(' '),
      icon: <CheckCircleOutlineIcon color="success" fontSize="small" />,
      label: 'Connected',
    }
  }

  return {
    detail: '',
    icon: <ErrorOutlineIcon color="error" fontSize="small" />,
    label: 'Unable to verify',
  }
}

const renderCount = (
  count: number | undefined,
  options: {
    isCheckingEvents: boolean
    isLoggingDisabled: boolean
  }
) => {
  if (options.isLoggingDisabled) {
    return (
      <Typography variant="body2" color="text.secondary">
        Skipped
      </Typography>
    )
  }

  if (options.isCheckingEvents) {
    return <Skeleton variant="text" width={56} sx={{ ml: 'auto' }} />
  }

  const hasFailure = typeof count === 'number' && count < 0

  return (
    <Typography
      variant="body2"
      color={hasFailure ? 'text.secondary' : 'text.primary'}
    >
      {hasFailure ? '-' : formatCount(count)}
    </Typography>
  )
}

const DevicesWizardModal = ({
  open,
  onClose,
  onSaveAndClose,
  devices,
  onResync,
  isResyncing,
  verificationStage = 'idle',
  ipChanges,
  setIpChanges,
}: DevicesWizardModalProps) => {
  const deviceList = devices ?? []
  const hasDevices = deviceList.length > 0
  const isSavingIpChanges = verificationStage === 'saving'
  const isCheckingEvents = verificationStage === 'checking'
  const loggingEnabledCount = deviceList.filter(
    (device) => device.loggingEnabled !== false
  ).length
  const skippedCount = deviceList.length - loggingEnabledCount
  const failedCount =
    isResyncing
      ? 0
      : deviceList.filter(
          (device) =>
            device.loggingEnabled !== false &&
            ((device.beforeWorkflowEventCount ?? 0) < 0 ||
              (device.afterWorkflowEventCount ?? 0) < 0 ||
              (device.changeInEventCount ?? 0) < 0)
        ).length
  const connectedCount =
    isResyncing
      ? 0
      : deviceList.filter((device) => {
          const hasVerificationResult =
            typeof device.beforeWorkflowEventCount === 'number' &&
            typeof device.afterWorkflowEventCount === 'number' &&
            typeof device.changeInEventCount === 'number'

          return (
            device.loggingEnabled !== false &&
            hasVerificationResult &&
            (device.beforeWorkflowEventCount ?? -1) >= 0 &&
            (device.afterWorkflowEventCount ?? -1) >= 0 &&
            (device.changeInEventCount ?? -1) >= 0
          )
        }).length
  const pendingCount =
    isResyncing
      ? loggingEnabledCount
      : deviceList.filter((device) => {
          const hasVerificationResult =
            typeof device.beforeWorkflowEventCount === 'number' &&
            typeof device.afterWorkflowEventCount === 'number' &&
            typeof device.changeInEventCount === 'number'

          return device.loggingEnabled !== false && !hasVerificationResult
        }).length

  const actionLabel =
    verificationStage === 'saving'
      ? 'Saving IP addresses'
      : verificationStage === 'checking'
        ? 'Verifying devices'
        : 'Verify IP Addresses'

  const handleIpChange = (deviceId: number, newIp: string) => {
    setIpChanges((prev) => ({ ...prev, [deviceId]: newIp }))
  }

  return (
    <Modal
      open={open}
      onClose={onClose}
      aria-labelledby="device-verification-title"
    >
      <Paper
        sx={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          width: '90%',
          maxWidth: 1100,
          maxHeight: '90vh',
          overflowY: 'auto',
          p: 4,
          borderRadius: 2,
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            gap: 2,
            mb: 2,
          }}
        >
          <Box>
            <Typography
              id="device-verification-title"
              variant="h6"
              sx={{ fontWeight: 600 }}
            >
              Verify Device IP Addresses
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {isResyncing
                ? `${actionLabel} for ${loggingEnabledCount} device${loggingEnabledCount === 1 ? '' : 's'}`
                : `${loggingEnabledCount} device${loggingEnabledCount === 1 ? '' : 's'} ready to verify`}
            </Typography>
          </Box>

          <Stack
            direction="row"
            spacing={1}
            useFlexGap
            flexWrap="wrap"
            sx={{ justifyContent: 'flex-end' }}
          >
            <Chip
              size="small"
              color="success"
              variant="outlined"
              label={`${connectedCount} connected`}
            />
            <Chip
              size="small"
              color={isResyncing ? 'primary' : 'default'}
              variant="outlined"
              label={`${pendingCount} ${isResyncing ? 'in progress' : 'pending'}`}
            />
            <Chip
              size="small"
              color={failedCount > 0 ? 'error' : 'default'}
              variant="outlined"
              label={`${failedCount} failed`}
            />
            {skippedCount > 0 && (
              <Chip
                size="small"
                color="default"
                variant="outlined"
                label={`${skippedCount} skipped`}
              />
            )}
          </Stack>
        </Box>

        <Alert severity="info" sx={{ mb: 3 }}>
          Checks each device connection and downloads recent event logs.
        </Alert>

        <TableContainer>
          <Table size="small" sx={{ minWidth: 980, tableLayout: 'fixed' }}>
            <TableHead>
              <TableRow>
                <TableCell sx={{ width: '24%' }}>Device Name</TableCell>
                <TableCell sx={{ width: '23%' }}>IP Address</TableCell>
                <TableCell align="right" sx={{ width: '10%' }}>
                  <Box
                    sx={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: 0.5,
                    }}
                  >
                    Existing Rows
                    <Tooltip title="Rows already stored before this verification run.">
                      <InfoOutlinedIcon color="action" fontSize="inherit" />
                    </Tooltip>
                  </Box>
                </TableCell>
                <TableCell align="right" sx={{ width: '10%' }}>
                  <Box
                    sx={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: 0.5,
                    }}
                  >
                    New Rows
                    <Tooltip title="Rows downloaded during this verification run.">
                      <InfoOutlinedIcon color="action" fontSize="inherit" />
                    </Tooltip>
                  </Box>
                </TableCell>
                <TableCell sx={{ width: '33%', minWidth: 320 }}>
                  Status
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {deviceList.map((device) => {
                const deviceId = device.id
                const isLoggingDisabled = device.loggingEnabled === false
                const newIp =
                  typeof deviceId === 'number'
                    ? ipChanges[deviceId] ?? device.ipaddress ?? ''
                    : device.ipaddress ?? ''
                const existingRowCount = isLoggingDisabled
                  ? undefined
                  : device.beforeWorkflowEventCount
                const insertedRowCount = isLoggingDisabled
                  ? undefined
                  : device.changeInEventCount
                const hasPendingIpChange = device.ipaddress !== newIp
                const isSavingThisIp =
                  isSavingIpChanges && hasPendingIpChange
                const isCheckingThisDevice =
                  isCheckingEvents && !isLoggingDisabled
                const verificationStatus = getVerificationStatus(device, {
                  hasPendingIpChange,
                  isCheckingEvents: isCheckingThisDevice,
                  isSavingIpChanges,
                })

                return (
                  <TableRow
                    key={deviceId ?? device.deviceIdentifier ?? getDeviceName(device)}
                    hover={!isLoggingDisabled}
                    sx={{
                      height: 118,
                      ...(isLoggingDisabled
                        ? {
                            backgroundColor: 'action.hover',
                            opacity: 0.6,
                          }
                        : {}),
                      '& > td': {
                        py: 1.5,
                        verticalAlign: 'top',
                      },
                    }}
                  >
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        {getDeviceName(device)}
                      </Typography>
                      {device.deviceIdentifier && (
                        <Typography variant="caption" color="text.secondary">
                          {device.deviceIdentifier}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Box sx={{ minWidth: 220 }}>
                        <TextField
                          fullWidth
                          label="IP Address"
                          size="small"
                          value={newIp}
                          disabled={typeof deviceId !== 'number'}
                          onChange={(e) => {
                            if (typeof deviceId === 'number') {
                              handleIpChange(deviceId, e.target.value)
                            }
                          }}
                        />
                        <Typography
                          variant="caption"
                          color={
                            isSavingThisIp
                              ? 'primary.main'
                              : hasPendingIpChange
                              ? 'warning.main'
                              : 'text.secondary'
                          }
                          sx={{ display: 'block', mt: 0.5 }}
                        >
                          {isSavingThisIp
                            ? 'Saving...'
                            : hasPendingIpChange
                              ? 'Unsaved'
                              : 'Saved'}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell align="right">
                      {renderCount(existingRowCount, {
                        isCheckingEvents: isCheckingThisDevice,
                        isLoggingDisabled,
                      })}
                    </TableCell>
                    <TableCell align="right">
                      {renderCount(insertedRowCount, {
                        isCheckingEvents: isCheckingThisDevice,
                        isLoggingDisabled,
                      })}
                    </TableCell>
                    <TableCell sx={{ width: '33%', minWidth: 320 }}>
                      <Box
                        sx={{
                          display: 'flex',
                          alignItems: 'flex-start',
                          gap: 1,
                          minHeight: 74,
                        }}
                      >
                        <Box sx={{ mt: 0.25 }}>{verificationStatus.icon}</Box>
                        <Box sx={{ minWidth: 0, maxWidth: 300 }}>
                          <Typography
                            variant="body2"
                            sx={{ fontWeight: 600 }}
                          >
                            {verificationStatus.label}
                          </Typography>
                          {verificationStatus.detail && (
                            <Typography
                              variant="caption"
                              color="text.secondary"
                              component="div"
                              sx={{
                                lineHeight: 1.45,
                                minHeight: 40,
                              }}
                            >
                              {verificationStatus.detail}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </TableContainer>

        <Box
          sx={{
            display: 'flex',
            alignItems: 'flex-end',
            justifyContent: 'space-between',
            mt: 2,
            gap: 1,
            flexWrap: 'wrap',
          }}
        >
          <Box>
            <LoadingButton
              startIcon={<LanIcon />}
              loading={isResyncing}
              loadingPosition="start"
              variant="contained"
              color="primary"
              disabled={!hasDevices}
              onClick={onResync}
            >
              {actionLabel}
            </LoadingButton>
          </Box>

          <Box>
            <Button onClick={onClose}>Close</Button>
            <Button
              variant="contained"
              onClick={onSaveAndClose}
              disabled={isResyncing}
            >
              Save and Close
            </Button>
          </Box>
        </Box>
      </Paper>
    </Modal>
  )
}

export default DevicesWizardModal
