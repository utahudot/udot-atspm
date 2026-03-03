import { Box, Button, Stack } from '@mui/material'
import { GpxUploadOptions } from '../../types'
import { GpxUploadRow } from './GpxUploadRow'

interface Prop {
  locations: string[]
  entries: GpxUploadOptions[]
  setEntries: React.Dispatch<React.SetStateAction<GpxUploadOptions[]>>
}

function createEmptyEntry(primary = false): GpxUploadOptions {
  return {
    id: '',
    startLocationMode: '',
    endLocationMode: '',
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
      <Stack>
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
          sx={{ alignSelf: 'flex-start' }}
        >
          + Add another GPX
        </Button>
      </Stack>
    </Box>
  )
}
