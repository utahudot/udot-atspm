import {
  useGetApiV1AccessCategory,
  useGetApiV1City,
  useGetApiV1County,
  useGetApiV1FunctionalType,
  useGetApiV1Region,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  Autocomplete,
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { forwardRef, useEffect, useImperativeHandle } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

// ---------------------------
// schema
// ---------------------------

const segmentPropertiesSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  udotRouteNumber: z.string().min(1, 'Route Number is required'),
  alternateIdentifier: z.string().optional(),
  functionalType: z.string().min(1, 'Functional Type is required'),
  startMilePoint: z.number({ required_error: 'Start Mile Point is required' }),
  endMilePoint: z.number({ required_error: 'End Mile Point is required' }),
  speedLimit: z.number().min(1, 'Speed Limit must be positive'),
  offset: z.number(),
  region: z.string().min(1, 'Region is required'),
  direction: z.enum(['NB', 'EB', 'SB', 'WB'], {
    message: 'Direction is required',
  }),
  polarity: z.enum(['PM', 'NM']).optional(),
  accessCategory: z.string().min(1, 'Access Category is required'),
  city: z.string().min(1, 'City is required'),
  county: z.string().min(1, 'County is required'),
})

type SegmentPropertiesForm = z.infer<typeof segmentPropertiesSchema>

