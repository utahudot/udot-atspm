import AddRoundedIcon from '@mui/icons-material/AddRounded'
import { Box, Button, Stack } from '@mui/material'
import { nanoid } from 'nanoid'
import { GpxUploadOptions } from '../../types'
import { GpxUploadRow } from './GpxUploadRow'

interface Prop {
  locations: string[]
  entries: GpxUploadOptions[]
  setEntries: React.Dispatch<React.SetStateAction<GpxUploadOptions[]>>
}

function createEmptyEntry(primary = false): GpxUploadOptions {
  return {
    id: nanoid(),
    startLocation: '',
    endLocation: '',
    error: null,
    primary,
  }
}

export const GpxUploadComponent = (prop: Prop) => {
  const { locations, entries, setEntries } = prop

  const updateEntry = (id: string, updates: Partial<GpxUploadOptions>) => {
    setEntries((prev) =>
      prev.map((e) => (e.id === id ? { ...e, ...updates } : e))
    )
  }

  const addEntry = () => {
    setEntries((prev) => [...prev, createEmptyEntry(false)])
  }

  const deleteEntry = (id: string) => {
    setEntries((prev) => prev.filter((e) => e.id !== id))
  }

  return (
    <Box>
      <Stack spacing={1}>
        {entries.map((entry, index) => (
          <GpxUploadRow
            key={`${index} ${entry.id}`}
            entry={entry}
            index={index}
            locations={locations}
            canDelete={!entry.primary}
            onDelete={() => deleteEntry(entry.id)}
            onChange={(updates) => updateEntry(entry.id, updates)}
          />
        ))}

        <Button
          onClick={addEntry}
          variant="text"
          size="small"
          startIcon={<AddRoundedIcon fontSize="small" />}
          sx={{
            alignSelf: 'flex-start',
            px: 0.5,
            fontSize: '0.76rem',
            fontWeight: 600,
            textTransform: 'none',
          }}
        >
          Add another GPX
        </Button>
      </Stack>
    </Box>
  )
}
