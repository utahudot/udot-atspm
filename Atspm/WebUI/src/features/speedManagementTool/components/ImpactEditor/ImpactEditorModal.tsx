import { useGetApiV1ImpactType } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { Impact } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { AllSegmentsSegment } from '@/features/speedManagementTool/api/getSegments'
import SegmentSelectMapComponent from '@/features/speedManagementTool/components/SegmentSelectMap'
import { zodResolver } from '@hookform/resolvers/zod'
import CloseIcon from '@mui/icons-material/Close'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  List,
  ListItem,
  ListItemSecondaryAction,
  MenuItem,
  OutlinedInput,
  Paper,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

interface ImpactEditorModalProps {
  data?: Impact
  open?: boolean
  onClose: () => void
  onSave: (impact: Impact) => void
  onCreate: (impact: Impact) => Promise<void>
  onEdit: (impact: Impact) => Promise<void>
  segments: AllSegmentsSegment[]
  isSegmentsLoading: boolean
}

const impactSchema = z.object({
  id: z.string().optional(),
  description: z.string().min(1, 'Description is required'),
  start: z
    .date()
    .nullable()
    .refine((date) => date !== null, 'Start Date is required'),
  end: z.date().nullable().optional(),
  startMile: z.preprocess(
    (val) => (val === '' || val === null ? undefined : Number(val)),
    z
      .number({
        required_error: 'Start Mile is required',
        invalid_type_error: 'Start Mile must be a number',
      })
      .nonnegative('Start Mile must be nonnegative')
  ),
  endMile: z.preprocess(
    (val) => (val === '' || val === null ? undefined : Number(val)),
    z
      .number({
        required_error: 'End Mile is required',
        invalid_type_error: 'End Mile must be a number',
      })
      .nonnegative('End Mile must be nonnegative')
  ),
  impactTypeIds: z
    .array(z.string())
    .nonempty('At least one Impact Type is required'),
  segmentIds: z
    .array(z.any())
    .nonempty('At least one segment must be selected'),
})

