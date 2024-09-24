import { Impact } from '@/features/speedManagementTool/types/impact'
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline'
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
import { useRouter } from 'next/router'
import React, { useEffect, useState } from 'react'
import SegmentSelectMap from './SegmentSelectMap'
import { useGetSegments } from '../api/getSegments'
import { useGetApiV1ImpactType } from '@/api/speedManagement/aTSPMSpeedManagementApi'
interface ImpactEditorModalProps {
  data?: Impact
  open?: boolean
  onClose?: () => void
  onSave: (impact: Impact) => void
  onCreate: (inpact: Impact) => void
  onEdit: (impact: Impact) => void
}

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

const ImpactEditorModal: React.FC<ImpactEditorModalProps> = ({
  data,
  open,
  onClose,
  onSave,
  onCreate,
  onEdit,
}) => {
  const [impact, setImpact] = useState<Impact>({
    id: data?.id || null,
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

  const { data: impactTypeData, isLoading: isLoadingImpactTypes } =
    useGetApiV1ImpactType()
  const router = useRouter()
  const theme = useTheme()

  const { data: segmentData, isLoading: isLoadingSegments } = useGetSegments();


  useEffect(() => {
    if (data) {
      setImpact(data)
    }
  }, [data])

  const handleChange = (
    event: SelectChangeEvent<typeof impact.impactTypeIds>
  ) => {
    const {
      target: { value },
    } = event

    const selectedIds = value
      ? typeof value === 'string'
        ? value.split(',')
        : value
      : []
    const selectedImpactTypes =
      impactTypeData?.filter(
        (type) => type.id && selectedIds.includes(type.id)
      ) || []

    setImpact({
      ...impact,
      impactTypeIds: selectedIds,
      impactTypes: selectedImpactTypes,
    })
  }

  const handleSave = () => {
    const newErrors = {
      description: impact.description === '',
      start: !impact.start,
      end: !impact.end,
      startMile: !impact.startMile,
      endMile: !impact.endMile,
      impactTypes: !impact.impactTypes,
      segmentIds: !impact.segmentIds,
    }

    setErrors(newErrors)

    if (Object.values(newErrors).some((hasError) => hasError)) {
      return
    }
    console.log(impact)
    if (impact.id) {
      onEdit(impact)
      console.log(impact)
    } else {
      onCreate(impact)
      console.log(impact)
    }
    onClose()
  }

  const handleSegmentSelect = (
    segmentId: string,
    startMile: number,
    endMile: number
  ) => {
    setImpact((prevImpact) => {
      const currentSegmentIds = prevImpact.segmentIds || [];
      const updatedSegmentIds = currentSegmentIds.includes(segmentId)
        ? currentSegmentIds.filter((id) => id !== segmentId)
        : [...currentSegmentIds, segmentId];
  
      // Update start and end miles
      const updatedStartMile = Math.min(
        ...updatedSegmentIds.map((id) => {
          const segment = segmentData.features.find(
            (s) => s.properties.Id === id
          ); // Fixed typo here
          return segment ? segment.properties.StartMilePoint : Infinity;
        })
      );
  
      const updatedEndMile = Math.max(
        ...updatedSegmentIds.map((id) => {
          const segment = segmentData.features.find(
            (s) => s.properties.Id === id
          ); // Fixed typo here
          return segment ? segment.properties.EndMilePoint : -Infinity;
        })
      );
  
      return {
        ...prevImpact,
        segmentIds: updatedSegmentIds,
        startMile: updatedStartMile,
        endMile: updatedEndMile,
      };
    });
  };
  
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
            onChange={(e) =>
              setImpact({ ...impact, description: e.target.value })
            }
            fullWidth
          />
          <TextField
            label="Start Date"
            name="start"
            type="datetime-local"
            value={impact.start || new Date()}
            onChange={(e) => setImpact({ ...impact, start: e.target.value })}
            fullWidth
            InputLabelProps={{
              shrink: true,
            }}
          />
          <TextField
            label="End Date"
            name="end"
            type="datetime-local"
            value={impact.end || ''}
            onChange={(e) => setImpact({ ...impact, end: e.target.value })}
            fullWidth
            InputLabelProps={{
              shrink: true,
            }}
          />
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <TextField
            label="Start Mile"
            name="startMile"
            type="number"
            value={impact.startMile}
            onChange={(e) =>
              setImpact({
                ...impact,
                startMile: parseFloat(e.target.value),
              })
            }
            fullWidth
          />
          <TextField
            label="End Mile"
            name="endMile"
            type="number"
            value={impact.endMile}
            onChange={(e) =>
              setImpact({
                ...impact,
                endMile: parseFloat(e.target.value),
              })
            }
            fullWidth
          />
          <FormControl fullWidth>
            <InputLabel id="impact-types-label">Impact Types</InputLabel>
            <Select
              labelId="impact-types-label"
              id="impact-types-select"
              multiple
              value={
                impact.impactTypes
                  ? impact.impactTypes.map((type) => type.id)
                  : []
              } // Ensure it's an array
              onChange={handleChange}
              input={
                <OutlinedInput id="select-multiple-chip" label="Impact Types" />
              }
              renderValue={(selected) => {
                if (!impactTypeData || impactTypeData.length === 0) {
                  return '' // Show blank if data hasn't loaded
                }
                return (
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
                )
              }}
              MenuProps={MenuProps}
            >
              <MenuItem onClick={() => router.push('/admin/impact-types')}>
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    color: '#555555',
                  }}
                >
                  <AddCircleOutlineIcon style={{ fontSize: 'large' }} />
                  <Box sx={{ ml: 1 }}> Create Impact Type</Box>
                </Box>
              </MenuItem>
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
          {/* {errors?.segmentIds && (
            <Alert severity="error" sx={{ marginLeft: 1 }}>
              Please add Segments using the map below
            </Alert>
          )} */}
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
      <SegmentSelectMap
        selectedSegmentIds={impact.segmentIds}
        onSegmentSelect={handleSegmentSelect}
        segmentData={segmentData}
        isLoadingSegments={isLoadingSegments}
      />
    </Dialog>
  )
}

export default ImpactEditorModal
