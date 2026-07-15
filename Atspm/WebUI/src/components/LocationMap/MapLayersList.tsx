import type { PersistedMapLayer } from '@/features/mapLayers/types'
import LayersIcon from '@mui/icons-material/Layers'
import {
  Box,
  Button,
  ButtonGroup,
  Checkbox,
  ClickAwayListener,
  FormControlLabel,
  Popper,
} from '@mui/material'
import { useRef, useState } from 'react'

interface MapLayersListProps {
  mapLayers: PersistedMapLayer[]
  activeLayers: number[]
  handleLayerToggle: (layerId: number) => void
}

const MapLayersList = ({
  mapLayers,
  activeLayers,
  handleLayerToggle,
}: MapLayersListProps) => {
  const [isLayersPopperOpen, setIsLayersPopperOpen] = useState(false)
  const layersButtonRef = useRef(null)

  if (!mapLayers.length) return null

  return (
    <>
      <ButtonGroup
        variant="contained"
        size="small"
        disableElevation
        sx={{
          position: 'absolute',
          left: '10px',
          bottom: '20px',
          zIndex: 1000,
        }}
      >
        <Button
          ref={layersButtonRef}
          variant="contained"
          aria-label="Map layers"
          onClick={() => setIsLayersPopperOpen(!isLayersPopperOpen)}
        >
          <LayersIcon fontSize="small" />
        </Button>
      </ButtonGroup>

      <Popper
        open={isLayersPopperOpen}
        anchorEl={layersButtonRef.current}
        placement="top-start"
        style={{ zIndex: 1000 }}
      >
        <ClickAwayListener onClickAway={() => setIsLayersPopperOpen(false)}>
          <Box
            sx={{
              p: 2,
              bgcolor: 'background.paper',
              borderRadius: 1,
              boxShadow: 3,
              display: 'flex',
              flexDirection: 'column',
              gap: 1,
            }}
          >
            {mapLayers.map((layer) => (
              <FormControlLabel
                key={layer.id}
                control={
                  <Checkbox
                    checked={activeLayers.includes(layer.id)}
                    onChange={() => handleLayerToggle(layer.id)}
                    size="small"
                    sx={{ p: 0.5 }}
                  />
                }
                label={layer.name}
              />
            ))}
          </Box>
        </ClickAwayListener>
      </Popper>
    </>
  )
}

export default MapLayersList