// ---------------------------
// component
// ---------------------------
const SegmentProperties = forwardRef((_, ref) => {
  const { segmentProperties, updateSegmentProperties } = useSegmentEditorStore()

  // look-ups
  const { data: countiesData } = useGetApiV1County()
  const { data: functionalTypesData } = useGetApiV1FunctionalType()
  const { data: citiesData } = useGetApiV1City()
  const { data: accessCategoriesData } = useGetApiV1AccessCategory()
  const { data: regionsData } = useGetApiV1Region()

  const cities = citiesData ?? []
  const functionalTypes = functionalTypesData ?? []
  const counties = countiesData ?? []
  const accessCategories = accessCategoriesData ?? []
  const regions = regionsData ?? []
  const directions = ['NB', 'EB', 'SB', 'WB']

  // names
  const cityNames = cities.map((c) => c.name)
  const accessCategoryNames = accessCategories.map((a) => a.name)
  const countiesNames = counties.map((c) => c.name)
  const functionalTypeNames = functionalTypes.map((f) => f.name)
  const regionNames = regions.map((r) => r.name)

  // form
  const {
    control,
    handleSubmit,
    reset,
    trigger,
    formState: { errors },
    setValue,
  } = useForm<SegmentPropertiesForm>({
    resolver: zodResolver(segmentPropertiesSchema),
    mode: 'onChange',
    reValidateMode: 'onChange',
    defaultValues: {
      name: '',
      udotRouteNumber: '',
      alternateIdentifier: '',
      functionalType: '',
      startMilePoint: '',
      endMilePoint: '',
      speedLimit: '',
      offset: '',
      region: '',
      direction: '',
      polarity: 'PM',
      accessCategory: '',
      city: '',
      county: '',
    },
  })

  // keep form in sync with store
  useEffect(() => {
    reset({
      name: segmentProperties.name ?? '',
      udotRouteNumber: segmentProperties.udotRouteNumber ?? '',
      alternateIdentifier: segmentProperties.alternateIdentifier ?? '',
      functionalType: segmentProperties.functionalType ?? '',
      startMilePoint: segmentProperties.startMilePoint ?? '',
      endMilePoint: segmentProperties.endMilePoint ?? '',
      speedLimit: segmentProperties.speedLimit ?? '',
      offset: segmentProperties.offset ?? '',
      region: segmentProperties.region ?? '',
      direction: segmentProperties.direction ?? '',
      polarity: 'PM',
      accessCategory: segmentProperties.accessCategory ?? '',
      city: segmentProperties.city ?? '',
      county: segmentProperties.county ?? '',
    })
  }, [])

  useEffect(() => {
    setValue('startMilePoint', segmentProperties.startMilePoint ?? 0)
    setValue('endMilePoint', segmentProperties.endMilePoint ?? 0)
  }, [
    segmentProperties.startMilePoint,
    segmentProperties.endMilePoint,
    setValue,
  ])

  // when user hits save inside this form (not the parent)
  const onSubmit = (data: SegmentPropertiesForm) => {
    updateSegmentProperties({ ...data, offset: data.offset ?? 0 })
  }

  // expose submitForm to the parent
  useImperativeHandle(ref, () => ({
    submitForm: async () => {
      const valid = await trigger() // ★ boolean
      if (valid) {
        await handleSubmit(onSubmit)() // ★ update store
      }
      return valid // ★ tell parent result
    },
  }))

  return (
    <Box
      sx={{
        width: 'auto',
        display: 'flex',
        flexDirection: 'column',
        gap: '10px',
        minHeight: '550px',
      }}
      component="form"
      onSubmit={handleSubmit(onSubmit)}
    >
      <Typography variant="h5" fontWeight="bold">
        Segment Properties
      </Typography>

      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, pb: 3 }}>
        <Controller
          name="name"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Name"
              required
              sx={{ flex: '1 1 45%' }}
              error={!!errors.name}
            />
          )}
        />
        <Controller
          name="udotRouteNumber"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Route Number"
              required
              sx={{ flex: '1 1 45%' }}
              error={!!errors.udotRouteNumber}
            />
          )}
        />
        <Controller
          name="alternateIdentifier"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="PeMS Identifier"
              sx={{ flex: '1 1 45%' }}
              error={!!errors.alternateIdentifier}
            />
          )}
        />
        <Controller
          name="functionalType"
          control={control}
          render={({ field }) => {
            const opts = functionalTypeNames
            const selected = opts.includes(field.value) ? field.value : null

            return (
              <Autocomplete
                options={opts}
                freeSolo
                value={selected}
                inputValue={field.value}
                onChange={(_, v) => field.onChange(v ?? '')}
                onInputChange={(_, v) => field.onChange(v)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Functional Types"
                    required
                    error={!!errors.functionalType}
                  />
                )}
                sx={{ flex: '1 1 45%' }}
              />
            )
          }}
        />
        <Controller
          name="direction"
          control={control}
          render={({ field }) => {
            const opts = directions
            const selected = opts.includes(field.value) ? field.value : null

            return (
              <Autocomplete
                options={opts}
                freeSolo
                value={selected}
                inputValue={field.value}
                onChange={(_, v) => field.onChange(v ?? '')}
                onInputChange={(_, v) => field.onChange(v)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Direction"
                    required
                    error={!!errors.direction}
                  />
                )}
                sx={{ flex: '1 1 45%' }}
              />
            )
          }}
        />
        <Controller
          name="polarity"
          control={control}
          defaultValue="PM"
          render={({ field }) => (
            <FormControl sx={{ flex: '1 1 45%' }}>
              <InputLabel id="polarity-label">Polarity</InputLabel>
              <Select
                labelId="polarity-label"
                label="Polarity"
                {...field}
                value={field.value}
              >
                <MenuItem value="PM">PM</MenuItem>
                <MenuItem value="NM">NM</MenuItem>
              </Select>
            </FormControl>
          )}
        />
        <Controller
          name="accessCategory"
          control={control}
          render={({ field }) => {
            const opts = accessCategoryNames
            const selected = opts.includes(field.value) ? field.value : null

            return (
              <Autocomplete
                options={opts}
                freeSolo
                value={selected}
                inputValue={field.value}
                onChange={(_, v) => field.onChange(v ?? '')}
                onInputChange={(_, v) => field.onChange(v)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Access Category"
                    required
                    error={!!errors.accessCategory}
                  />
                )}
                sx={{ flex: '1 1 45%' }}
              />
            )
          }}
        />

        <Controller
          name="startMilePoint"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Start Mile Point"
              type="number"
              required
              sx={{ flex: '1 1 45%' }}
              error={!!errors.startMilePoint}
              onChange={(e) => field.onChange(Number(e.target.value))}
            />
          )}
        />
        <Controller
          name="endMilePoint"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="End Mile Point"
              type="number"
              required
              sx={{ flex: '1 1 45%' }}
              error={!!errors.endMilePoint}
              onChange={(e) => field.onChange(Number(e.target.value))}
            />
          )}
        />
        <Controller
          name="speedLimit"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Speed Limit"
              type="number"
              required
              sx={{ flex: '1 1 45%' }}
              error={!!errors.speedLimit}
              onChange={(e) => field.onChange(Number(e.target.value))}
            />
          )}
        />
        <Controller
          name="offset"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Offset"
              type="number"
              sx={{ flex: '1 1 45%' }}
              error={!!errors.offset}
              onChange={(e) => field.onChange(Number(e.target.value))}
            />
          )}
        />
        <Controller
          name="region"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Region"
              select
              sx={{ flex: '1 1 45%' }}
              value={regionNames.find((name) => name === field.value) ?? ''}
              onChange={(e) => field.onChange(e.target.value)}
            >
              {regionNames.map((name) => (
                <MenuItem key={name} value={name}>
                  {name}
                </MenuItem>
              ))}
            </TextField>
          )}
        />

        <Controller
          name="city"
          control={control}
          render={({ field }) => {
            const opts = cityNames
            const selected = opts.includes(field.value) ? field.value : null

            return (
              <Autocomplete
                options={opts}
                freeSolo
                value={selected}
                inputValue={field.value}
                onChange={(_, v) => field.onChange(v ?? '')}
                onInputChange={(_, v) => field.onChange(v)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="City"
                    required
                    error={!!errors.city}
                  />
                )}
                sx={{ flex: '1 1 45%' }}
              />
            )
          }}
        />
        <Controller
          name="county"
          control={control}
          render={({ field }) => {
            const opts = countiesNames
            const selected = opts.includes(field.value) ? field.value : null

            return (
              <Autocomplete
                options={opts}
                freeSolo
                value={selected}
                inputValue={field.value}
                onChange={(_, v) => field.onChange(v ?? '')}
                onInputChange={(_, v) => field.onChange(v)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="County"
                    required
                    error={!!errors.county}
                  />
                )}
                sx={{ flex: '1 1 45%' }}
              />
            )
          }}
        />
        <Box sx={{ flex: '1 1 45%' }} />
      </Box>
    </Box>
  )
})

export default SegmentProperties

SegmentProperties.displayName = 'SegmentProperties'
