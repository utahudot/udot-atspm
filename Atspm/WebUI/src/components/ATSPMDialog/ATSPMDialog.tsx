import AuditInfo from '@/components/AuditInfo'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from '@mui/material'

interface ATSPM_DialogProps {
  children: React.ReactNode
  onSubmit?: (event: React.FormEvent<HTMLFormElement>) => void
  auditInfo?: {
    createdBy?: string
    createdOn?: string
    modifiedBy?: string
    modifiedOn?: string
  }
  isOpen: boolean
  onClose: () => void
  title?: string
  dialogProps?: object
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | false
}

const ATSPMDialog = ({
  children,
  onSubmit,
  auditInfo,
  isOpen,
  onClose,
  title,
  dialogProps,
  maxWidth = 'sm',
}: ATSPM_DialogProps) => {
  return (
    <Dialog
      open={isOpen}
      onClose={onClose}
      aria-labelledby={`${title}-dialog-title`}
      maxWidth={maxWidth}
      fullWidth
    >
      <DialogTitle id={`${title}-dialog-title`} variant="h4">
        {title}
      </DialogTitle>
      <form onSubmit={onSubmit}>
        <DialogContent sx={{ pt: 0, ...dialogProps }}>
          {children}
          <DialogActions
            sx={{
              justifyContent: auditInfo ? 'space-between' : 'flex-end',
              px: 0,
              pb: 0,
            }}
          >
            {auditInfo && <AuditInfo obj={auditInfo} />}
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button variant="outlined" onClick={onClose} color="primary">
                Cancel
              </Button>
              <Button variant="contained" type="submit" color="primary">
                Save
              </Button>
            </Box>
          </DialogActions>
        </DialogContent>
      </form>
    </Dialog>
  )
}

export default ATSPMDialog
