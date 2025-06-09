// components/VersionHistoryDialog.tsx
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogTitle,
  Typography,
} from '@mui/material'
import React from 'react'

interface Version {
  id: number
  name: string
  date: string
  version: string
  notes: string
  createdBy: string
  children?: Version[]
}

interface Props {
  open: boolean
  onClose: () => void
  data: Version[]
  isLoading: boolean
}

const th: React.CSSProperties = {
  borderBottom: '1px solid #ccc',
  textAlign: 'left',
  padding: '8px',
  fontWeight: 'bold',
  backgroundColor: '#f5f5f5',
}

const td: React.CSSProperties = {
  borderBottom: '1px solid #eee',
  padding: '8px',
}

const renderVersionRow = (version: Version) => (
  <tr key={version.id}>
    <td style={td}>{version.version}</td>
    <td style={td}>{version.name}</td>
    <td style={td}>{new Date(version.date).toLocaleDateString()}</td>
    <td style={td}>{version.notes}</td>
    <td style={td}>{version.createdBy}</td>
  </tr>
)

const VersionHistoryDialog: React.FC<Props> = ({
  open,
  onClose,
  data,
  isLoading,
}) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="xl"
      PaperProps={{ sx: { minHeight: '80vh', p: 4 } }}
    >
      <DialogTitle
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Typography variant="h3" component="div">
          Version History
        </Typography>
      </DialogTitle>

      <Box sx={{ flexGrow: 1, p: 2 }}>
        {isLoading ? (
          <Typography>Loading...</Typography>
        ) : (
          <Box sx={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={th}>Version</th>
                  <th style={th}>Name</th>
                  <th style={th}>Date</th>
                  <th style={th}>Notes</th>
                  <th style={th}>Created By</th>
                </tr>
              </thead>
              <tbody>
                {[...new Map(data.map((item) => [item.id, item])).values()].map(
                  renderVersionRow
                )}
              </tbody>
            </table>
          </Box>
        )}
      </Box>

      <DialogActions sx={{ justifyContent: 'flex-end' }}>
        <Button onClick={onClose} variant="contained">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default VersionHistoryDialog
