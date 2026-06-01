import { usePostApiV1EntityFileAtspmRefresh } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useNotificationStore } from '@/stores/notifications'
import { Alert, Button, Typography } from '@mui/material'

export default function AtspmSourcePanel() {
  const { addNotification } = useNotificationStore()
  const { mutateAsync: refreshAtspm, isLoading } =
    usePostApiV1EntityFileAtspmRefresh()

  const handleRefresh = async () => {
    try {
      await refreshAtspm({})
      addNotification({ title: 'ATSPM data refresh started', type: 'success' })
    } catch (e: any) {
      addNotification({
        title: e?.message ?? 'Failed to refresh ATSPM',
        type: 'error',
      })
    }
  }

  return (
    <>
      <Typography variant="h6" sx={{ mb: 1 }}>
        ATSPM • Refresh Data
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Updates the ATSPM dataset. No bucket or file input is required; the
        server uses configured connections to pull the latest data and re-run
        any necessary processing.
      </Typography>

      {!isLoading && (
        <Button
          variant="contained"
          onClick={handleRefresh}
          disabled={isLoading}
        >
          Refresh Data
        </Button>
      )}
      {isLoading && (
        <Alert severity="info" sx={{ mt: 2 }}>
          ATSPM data refresh is in progress. No further action is required.
        </Alert>
      )}
    </>
  )
}
