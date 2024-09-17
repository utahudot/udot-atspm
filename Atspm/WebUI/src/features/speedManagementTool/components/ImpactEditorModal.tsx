import { useGetApiImpactType } from '@/api/speedManagement'
import { Impact } from '@/features/speedManagementTool/types/impact'
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  FormControl,
  Grid,
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
import { useGetSegments } from '../api/getSegments'
interface ImpactEditorModalProps {
  data?: Impact
  open: boolean
  onClose: () => void
  onCreate: (impact: Impact) => void
  onEdit: (impact: Impact) => void
  onDelete?: (id: string) => void
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
  onCreate,
  onEdit,
  onDelete,
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
  })

  const { data: segmentData, isLoading: isLoadingSegements } = useGetSegments()

  useEffect(() => {
    if (!isLoadingSegements) {
      console.log(segmentData)
    }
  }, [segmentData])

  const { data: impactTypeData } = useGetApiImpactType()
  const router = useRouter()
  const theme = useTheme()

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
    if (impact.id) {
      onEdit(impact)
    } else {
      onCreate(impact)
    }
    onClose()
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="lg">
      <Box sx={{ marginX: '1.25rem' }}>
        <h4>{impact.id ? 'Edit Impact' : 'Create New Impact'}</h4>
        <Grid container spacing={2}>
          <Grid item xs={5}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Description"
                  name="description"
                  value={impact.description}
                  onChange={(e) =>
                    setImpact({ ...impact, description: e.target.value })
                  }
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Start Date"
                  name="start"
                  type="datetime-local"
                  value={impact.start || new Date()}
                  onChange={(e) =>
                    setImpact({ ...impact, start: e.target.value })
                  }
                  fullWidth
                  InputLabelProps={{
                    shrink: true,
                  }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="End Date"
                  name="end"
                  type="datetime-local"
                  value={impact.end || ''}
                  onChange={(e) =>
                    setImpact({ ...impact, end: e.target.value })
                  }
                  fullWidth
                  InputLabelProps={{
                    shrink: true,
                  }}
                />
              </Grid>
              <Grid item xs={12}>
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
              </Grid>
              <Grid item xs={12}>
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
              </Grid>
              {impact.impactTypes && (
                <Grid item xs={12}>
                  <FormControl fullWidth>
                    <InputLabel id="impact-types-label">
                      Impact Types
                    </InputLabel>
                    <Select
                      labelId="impact-types-label"
                      id="impact-types-select"
                      multiple
                      value={impact.impactTypes?.map((type) => type.id)}
                      onChange={handleChange}
                      input={
                        <OutlinedInput
                          id="select-multiple-chip"
                          label="Impact Types"
                        />
                      }
                      renderValue={(selected) => (
                        <Box
                          sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}
                        >
                          {selected.map((value) => (
                            <Chip
                              key={value}
                              label={
                                impactTypeData?.find(
                                  (type) => type.id === value
                                )?.name
                              }
                            />
                          ))}
                        </Box>
                      )}
                      MenuProps={MenuProps}
                    >
                      <MenuItem
                        onClick={() => router.push('/admin/impact-type')}
                      >
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
                            fontWeight: impact.impactTypes?.some(
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
                </Grid>
              )}
              <Grid item xs={12}>
                <TextField
                  label="Segments"
                  name="segmentIds"
                  value={'segments'}
                  fullWidth
                />
              </Grid>
            </Grid>
          </Grid>
          <Grid item xs={7}>
            {/* {segmentData && <SegmentsSelectorMap
      segments={segmentData} // Assuming segmentData is the array of segments
      initialSelectedSegmentIds={impact.segmentIds} // Pass the initial selected segment IDs
      onUpdateSelectedSegments={(selectedIds) => {
        setImpact({ ...impact, segmentIds: selectedIds });
      }}
    />} */}
            {/* <SegmentSelectMap /> */}

            {/* <div
              style={{
                width: '100%',
                height: '100%',
                backgroundColor: '#eee',
                borderRadius: '5px',
              }}
            >
              <div style={{ padding: '15px' }}>Map Placeholder</div>
            </div> */}
          </Grid>
        </Grid>
      </Box>
      <DialogActions>
        <Button onClick={onClose} color="secondary">
          Cancel
        </Button>
        <Button onClick={handleSave} color="primary">
          Save
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default ImpactEditorModal
