import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import InsertDriveFileOutlinedIcon from '@mui/icons-material/InsertDriveFileOutlined'
import UploadFileOutlinedIcon from '@mui/icons-material/UploadFileOutlined'
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { parseGpxFile } from '../../gpxFileParser'
import { GpxUploadOptions } from '../../types'
import {
  UPLOAD_ALERT_SX,
  UPLOAD_FILE_BOX_SX,
  UPLOAD_PANEL_BORDER_COLOR,
  UPLOAD_PANEL_MUTED_BG,
  UPLOAD_PANEL_SUBTLE_BG,
} from '../uploadStyles'

interface GpxUploadRowProps {
  entry: GpxUploadOptions
  index: number
  locations: string[]
  canDelete: boolean
  onDelete: () => void
  onChange: (updates: Partial<GpxUploadOptions>) => void
}

export const GpxUploadRow = ({
  entry,
  index,
  canDelete,
  onDelete,
  onChange,
  locations,
}: GpxUploadRowProps) => {
  return (
    <Box
      sx={{
        border: '1px solid',
        borderColor: UPLOAD_PANEL_BORDER_COLOR,
        borderRadius: 2,
        p: 1,
        backgroundColor: entry.primary ? UPLOAD_PANEL_SUBTLE_BG : UPLOAD_PANEL_MUTED_BG,
      }}
    >
      <Stack spacing={1}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 1,
          }}
        >
          <Stack direction="row" spacing={0.75} alignItems="center">
            <Chip
              label={`GPX ${index + 1}`}
              size="small"
              color={entry.primary ? 'primary' : 'default'}
              variant={entry.primary ? 'filled' : 'outlined'}
              sx={{
                height: 22,
                '& .MuiChip-label': {
                  px: 0.9,
                  fontSize: '0.7rem',
                  fontWeight: 700,
                },
              }}
            />
            <Typography variant="caption" sx={{ color: 'text.secondary' }}>
              {entry.primary ? 'Primary track' : 'Additional track'}
            </Typography>
          </Stack>

          {canDelete && (
            <IconButton
              size="small"
              onClick={onDelete}
              sx={{ color: 'text.secondary', p: 0.4 }}
              aria-label="Remove GPX"
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          )}
        </Box>

        <Stack direction="row" spacing={1} alignItems="stretch">
          <Button
            variant="outlined"
            size="small"
            component="label"
            startIcon={<UploadFileOutlinedIcon fontSize="small" />}
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
            Choose file
            <input
              type="file"
              hidden
              accept=".gpx"
              onChange={async (e) => {
                const file = e.target.files?.[0]
                if (!file) return

                try {
                  const parsed = await parseGpxFile(file)
                  onChange({
                    file,
                    parsedData: parsed,
                    parsedEntityData: undefined,
                    error: null,
                  })
                } catch {
                  onChange({
                    file: undefined,
                    parsedData: undefined,
                    parsedEntityData: undefined,
                    error: 'Invalid GPX file',
                  })
                }
              }}
            />
          </Button>

          <Box sx={UPLOAD_FILE_BOX_SX}>
            <Typography
              variant="caption"
              sx={{ display: 'block', color: 'text.secondary', lineHeight: 1.2 }}
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
                  color: entry.file ? 'text.secondary' : '#94A3B8',
                  flexShrink: 0,
                }}
              />
              <Typography
                variant="body2"
                noWrap
                sx={{
                  fontSize: '0.78rem',
                  color: entry.file ? 'text.primary' : 'text.secondary',
                }}
              >
                {entry.file?.name ?? 'No file selected'}
              </Typography>
            </Box>
          </Box>
        </Stack>

        {entry.error && (
          <Alert severity="error" sx={UPLOAD_ALERT_SX}>
            {entry.error}
          </Alert>
        )}

        <Stack direction="row" spacing={1}>
          <TextField
            select
            label="Start"
            size="small"
            value={entry.startLocation}
            onChange={(e) =>
              onChange({
                startLocation: e.target.value,
              })
            }
            fullWidth
            sx={{
              '& .MuiInputBase-input': {
                fontSize: '0.82rem',
              },
            }}
          >
            {locations?.map((l, i) => (
              <MenuItem key={i} value={l} sx={{ fontSize: '0.82rem' }}>
                {l}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            select
            label="End"
            size="small"
            value={entry.endLocation}
            onChange={(e) =>
              onChange({
                endLocation: e.target.value,
              })
            }
            fullWidth
            sx={{
              '& .MuiInputBase-input': {
                fontSize: '0.82rem',
              },
            }}
          >
            {locations?.map((l, i) => (
              <MenuItem key={i} value={l} sx={{ fontSize: '0.82rem' }}>
                {l}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
      </Stack>
    </Box>
  )
}
