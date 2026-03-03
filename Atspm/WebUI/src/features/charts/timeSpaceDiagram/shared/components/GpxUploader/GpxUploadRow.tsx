import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import {
  Box,
  Button,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { parseGpxFile } from '../../gpxFileParser'
import { GpxUploadOptions } from '../../types'

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
    <Box borderColor="divider">
      <Typography variant="subtitle2" gutterBottom>
        GPX File {index + 1}
      </Typography>

      {canDelete && (
        <IconButton
          size="small"
          onClick={onDelete}
          sx={{ ml: 'auto' }}
          aria-label="Remove GPX"
        >
          <DeleteOutlineIcon fontSize="small" />
        </IconButton>
      )}

      {/* File Upload */}
      <Button variant="outlined" component="label">
        Insert GPX File
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
              onChange({ error: 'Invalid GPX file' })
            }
          }}
        />
      </Button>

      {entry.error && (
        <Typography color="error" variant="caption">
          {entry.error}
        </Typography>
      )}

      <Stack direction="row" spacing={2} mt={2}>
        {/* Start Location */}
        <TextField
          select
          label="Start Location"
          size="small"
          value={entry.startLocation}
          onChange={(e) =>
            onChange({
              startLocation: e.target.value as any,
            })
          }
          fullWidth
        >
          {locations?.map((l, i) => (
            <MenuItem key={i} value={l}>
              {l}
            </MenuItem>
          ))}
        </TextField>

        {/* End Location */}
        <TextField
          select
          label="End Location"
          size="small"
          value={entry.endLocation}
          onChange={(e) =>
            onChange({
              endLocation: e.target.value as any,
            })
          }
          fullWidth
        >
          {locations?.map((l, i) => (
            <MenuItem key={i} value={l}>
              {l}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
    </Box>
  )
}
