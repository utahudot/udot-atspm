import { CreatedApiKeyResponse } from '@/features/identity/api/apiKeys'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  TextField,
  Tooltip,
} from '@mui/material'

interface CreatedApiKeyDialogProps {
  createdKey: CreatedApiKeyResponse | null
  onClose: () => void
  onCopy: () => void
}

const CreatedApiKeyDialog = ({
  createdKey,
  onClose,
  onCopy,
}: CreatedApiKeyDialogProps) => {
  return (
    <Dialog open={Boolean(createdKey)} onClose={onClose} fullWidth>
      <DialogTitle>API Key Created</DialogTitle>
      <DialogContent>
        <Alert severity="warning" sx={{ mb: 2 }}>
          Copy this key now. It cannot be retrieved again.
        </Alert>
        <TextField
          value={createdKey?.key ?? ''}
          fullWidth
          multiline
          InputProps={{
            readOnly: true,
            endAdornment: (
              <Tooltip title="Copy API key">
                <IconButton aria-label="Copy API key" onClick={onCopy} edge="end">
                  <ContentCopyIcon />
                </IconButton>
              </Tooltip>
            ),
          }}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="contained">
          Done
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default CreatedApiKeyDialog
