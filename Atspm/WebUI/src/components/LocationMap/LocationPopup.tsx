import { SearchLocation as Location } from '@/api/config'
import CloseIcon from '@mui/icons-material/Close'
import MapIcon from '@mui/icons-material/Map'
import StreetviewIcon from '@mui/icons-material/Streetview'
import { Box, IconButton, Tooltip } from '@mui/material'
import { memo } from 'react'
import { useMap } from 'react-leaflet'

export type StreetViewAvailability = 'unknown' | 'available' | 'unavailable'

type LocationPopupProps = {
  marker: Location
  regionName: string
  jurisdictionName: string
  areaNames: string[]
  streetViewStatus: StreetViewAvailability | undefined
  streetViewUrl: (lat: number, lng: number) => string
  googleMapsUrl: (lat: number, lng: number) => string
}

const PopupCloseButton = () => {
  const map = useMap()

  return (
    <Tooltip title="Close" arrow>
      <IconButton
        size="small"
        onClick={() => map.closePopup()}
        sx={{
          position: 'absolute',
          top: -5,
          right: 1,
          bgcolor: 'transparent',
          '&:hover': { bgcolor: 'transparent' },
          p: 0,
          zIndex: 2,
        }}
        aria-label="Close popup"
      >
        <CloseIcon fontSize="small" />
      </IconButton>
    </Tooltip>
  )
}

const LocationPopup = ({
  marker,
  regionName,
  jurisdictionName,
  areaNames,
  streetViewStatus,
  streetViewUrl,
  googleMapsUrl,
}: LocationPopupProps) => {
  const streetViewEnabled = streetViewStatus === 'available'
  const streetViewChecking = streetViewStatus === 'unknown'
  const streetViewChecked = streetViewStatus != null

  const region = regionName || '—'
  const jurisdiction = jurisdictionName || '—'
  const areas = areaNames.length ? areaNames.join(', ') : '—'

  return (
    <Box sx={{ minWidth: 320, position: 'relative', pr: 4 }}>
      <Box sx={{ minWidth: 0 }}>
        <Box sx={{ fontWeight: 600, lineHeight: 1.2 }}>
          Location #{marker.locationIdentifier}
        </Box>
        <Box
          sx={{
            mt: 0.25,
            color: 'text.secondary',
            lineHeight: 1.2,
            whiteSpace: 'nowrap',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            maxWidth: 300,
          }}
          title={`${marker.primaryName} & ${marker.secondaryName}`}
        >
          {marker.primaryName} & {marker.secondaryName}
        </Box>
        <PopupCloseButton />
      </Box>

      <Box
        sx={{
          mt: 1,
          pt: 1,
          borderTop: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Box sx={{ display: 'flex', gap: 0.5, mb: 1 }}>
          <Tooltip
            title={
              streetViewEnabled
                ? 'Open Street View'
                : streetViewChecking
                  ? 'Checking Street View…'
                  : streetViewChecked
                    ? 'Street View not available'
                    : 'Street View availability unknown'
            }
            arrow
          >
            <span>
              <IconButton
                size="small"
                component={streetViewEnabled ? 'a' : 'button'}
                href={
                  streetViewEnabled
                    ? streetViewUrl(marker.latitude, marker.longitude)
                    : undefined
                }
                target={streetViewEnabled ? '_blank' : undefined}
                rel={streetViewEnabled ? 'noreferrer' : undefined}
                disabled={!streetViewEnabled}
                sx={{
                  bgcolor: 'action.hover',
                  '&:hover': { bgcolor: 'action.selected' },
                }}
                aria-label="Open Street View"
              >
                <StreetviewIcon fontSize="small" />
              </IconButton>
            </span>
          </Tooltip>

          <Tooltip title="Open in Google Maps" arrow>
            <IconButton
              size="small"
              component="a"
              href={googleMapsUrl(marker.latitude, marker.longitude)}
              target="_blank"
              rel="noreferrer"
              sx={{
                bgcolor: 'action.hover',
                '&:hover': { bgcolor: 'action.selected' },
              }}
              aria-label="Open in Google Maps"
            >
              <MapIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>

        <Box sx={{ display: 'grid', gap: 0.75 }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: '110px 1fr' }}>
            <Box sx={{ color: 'text.secondary', fontSize: 13 }}>Region</Box>
            <Box sx={{ fontSize: 13, fontWeight: 600 }} title={region}>
              {region}
            </Box>
          </Box>

          <Box sx={{ display: 'grid', gridTemplateColumns: '110px 1fr' }}>
            <Box sx={{ color: 'text.secondary', fontSize: 13 }}>
              Jurisdiction
            </Box>
            <Box sx={{ fontSize: 13, fontWeight: 600 }} title={jurisdiction}>
              {jurisdiction}
            </Box>
          </Box>

          <Box sx={{ display: 'grid', gridTemplateColumns: '110px 1fr' }}>
            <Box sx={{ color: 'text.secondary', fontSize: 13 }}>Areas</Box>
            <Box sx={{ fontSize: 13, fontWeight: 600, lineHeight: 1.25 }}>
              {areas}
            </Box>
          </Box>
        </Box>
      </Box>
    </Box>
  )
}

export default memo(LocationPopup)
