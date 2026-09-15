import { CreateApiKeyData } from '@/features/identity/api/apiKeys'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Grid,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { useState } from 'react'

interface CreateApiKeyDialogProps {
  open: boolean
  availableClaims: string[]
  isClaimsLoading: boolean
  isCreating: boolean
  onClose: () => void
  onCreate: (payload: CreateApiKeyData) => Promise<boolean>
}

const CreateApiKeyDialog = ({
  open,
  availableClaims,
  isClaimsLoading,
  isCreating,
  onClose,
  onCreate,
}: CreateApiKeyDialogProps) => {
  const [name, setName] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [selectedClaims, setSelectedClaims] = useState<string[]>([])

  const resetForm = () => {
    setName('')
    setExpiresAt('')
    setSelectedClaims([])
  }

  const close = () => {
    onClose()
    resetForm()
  }

  const toggleClaim = (claim: string) => {
    setSelectedClaims((claims) =>
      claims.includes(claim)
        ? claims.filter((selected) => selected !== claim)
        : [...claims, claim].sort()
    )
  }

  const handleCreate = async () => {
    const wasCreated = await onCreate({
      name: name.trim(),
      expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      claims: selectedClaims,
    })

    if (wasCreated) {
      close()
    }
  }

  return (
    <Dialog open={open} onClose={close} fullWidth>
      <DialogTitle>Create API Key</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            inputProps={{ maxLength: 200 }}
            fullWidth
            required
          />
          <TextField
            label="Expires"
            type="datetime-local"
            value={expiresAt}
            onChange={(event) => setExpiresAt(event.target.value)}
            InputLabelProps={{ shrink: true }}
            fullWidth
          />
          <Box>
            <Typography variant="h6" sx={{ mb: 1 }}>
              Claims
            </Typography>
            {isClaimsLoading ? (
              <CircularProgress size={24} />
            ) : availableClaims.length === 0 ? (
              <Alert severity="info">
                No assignable API key claims are available.
              </Alert>
            ) : (
              <Grid container spacing={1}>
                {availableClaims.map((claim) => (
                  <Grid item xs={12} sm={6} key={claim}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={selectedClaims.includes(claim)}
                          onChange={() => toggleClaim(claim)}
                        />
                      }
                      label={claim}
                    />
                  </Grid>
                ))}
              </Grid>
            )}
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={close} variant="outlined">
          Cancel
        </Button>
        <Button
          onClick={handleCreate}
          variant="contained"
          disabled={isCreating || !name.trim() || selectedClaims.length === 0}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default CreateApiKeyDialog
