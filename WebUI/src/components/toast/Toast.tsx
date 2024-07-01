import { useNotificationStore } from '@/stores/notifications'
import { Alert, Snackbar } from '@mui/material'

const Toast = () => {
  const { notifications, dismissNotification } = useNotificationStore()

  return (
    <>
      {notifications.map((notification) => (
        <Snackbar
          key={notification.id}
          open={true}
          autoHideDuration={6000}
          onClose={() => dismissNotification(notification.id)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert
            onClose={() => dismissNotification(notification.id)}
            severity={notification.type}
          >
            <strong>{notification.title}</strong>
            {notification.message && <div>{notification.message}</div>}
          </Alert>
        </Snackbar>
      ))}
    </>
  )
}

export default Toast
