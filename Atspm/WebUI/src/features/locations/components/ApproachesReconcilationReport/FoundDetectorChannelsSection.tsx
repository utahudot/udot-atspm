import type { ConfigApproach } from '@/features/locations/components/editLocation/locationStore'
import type { DetectionType } from '@/features/locations/types'
import { useNotificationStore } from '@/stores/notifications'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Menu,
  MenuItem,
  Paper,
  Typography,
  useTheme,
} from '@mui/material'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFnsV3'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import React, { useEffect, useMemo, useState } from 'react'
import { NavigationProvider } from '../Cell/CellNavigation'
import { DetectorEditor } from '../editDetector/DetectorEditor'
import type {
  ConfigDetector,
  LocationStore,
} from '../editLocation/locationStore'
import DiscrepancyRow, { DiscrepancyItem } from './DiscrepancyRow'
import DiscrepancySectionCard from './DiscrepancySectionCard'
import type { ItemStatus } from './useDiscrepancyStatuses'

export default function FoundDetectorChannelsSection({
  items,
  itemStatuses,
  updateStatus,
  selected,
  setSelected,
  approaches,
  addDetector,
  detectionTypes,
  locationTypeName,
}: {
  items: DiscrepancyItem[]
  itemStatuses: Record<string, ItemStatus>
  updateStatus: (id: string, status: ItemStatus) => void
  selected: Record<string, boolean>
  setSelected: React.Dispatch<React.SetStateAction<Record<string, boolean>>>
  approaches: ConfigApproach[]
  addDetector: LocationStore['addDetector']
  detectionTypes: DetectionType[]
  locationTypeName?: string
}) {
  const [draft, setDraft] = useState<ConfigApproach | null>(null)
  const [originalChannels, setOriginalChannels] = useState<number[]>([])
  const [errors, setErrors] = useState<LocationStore['errors']>(null)
  const draftChannels =
    draft?.detectors.map((d) => Number(d.detectorChannel)) ?? []
  const usedChannels = new Set(
    approaches.flatMap((a) => a.detectors.map((d) => Number(d.detectorChannel)))
  )
  const invalidChannels =
    draftChannels.some(
      (c) => !Number.isInteger(c) || c < 1 || usedChannels.has(c)
    ) || new Set(draftChannels).size !== draftChannels.length
  const theme = useTheme()
  const { addNotification } = useNotificationStore()

  const [availableItems, setAvailableItems] = useState<DiscrepancyItem[]>(items)

  useEffect(() => {
    setAvailableItems(items)
  }, [items])

  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const open = Boolean(anchorEl)

  const toggle = (key: string) => setSelected((p) => ({ ...p, [key]: !p[key] }))
  const clear = () => setSelected({})

  const selectedItems = useMemo(
    () => availableItems.filter((it) => !!selected[it.id.toString()]),
    [availableItems, selected]
  )

  const selectedCount = selectedItems.length

  const onOpenMenu = (e: React.MouseEvent<HTMLButtonElement>) =>
    setAnchorEl(e.currentTarget)
  const onCloseMenu = () => setAnchorEl(null)

  const addSelectedToApproach = (approachId: number) => {
    if (!selectedItems.length) return

    const approach = approaches.find((a) => a.id === approachId)
    if (!approach) return
    const channels = selectedItems.map((it) => Number(it.label))
    setOriginalChannels(channels)
    setErrors(null)
    setDraft({
      ...approach,
      detectors: channels.map(
        (channel, index) =>
          ({
            id: -(index + 1),
            approachId,
            isNew: true,
            detectorChannel: channel,
            dectectorIdentifier: '',
            detectionTypes: [],
            detectionHardware: 'NA',
            laneNumber: null,
            movementType: 'NA',
            laneType: 'NA',
            latencyCorrection: 0,
            distanceFromStopBar: null,
            decisionPoint: null,
            minSpeedFilter: null,
            movementDelay: null,
            dateAdded: new Date().toISOString(),
            detectorComments: [],
          }) as unknown as ConfigDetector
      ),
    })
    onCloseMenu()
  }

  const applyDraft = () => {
    if (!draft || invalidChannels || !approaches.some((a) => a.id === draft.id))
      return
    try {
      const channelsAdded: number[] = []

      for (const detector of draft.detectors) {
        addDetector(draft.id, Number(detector.detectorChannel), detector)
      }
      channelsAdded.push(
        ...originalChannels.filter((channel) => draftChannels.includes(channel))
      )
      if (channelsAdded.length) {
        setAvailableItems((prev) =>
          prev.filter((it) => !channelsAdded.includes(Number(it.label)))
        )
        for (const it of selectedItems) {
          if (channelsAdded.includes(Number(it.label)))
            updateStatus(it.id.toString(), 'added')
        }
      }

      addNotification({
        title: `Added ${selectedItems.length} detector(s)`,
        type: 'success',
      })
      clear()
      setDraft(null)
    } catch (err) {
      console.error(err)
      addNotification({ title: 'Error adding detectors', type: 'error' })
    } finally {
      onCloseMenu()
    }
  }

  return (
    <>
      <Paper
        variant="outlined"
        sx={{ p: 2, backgroundColor: theme.palette.grey[50] }}
      >
        <DiscrepancySectionCard
          title="Detector Channels"
          actions={
            <>
              {selectedCount > 0 ? (
                <Typography
                  variant="body2"
                  sx={{
                    mr: 1,
                    color: theme.palette.text.secondary,
                    whiteSpace: 'nowrap',
                  }}
                >
                  {selectedCount} selected
                </Typography>
              ) : null}

              <Button
                size="small"
                variant="outlined"
                disabled={selectedCount === 0 || approaches.length === 0}
                onClick={onOpenMenu}
                endIcon={<KeyboardArrowDownIcon />}
                sx={{ height: 28 }}
              >
                Add selected to...
              </Button>

              <Menu anchorEl={anchorEl} open={open} onClose={onCloseMenu}>
                {approaches.map((a) => (
                  <MenuItem
                    key={a.id}
                    onClick={() => addSelectedToApproach(a.id)}
                  >
                    {a.description || a.id}
                  </MenuItem>
                ))}
              </Menu>

              <Button
                size="small"
                variant="text"
                disabled={selectedCount === 0}
                onClick={clear}
                sx={{ height: 28 }}
              >
                Clear
              </Button>
            </>
          }
        >
          <DiscrepancyRow
            items={availableItems}
            itemStatuses={itemStatuses}
            isSelected={(it) => !!selected[it.id.toString()]}
            onToggleSelected={(it) => toggle(it.id.toString())}
            buttonWidth={60}
          />
        </DiscrepancySectionCard>
      </Paper>
      <Dialog
        open={Boolean(draft)}
        onClose={() => setDraft(null)}
        maxWidth="xl"
        fullWidth
      >
        <DialogTitle>Configure detectors before adding</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>
            Adding to {draft?.description}. Edit the detector properties, then
            add them to the configuration.
          </Typography>
          {invalidChannels && (
            <Alert severity="error">
              Channels must be positive whole numbers and unique across the
              location.
            </Alert>
          )}
          {draft && (
            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <NavigationProvider>
                <DetectorEditor
                  approach={draft}
                  deleteMode={false}
                  detectionTypes={detectionTypes}
                  locationTypeName={locationTypeName}
                  errors={errors}
                  setErrors={setErrors}
                  warnings={null}
                  channelMap={
                    new Map(
                      [
                        ...approaches.flatMap((a) => a.detectors),
                        ...draft.detectors,
                      ].map((d) => [d.id, Number(d.detectorChannel)])
                    )
                  }
                  updateDetector={(id, name, value) =>
                    setDraft(
                      (current) =>
                        current && {
                          ...current,
                          detectors: current.detectors.map((d) =>
                            d.id === id
                              ? { ...d, [name]: value === '' ? null : value }
                              : d
                          ),
                        }
                    )
                  }
                />
              </NavigationProvider>
            </LocalizationProvider>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDraft(null)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={invalidChannels}
            onClick={applyDraft}
          >
            Add detectors
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
