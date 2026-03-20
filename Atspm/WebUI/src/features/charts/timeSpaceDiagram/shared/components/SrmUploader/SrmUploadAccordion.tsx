import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import InsertDriveFileOutlinedIcon from '@mui/icons-material/InsertDriveFileOutlined'
import UploadFileOutlinedIcon from '@mui/icons-material/UploadFileOutlined'
import { LoadingButton } from '@mui/lab'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  Stack,
  Typography,
} from '@mui/material'
import { useRef, useState } from 'react'
import {
  UPLOAD_ACCORDION_SX,
  UPLOAD_ALERT_SX,
  UPLOAD_FILE_BOX_SX,
} from '../uploadStyles'

interface Props {
  loading?: boolean
  error?: string | null
  hasAppliedSrm?: boolean
  onApply: (file: File) => Promise<void> | void
  onClear: () => void
}

export const SrmUploadAccordion = ({
  loading = false,
  error,
  hasAppliedSrm = false,
  onApply,
  onClear,
}: Props) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const fileInputRef = useRef<HTMLInputElement | null>(null)

  return (
    <Accordion
      defaultExpanded
      disableGutters
      elevation={0}
      sx={UPLOAD_ACCORDION_SX}
    >
      <AccordionSummary
        expandIcon={
          <ExpandMoreIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
        }
      >
        <Box sx={{ minWidth: 0 }}>
          <Typography fontWeight={600} sx={{ fontSize: '0.82rem' }}>
            SRM Overlay
          </Typography>
          <Typography
            variant="caption"
            sx={{
              display: 'block',
              color: 'text.secondary',
              lineHeight: 1.3,
              mt: 0.1,
            }}
          >
            Apply an SRM CSV to display overlay data.
          </Typography>
        </Box>
      </AccordionSummary>

      <AccordionDetails>
        <Stack spacing={1}>
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv"
            style={{ display: 'none' }}
            onChange={(e) => {
              setSelectedFile(e.target.files?.[0] ?? null)
            }}
          />

          <Stack direction="row" spacing={1} alignItems="stretch">
            <Button
              variant="outlined"
              size="small"
              startIcon={<UploadFileOutlinedIcon fontSize="small" />}
              onClick={() => fileInputRef.current?.click()}
              sx={{
                flexShrink: 0,
                whiteSpace: 'nowrap',
                px: 1,
                minHeight: 36,
                fontSize: '0.74rem',
                fontWeight: 600,
                textTransform: 'none',
              }}
            >
              Select CSV
            </Button>

            <Box sx={UPLOAD_FILE_BOX_SX}>
              <Typography
                variant="caption"
                sx={{
                  display: 'block',
                  color: 'text.secondary',
                  lineHeight: 1.2,
                }}
              >
                Selected file
              </Typography>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 0.6,
                  minWidth: 0,
                  mt: 0.25,
                }}
              >
                <InsertDriveFileOutlinedIcon
                  sx={{
                    fontSize: 15,
                    color: selectedFile ? 'text.secondary' : '#94A3B8',
                    flexShrink: 0,
                  }}
                />
                <Typography
                  variant="body2"
                  noWrap
                  sx={{
                    fontSize: '0.78rem',
                    color: selectedFile ? 'text.primary' : 'text.secondary',
                  }}
                >
                  {selectedFile?.name ?? 'No file selected'}
                </Typography>
              </Box>
            </Box>
          </Stack>

          <Typography
            variant="caption"
            sx={{
              color: 'text.secondary',
              lineHeight: 1.35,
              px: 0.25,
            }}
          >
            SRM is applied as an optional overlay after charts load.
          </Typography>

          {error && (
            <Alert severity="error" sx={UPLOAD_ALERT_SX}>
              {error}
            </Alert>
          )}

          <Box display="flex" gap={1}>
            <LoadingButton
              loading={loading}
              variant="contained"
              size="small"
              sx={{ flex: 1 }}
              disabled={!selectedFile}
              onClick={() => selectedFile && onApply(selectedFile)}
            >
              Apply
            </LoadingButton>

            <Button
              variant="text"
              size="small"
              sx={{ minWidth: 72 }}
              disabled={!selectedFile && !hasAppliedSrm}
              onClick={() => {
                setSelectedFile(null)
                if (fileInputRef.current) {
                  fileInputRef.current.value = ''
                }
                onClear()
              }}
            >
              Clear
            </Button>
          </Box>
        </Stack>
      </AccordionDetails>
    </Accordion>
  )
}
