import { ApiKeyMetadata } from '@/features/identity/api/apiKeys'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material'

interface RevokeApiKeyDialogProps {
  apiKey: ApiKeyMetadata | null
  isRevoking: boolean
  onClose: () => void
  onConfirm: () => void
}

const RevokeApiKeyDialog = ({
  apiKey,
  isRevoking,
  onClose,
  onConfirm,
}: RevokeApiKeyDialogProps) => {
  return (
    <Dialog open={Boolean(apiKey)} onClose={onClose}>
      <DialogTitle>Revoke API Key?</DialogTitle>
      <DialogContent>
        <Typography>
          Are you sure you want to revoke <b>{apiKey?.name}</b>?
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="outlined">
          Cancel
        </Button>
        <Button
          onClick={onConfirm}
          variant="contained"
          color="error"
          disabled={isRevoking}
        >
          Revoke
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default RevokeApiKeyDialog