const ImpactEditorModal = ({
  data,
  open = false,
  onClose,
  onSave,
  onCreate,
  onEdit,
  segments,
  isSegmentsLoading,
}: ImpactEditorModalProps) => {
  const { data: impactTypes } = useGetApiV1ImpactType()

  const [showErrors, setShowErrors] = useState(false)

  const defaultValues = useMemo(
    () => ({
      id: data?.id,
      description: data?.description || '',
      start: data?.start ? new Date(data.start) : null,
      end: data?.end ? new Date(data.end) : null,
      startMile: data?.startMile ?? null,
      endMile: data?.endMile ?? null,
      impactTypeIds: data?.impactTypes?.map((it) => it?.id) || [],
      segmentIds: data?.segmentIds || [],
    }),
    [data]
  )

  const {
    control,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
    watch,
    trigger,
    clearErrors,
    getValues,
  } = useForm({
    resolver: zodResolver(impactSchema),
    defaultValues,
    mode: 'onSubmit',
  })

  const selectedSegmentIds = watch('segmentIds')

  useEffect(() => {
    if (data) {
      Object.keys(defaultValues).forEach((key) => {
        setValue(
          key as keyof typeof defaultValues,
          defaultValues[key as keyof typeof defaultValues]
        )
      })
    }
  }, [data, setValue, defaultValues])

  const handleSegmentSelect = useCallback(
    (segment: AllSegmentsSegment) => {
      const currentSegmentIds = selectedSegmentIds || []
      const segmentId = segment?.id

      const updatedSegmentIds = currentSegmentIds.includes(segmentId)
        ? currentSegmentIds.filter((id) => id !== segmentId)
        : [...currentSegmentIds, segmentId]

      const selectedSegments = segments.filter((s) =>
        updatedSegmentIds.includes(s.id)
      )

      if (selectedSegments.length > 0) {
        const minMile = Math.min(
          ...selectedSegments.map((s) => s.properties.startMilePoint)
        )
        const maxMile = Math.max(
          ...selectedSegments.map((s) => s.properties.endMilePoint)
        )
        const roundedStartMile = Math.round(minMile * 100) / 100
        const roundedEndMile = Math.round(maxMile * 100) / 100

        setValue('segmentIds', updatedSegmentIds)
        setValue('startMile', roundedStartMile)
        setValue('endMile', roundedEndMile)
      } else {
        setValue('segmentIds', updatedSegmentIds)
        setValue('startMile', null)
        setValue('endMile', null)
      }
      trigger(['segmentIds', 'startMile', 'endMile'])
    },
    [segments, selectedSegmentIds, setValue, trigger]
  )

  const handleRemoveSegment = useCallback(
    (segmentId: string) => {
      const updatedSegmentIds = selectedSegmentIds.filter(
        (id) => id !== segmentId
      )

      const selectedSegments = segments.filter((s) =>
        updatedSegmentIds.includes(s.id)
      )

      if (selectedSegments.length > 0) {
        const minMile = Math.min(
          ...selectedSegments.map((s) => s.properties.startMilePoint)
        )
        const maxMile = Math.max(
          ...selectedSegments.map((s) => s.properties.endMilePoint)
        )
        const roundedStartMile = Math.round(minMile * 100) / 100
        const roundedEndMile = Math.round(maxMile * 100) / 100

        setValue('segmentIds', updatedSegmentIds)
        setValue('startMile', roundedStartMile)
        setValue('endMile', roundedEndMile)
      } else {
        setValue('segmentIds', updatedSegmentIds)
      }
    },
    [segments, selectedSegmentIds, setValue]
  )

  const onSubmit = useCallback(
    async (formData: Impact) => {
      try {
        if (formData.id) {
          await onEdit(formData)
        } else {
          await onCreate(formData)
        }
        onSave(formData)
        onClose()
      } catch (error) {
        console.error('Error occurred while saving impact:', error)
      }
    },
    [onClose, onCreate, onEdit, onSave]
  )

  const handleFormSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault()
      setShowErrors(true)

      const formData = getValues()
      if (formData.start && !(formData.start instanceof Date)) {
        formData.start = new Date(formData.start)
      }
      if (formData.end && !(formData.end instanceof Date)) {
        formData.end = new Date(formData.end)
      }

      setValue('start', formData.start)
      setValue('end', formData.end)

      const isValid = await trigger()

      if (isValid) {
        handleSubmit(onSubmit)()
      }
    },
    [trigger, handleSubmit, onSubmit, getValues, setValue]
  )

  const handleInputChange = useCallback(
    (fieldName: string) => {
      if (errors[fieldName as keyof typeof errors]) {
        clearErrors(fieldName as keyof typeof errors)
      }
    },
    [errors, clearErrors]
  )

  const impactTypeOptions = useMemo(
    () =>
      impactTypes?.map((type) => {
        if (!type || !type.id) return null
        return (
          <MenuItem key={type.id} value={type.id}>
            {type.name}
          </MenuItem>
        )
      }) || [],
    [impactTypes]
  )

  if (!segments || segments.length === 0) return null

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
      <Box sx={{ display: 'flex', height: '100%' }}>
        {/* Left Column: Map */}
        <Box sx={{ flex: 1 }}>
          {isSegmentsLoading ? (
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                height: '100%',
              }}
            >
              <CircularProgress />
            </Box>
          ) : (
            <SegmentSelectMapComponent
              selectedSegmentIds={selectedSegmentIds}
              onSegmentSelect={handleSegmentSelect}
              segments={segments}
            />
          )}
        </Box>

        <Box
          component="form"
          onSubmit={handleFormSubmit}
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
            padding: 2,
            overflowY: 'auto',
          }}
        >
          <Typography variant="h4" sx={{ p: 1 }}>
            {data?.id ? 'Edit Impact' : 'Create New Impact'}
          </Typography>
          <Controller
            name="description"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Description"
                error={showErrors && !!errors.description}
                helperText={showErrors && errors.description?.message}
                fullWidth
                onChange={(e) => {
                  field.onChange(e)
                  handleInputChange('description')
                }}
              />
            )}
          />

          <Box sx={{ display: 'flex', gap: 2 }}>
            <Controller
              name="start"
              control={control}
              render={({ field }) => (
                <DatePicker
                  label="Start Date"
                  value={field.value}
                  onChange={(date) => {
                    field.onChange(date)
                    handleInputChange('start')
                  }}
                  slotProps={{
                    textField: {
                      error: showErrors && !!errors.start,
                      helperText: showErrors && errors.start?.message,
                      fullWidth: true,
                    },
                  }}
                />
              )}
            />
            <Controller
              name="end"
              control={control}
              render={({ field }) => (
                <DatePicker
                  label="End Date"
                  value={field.value}
                  onChange={(date) => {
                    field.onChange(date)
                    handleInputChange('end')
                  }}
                  slotProps={{
                    textField: {
                      error: showErrors && !!errors.end,
                      helperText: showErrors && errors.end?.message,
                      fullWidth: true,
                    },
                  }}
                />
              )}
            />
          </Box>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <Controller
              name="startMile"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Start Mile"
                  type="number"
                  value={field.value ?? ''}
                  error={showErrors && !!errors.startMile}
                  helperText={showErrors && errors.startMile?.message}
                  fullWidth
                  onChange={(e) => {
                    field.onChange(e)
                    handleInputChange('startMile')
                  }}
                />
              )}
            />
            <Controller
              name="endMile"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="End Mile"
                  type="number"
                  value={field.value ?? ''}
                  error={showErrors && !!errors.endMile}
                  helperText={showErrors && errors.endMile?.message}
                  fullWidth
                  onChange={(e) => {
                    field.onChange(e)
                    handleInputChange('endMile')
                  }}
                />
              )}
            />
          </Box>

          <Controller
            name="impactTypeIds"
            control={control}
            render={({ field }) => (
              <FormControl
                fullWidth
                error={showErrors && !!errors.impactTypeIds}
              >
                <InputLabel id="impact-types-label">Impact Types</InputLabel>
                <Select
                  {...field}
                  labelId="impact-types-label"
                  id="impact-types-select"
                  multiple
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
                            impactTypes?.find((type) => type.id === value)?.name
                          }
                        />
                      ))}
                    </Box>
                  )}
                  onChange={(e) => {
                    field.onChange(e)
                    handleInputChange('impactTypeIds')
                  }}
                >
                  {impactTypeOptions}
                </Select>
                {showErrors && errors.impactTypeIds && (
                  <Typography color="error" variant="caption">
                    {errors.impactTypeIds.message}
                  </Typography>
                )}
              </FormControl>
            )}
          />

          <Divider>
            <Typography variant="caption">Selected Segments</Typography>
          </Divider>
          <Box sx={{ maxHeight: '500px', overflowY: 'auto' }}>
            {selectedSegmentIds.length > 0 ? (
              <List sx={{ padding: 0 }}>
                {selectedSegmentIds.map((segmentId) => {
                  const segment = segments.find((s) => s?.id === segmentId)
                  const segmentName = segment
                    ? segment?.properties?.name
                    : `Segment ${segmentId}`

                  return (
                    <Paper key={segmentId} sx={{ padding: 1, margin: 1 }}>
                      <ListItem sx={{ paddingLeft: 0 }}>
                        {segmentName}
                        <ListItemSecondaryAction>
                          <IconButton
                            edge="end"
                            aria-label="remove"
                            onClick={() => handleRemoveSegment(segmentId)}
                          >
                            <CloseIcon />
                          </IconButton>
                        </ListItemSecondaryAction>
                      </ListItem>
                    </Paper>
                  )
                })}
              </List>
            ) : (
              <Typography>No segments selected.</Typography>
            )}
            {showErrors && errors.segmentIds && (
              <Typography color="error" variant="caption">
                {errors.segmentIds.message}
              </Typography>
            )}
          </Box>

          <Box
            sx={{
              display: 'flex',
              justifyContent: 'flex-end',
              gap: 1,
              marginTop: 'auto',
            }}
          >
            <Button onClick={onClose} variant="outlined" color="secondary">
              Cancel
            </Button>
            <LoadingButton
              loading={isSubmitting}
              type="submit"
              variant="contained"
              color="primary"
              disabled={isSubmitting}
            >
              Save
            </LoadingButton>
          </Box>
        </Box>
      </Box>
    </Dialog>
  )
}

export default ImpactEditorModal
