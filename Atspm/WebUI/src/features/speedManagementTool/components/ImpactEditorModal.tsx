import { useGetApiV1ImpactType } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { Impact } from '@/features/speedManagementTool/types/impact'
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  FormControl,
  InputLabel,
  MenuItem,
  OutlinedInput,
  Select,
  SelectChangeEvent,
  TextField,
} from '@mui/material'
import { useTheme } from '@mui/material/styles'
import { DatePicker } from '@mui/x-date-pickers'
import { isDate } from 'date-fns'
import {
  forwardRef,
  memo,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import SegmentSelectMapComponent from './SegmentSelectMap'

const ITEM_HEIGHT = 48
const ITEM_PADDING_TOP = 8
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
}

const MemoizedSegmentSelectMap = memo(SegmentSelectMapComponent)

interface ImpactEditorModalProps {
  data?: Impact
  open?: boolean
  onClose: () => void
  onSave: (impact: Impact) => void
  onCreate: (impact: Impact) => Promise<void>
  onEdit: (impact: Impact) => Promise<void>
  segments: any
}

const ImpactEditorModal = forwardRef(
  (
    {
      data,
      open = false,
      onClose,
      onSave,
      onCreate,
      onEdit,
      segments,
    }: ImpactEditorModalProps,
    ref
  ) => {
    const theme = useTheme()
    const { data: impactTypeData } = useGetApiV1ImpactType()
    // const router = useRouter()

    const [impact, setImpact] = useState<Impact>({
      id: data?.id || 0,
      description: data?.description || '',
      start: data?.start || null,
      end: data?.end || null,
      startMile: data?.startMile || 0,
      endMile: data?.endMile || 0,
      impactTypeIds: data?.impactTypeIds || [],
      impactTypes: data?.impactTypes || [],
      segmentIds: data?.segmentIds || [],
      impactTypeNames: '',
    })

    const [errors, setErrors] = useState({
      description: false,
      start: false,
      end: false,
      startMile: false,
      endMile: false,
      impactTypes: false,
      segmentIds: false,
    })

    useEffect(() => {
      if (data) {
        setImpact(data)
      } else {
        setImpact({
          id: 0,
          description: '',
          start: null,
          end: null,
          startMile: 0,
          endMile: 0,
          impactTypeIds: [],
          impactTypes: [],
          segmentIds: [],
          impactTypeNames: '',
        })
      }
    }, [data])

    const handleChange = useCallback(
      (event: SelectChangeEvent<typeof impact.impactTypeIds>) => {
        const {
          target: { value },
        } = event

        const selectedIds =
          value && typeof value === 'string' ? value.split(',') : value || []

        const selectedImpactTypes =
          impactTypeData?.filter(
            (type) => type.id && selectedIds.includes(type.id)
          ) || []

        setImpact((prevImpact) => ({
          ...prevImpact,
          impactTypeIds: selectedIds,
          impactTypes: selectedImpactTypes,
        }))
      },
      [impactTypeData]
    )

    const handleSave = useCallback(async () => {
      const newErrors = {
        description: impact.description === '',
        start: !impact.start,
        end: !impact.end,
        startMile: !impact.startMile && impact.startMile !== 0,
        endMile: !impact.endMile && impact.endMile !== 0,
        impactTypes: impact.impactTypes.length === 0,
        segmentIds: impact.segmentIds.length === 0,
      }

      setErrors(newErrors)

      // If there are any errors, don't proceed
      if (Object.values(newErrors).some((hasError) => hasError)) {
        return
      }

      try {
        if (impact.id) {
          await onEdit(impact)
        } else {
          await onCreate(impact)
        }
        onSave(impact)
        onClose()
      } catch (error) {
        console.error('Error occurred while saving impact:', error)
      }
    }, [impact, onClose, onCreate, onEdit, onSave])

    const handleSegmentSelect = useCallback(
      (segmentId: string) => {
        setImpact((prevImpact) => {
          const currentSegmentIds = prevImpact.segmentIds || []
          let updatedSegmentIds

          if (currentSegmentIds.includes(segmentId)) {
            updatedSegmentIds = currentSegmentIds.filter(
              (id) => id !== segmentId
            )
          } else {
            updatedSegmentIds = [...currentSegmentIds, segmentId]
          }

          // Update start and end miles
          const updatedStartMile = Math.min(
            ...updatedSegmentIds.map((id) => {
              const segment = segments.find((s) => s.properties.Id === id)
              return segment ? segment.properties.StartMilePoint : Infinity
            })
          )

          const updatedEndMile = Math.max(
            ...updatedSegmentIds.map((id) => {
              const segment = segments.find((s) => s.properties.Id === id)
              return segment ? segment.properties.EndMilePoint : -Infinity
            })
          )

          return {
            ...prevImpact,
            segmentIds: updatedSegmentIds,
            startMile: updatedStartMile,
            endMile: updatedEndMile,
          }
        })
      },
      [segments]
    )

    const selectedSegmentIds = useMemo(
      () => impact.segmentIds || [],
      [impact.segmentIds]
    )

    return (
      <Dialog
        open={open}
        onClose={onClose}
        fullScreen
        maxWidth="md"
        PaperProps={{
          sx: {
            height: '100%',
            width: '100%',
            margin: 'auto',
            maxWidth: 'lg',
          },
        }}
      >
        <h4 style={{ marginLeft: '15px' }}>
          {impact.id ? 'Edit Impact' : 'Create New Impact'}
        </h4>

        <Box
          sx={{
            marginX: '1.25rem',
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
          }}
        >
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Description"
              name="description"
              value={impact.description}
              error={errors.description}
              onChange={(e) =>
                setImpact((prevImpact) => ({
                  ...prevImpact,
                  description: e.target.value,
                }))
              }
              fullWidth
            />
            <DatePicker
              label="Start Date"
              value={isDate(impact.start) ? new Date(impact.start) : null}
              onChange={(date) =>
                setImpact((prevImpact) => ({
                  ...prevImpact,
                  start: date,
                }))
              }
            />
            <DatePicker
              label="End Date"
              value={isDate(impact.end) ? new Date(impact.end) : null}
              onChange={(date) =>
                setImpact((prevImpact) => ({
                  ...prevImpact,
                  end: date,
                }))
              }
            />
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Start Mile"
              name="startMile"
              value={impact.startMile}
              onChange={(e) =>
                setImpact((prevImpact) => ({
                  ...prevImpact,
                  startMile: parseFloat(e.target.value),
                }))
              }
              fullWidth
              error={errors.startMile}
            />
            <TextField
              label="End Mile"
              name="endMile"
              value={impact.endMile}
              onChange={(e) =>
                setImpact((prevImpact) => ({
                  ...prevImpact,
                  endMile: parseFloat(e.target.value),
                }))
              }
              fullWidth
              error={errors.endMile}
            />
            <FormControl fullWidth error={errors.impactTypes}>
              <InputLabel id="impact-types-label">Impact Types</InputLabel>
              <Select
                labelId="impact-types-label"
                id="impact-types-select"
                multiple
                value={impact.impactTypeIds || []}
                onChange={handleChange}
                input={
                  <OutlinedInput
                    id="select-multiple-chip"
                    label="Impact Types"
                  />
                }
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {selected.map((value) => (
                      <Chip
                        key={value}
                        label={
                          impactTypeData?.find((type) => type.id === value)
                            ?.name
                        }
                      />
                    ))}
                  </Box>
                )}
                MenuProps={MenuProps}
              >
                {/* <MenuItem onClick={() => router.push('/admin/impact-types')}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    color: '#555555',
                  }}
                >
                  <AddCircleOutlineIcon style={{ fontSize: 'large' }} />
                  <Box sx={{ ml: 1 }}>Create Impact Type</Box>
                </Box>
              </MenuItem> */}
                {impactTypeData?.map((type) => (
                  <MenuItem
                    key={type.id}
                    value={type.id}
                    style={{
                      fontWeight: (impact.impactTypes || []).some(
                        (impactType) => impactType.id === type.id
                      )
                        ? theme.typography.fontWeightMedium
                        : theme.typography.fontWeightRegular,
                    }}
                  >
                    {type.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </Box>
        <DialogActions sx={{ justifyContent: 'space-between' }}>
          <Box>
            {errors?.segmentIds && (
              <Alert severity="error" sx={{ marginLeft: 1 }}>
                Please add Segments using the map below
              </Alert>
            )}
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button onClick={onClose} color="secondary">
              Cancel
            </Button>
            <Button onClick={handleSave} color="primary">
              Save
            </Button>
          </Box>
        </DialogActions>
        <MemoizedSegmentSelectMap
          selectedSegmentIds={selectedSegmentIds}
          onSegmentSelect={handleSegmentSelect}
          segments={segments}
        />
      </Dialog>
    )
  }
)

ImpactEditorModal.displayName = 'ImpactEditorModal'
export default memo(ImpactEditorModal)
