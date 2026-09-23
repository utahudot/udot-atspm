import { Button, Stack } from '@mui/material'

interface MapControlsProps {
  onSplit: () => void
  onJoin: () => void
  onClear: () => void
  polylineCoordinates: [number, number][]
}

export const MapControls = ({
  onSplit,
  onJoin,
  onClear,
  polylineCoordinates,
}: MapControlsProps) => {
  return (
    <Stack
      direction="row"
      spacing={1}
      sx={{
        position: 'absolute',
        bottom: 10,
        left: 10,
        zIndex: 1000,
        padding: '5px',
        borderRadius: '4px',
      }}
    >
      <Button
        variant="contained"
        color="primary"
        onClick={onSplit}
        size="small"
      >
        Split
      </Button>
      <Button variant="contained" color="primary" onClick={onJoin} size="small">
        Join
      </Button>
      <Button
        variant="contained"
        color="error"
        size="small"
        onClick={onClear}
        disabled={polylineCoordinates.length === 0}
      >
        Clear Points
      </Button>
    </Stack>
  )
}
