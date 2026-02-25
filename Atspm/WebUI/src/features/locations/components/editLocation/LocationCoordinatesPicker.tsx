import { usePatchLocationFromKey } from '@/api/config'
import { useLocationStore } from '@/features/locations/components/editLocation/locationStore'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
import PinDropIcon from '@mui/icons-material/PinDrop'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  Skeleton,
  Tooltip,
} from '@mui/material'
import dynamic from 'next/dynamic'
import { useEffect, useState } from 'react'
import { useQueryClient } from 'react-query'

type LatLngTuple = [number, number]

type LocationCoordinatePickerProps = {
  onChange: (lat: number, lng: number) => void
}

const DynamicLocationCoordinateMap = dynamic(
  () => import('./LocationCoordinateMap'),
  {
    ssr: false,
    loading: () => <Skeleton variant="rectangular" height={300} />,
  }
)

export default function LocationCoordinatePicker({
  onChange,
}: LocationCoordinatePickerProps) {
  const { location } = useLocationStore()
  const { addNotification } = useNotificationStore()
  const queryClient = useQueryClient()
  const { latitude, longitude, locationTypeId, id } = location!

  const { mutateAsync: saveCoordinates } = usePatchLocationFromKey()

  const [mapCenter, setMapCenter] = useState<LatLngTuple>([0, 0])
  const [zoom, setZoom] = useState(17)
  const [isOpen, setIsOpen] = useState(false)
  const [pendingCoords, setPendingCoords] = useState<LatLngTuple | null>(null)

  useEffect(() => {
    if (latitude != 0 && longitude != 0) {
      const lat = parseFloat(String(latitude))
      const lng = parseFloat(String(longitude))
      if (!Number.isNaN(lat) && !Number.isNaN(lng)) {
        setZoom(17)
        setMapCenter([lat, lng])
        return
      }
    }
    async function getDefaultCenter() {
      const env = await getEnv()
      setZoom(env?.MAP_DEFAULT_ZOOM)
      setMapCenter([env?.MAP_DEFAULT_LATITUDE, env?.MAP_DEFAULT_LONGITUDE])
    }

    getDefaultCenter()
  }, [latitude, longitude])

  const handleOpen = () => {
    const seed: LatLngTuple =
      latitude != null && longitude != null
        ? [
            parseFloat(String(latitude || mapCenter[0])),
            parseFloat(String(longitude || mapCenter[1])),
          ]
        : mapCenter

    setPendingCoords(seed)
    setIsOpen(true)
  }

  const handleClose = () => {
    setIsOpen(false)
    setPendingCoords(null)
  }

  const handleMapClick = (lat: number, lng: number) => {
    setPendingCoords([lat, lng])
  }

  const handleSelect = async () => {
    if (!pendingCoords) {
      setIsOpen(false)
      return
    }

    if (!id) {
      addNotification({
        type: 'error',
        title: 'Location ID is missing',
        message: 'Cannot save coordinates without a valid location ID.',
      })
      setIsOpen(false)
      return
    }

    try {
      await saveCoordinates({
        key: id,
        data: {
          latitude: pendingCoords[0],
          longitude: pendingCoords[1],
        },
      })
      queryClient.invalidateQueries()

      addNotification({
        type: 'success',
        title: `Coordinates updated`,
      })
    } catch (error) {
      console.error('Failed to save coordinates:', error)
      addNotification({
        type: 'error',
        title: 'Failed to save coordinates',
        message: String(error),
      })
    }

    const [lat, lng] = pendingCoords
    onChange(lat, lng)
    setMapCenter(pendingCoords)
    setIsOpen(false)
    setPendingCoords(null)
  }

  const effectiveCenter: LatLngTuple = pendingCoords ?? mapCenter

  return (
    <>
      <Tooltip title="Pick coordinates from map">
        <Button
          aria-label="pick coordinates from map"
          onClick={handleOpen}
          variant="outlined"
          size="small"
          sx={{
            minWidth: 0,
            padding: '8px 10px',
            borderRadius: '50%',
          }}
        >
          <PinDropIcon fontSize="small" />
        </Button>
      </Tooltip>

      <Dialog
        open={isOpen}
        onClose={handleClose}
        maxWidth="lg"
        fullWidth
        sx={{ p: 0 }}
      >
        <DialogContent sx={{ height: 800, p: 2, pb: 0 }}>
          <DynamicLocationCoordinateMap
            center={effectiveCenter}
            zoom={zoom}
            onSelect={handleMapClick}
            locationTypeId={locationTypeId ?? null}
          />
        </DialogContent>
        <DialogActions>
          <Box sx={{ flexGrow: 1, ml: 1 }}>
            Selected Coordinates:{' '}
            {pendingCoords
              ? `${pendingCoords[0]?.toFixed(6)}, ${pendingCoords[1]?.toFixed(6)}`
              : 'None'}
          </Box>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSelect} variant="contained">
            Update Coordinates
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
