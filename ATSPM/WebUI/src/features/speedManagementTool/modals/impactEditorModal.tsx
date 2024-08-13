import React, { useState, useEffect } from 'react'
import {
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Button,
  TextField,
  Grid,
  Box,
} from '@mui/material'
import { Impact } from '@/features/speedManagementTool/types/impact'
import { useGetImpactTypes } from '../api/getImpactTypes'
import { string } from 'zod'

interface ImpactEditorModalProps {
  data?: Impact
  open: boolean
  onClose: () => void
  onCreate: (impact: Impact) => void
  onEdit: (impact: Impact) => void
  onDelete?: (id: string) => void
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
    impactTypeIds: data?.impactTypeIds || null,
    impactTypes: data?.impactTypes || null,
    segmentIds: data?.segmentIds || [],
  })

  const { data: impactTypeData, isLoading:isLoadingImpactTypes } = useGetImpactTypes()
if (!isLoadingImpactTypes){
  console.log(impactTypeData)
}

  useEffect(() => {
    if (data) {
      setImpact(data)
    }
  }, [data])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setImpact({ ...impact, [name]: value })
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
      <Box sx={{marginX:'1.25rem'}}>
      <h4>{impact.id ? 'Edit Impact' : 'Create New Impact'}</h4>
        <Grid container spacing={2}>
          <Grid item xs={5}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  label="Description"
                  name="description"
                  value={impact.description}
                  onChange={handleChange}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Start Date"
                  name="start"
                  type="datetime-local"
                  value={impact.start}
                  onChange={handleChange}
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
                  value={impact.end}
                  onChange={handleChange}
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
                  onChange={handleChange}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="End Mile"
                  name="endMile"
                  type="number"
                  value={impact.endMile}
                  onChange={handleChange}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Impact Types"
                  name="impactTypes"
                  value={impact.impactTypes || ""}
                  fullWidth
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  label="Segments"
                  name="segmentIds"
                  value={impact.segmentIds}
                  // onChange={}
                  fullWidth
                />
              </Grid>
            </Grid>
          </Grid>
          <Grid item xs={7}>
            {/* Your map component goes here */}
            <div style={{ width: '100%', height: '100%', backgroundColor: '#eee', borderRadius:'5px' }}>
              <div style={{padding:'15px'}}>
              Map Placeholder
              </div>
            </div>
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
