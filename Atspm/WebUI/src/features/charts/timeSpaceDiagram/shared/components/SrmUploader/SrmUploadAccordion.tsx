import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { LoadingButton } from '@mui/lab'
import { useRef, useState } from 'react'

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
    <Accordion defaultExpanded elevation={0}>
      <AccordionSummary expandIcon={<ExpandMoreIcon />}>
        <Typography fontWeight={600}>SRM Options</Typography>
      </AccordionSummary>

      <AccordionDetails>
        <Stack spacing={1.5}>
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv"
            style={{ display: 'none' }}
            onChange={(e) => {
              setSelectedFile(e.target.files?.[0] ?? null)
            }}
          />

          <Button
            variant="outlined"
            size="small"
            fullWidth
            onClick={() => fileInputRef.current?.click()}
          >
            Select SRM CSV File
          </Button>

          <TextField
            fullWidth
            size="small"
            label="Selected File"
            value={selectedFile?.name ?? ''}
            disabled
            helperText="SRM is applied as an optional overlay after charts load."
          />

          {error && <Alert severity="error">{error}</Alert>}

          <Box display="flex" gap={1}>
            <LoadingButton
              loading={loading}
              variant="contained"
              fullWidth
              disabled={!selectedFile}
              onClick={() => selectedFile && onApply(selectedFile)}
            >
              Apply SRM
            </LoadingButton>

            <Button
              variant="text"
              fullWidth
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
