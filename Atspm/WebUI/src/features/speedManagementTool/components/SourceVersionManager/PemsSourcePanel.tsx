import { usePostApiV1EntityFileGeojsonFetchFile } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useNotificationStore } from '@/stores/notifications'
import { dateToTimestamp } from '@/utils/dateTime'
import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Box, Button, Stack, TextField, Typography } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

const schema = z.object({
  bucketName: z.string().min(1, 'Bucket name is required'),
  fileName: z.string().min(1, 'File name is required'),
  startDate: z.date(),
  endDate: z.date().optional(),
})
type Form = z.infer<typeof schema>

export default function PemsSourcePanel() {
  const { addNotification } = useNotificationStore()
  const { mutate: processGeojson, isLoading } =
    usePostApiV1EntityFileGeojsonFetchFile()

  const { register, control, handleSubmit, reset } = useForm<Form>({
    resolver: zodResolver(schema),
    defaultValues: { bucketName: '', fileName: '', startDate: undefined },
  })

  const onSubmit = (values: Form) => {
    processGeojson(
      {
        params: {
          bucketName: values.bucketName,
          fileName: values.fileName,
          startDate: dateToTimestamp(values.startDate),
          sourceId: 2,
        },
      },
      {
        onSuccess: () => {
          addNotification({
            title: 'File submitted for processing',
            type: 'success',
          })
          reset({ ...values })
        },
        onError: (err: any) =>
          addNotification({
            title:
              err?.response?.data?.message ??
              err?.message ??
              'Failed to submit file',
            type: 'error',
          }),
      }
    )
  }

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      sx={{ maxWidth: 640 }}
    >
      <Typography variant="h6" sx={{ mb: 2 }}>
        PeMS • Process File from Google Cloud Storage
      </Typography>
      <Alert severity="info" sx={{ mb: 3 }}>
        This page does not upload files. Place your GeoJSON in your cloud
        bucket, then provide the bucket and object path below to process and
        create a new dataset version.
      </Alert>

      <Stack spacing={2.5}>
        <TextField
          label="Bucket Name"
          fullWidth
          required
          {...register('bucketName')}
          placeholder="e.g., my-bucket"
          autoComplete="off"
        />
        <TextField
          label="File Name"
          fullWidth
          required
          {...register('fileName')}
          placeholder="e.g., path/to/file.geojson"
          autoComplete="off"
        />
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Controller
            name="startDate"
            control={control}
            render={({ field }) => (
              <DatePicker
                label="Version Start Date"
                value={field.value ?? null}
                onChange={(d) => field.onChange(d ?? undefined)}
                slotProps={{ textField: { required: true } }}
              />
            )}
          />
          <Controller
            name="endDate"
            control={control}
            render={({ field }) => (
              <DatePicker
                label="Version End Date"
                value={field.value ?? null}
                onChange={(d) => field.onChange(d ?? undefined)}
              />
            )}
          />
        </Box>

        <Stack direction="row" spacing={1.5}>
          <Button type="submit" variant="contained" disabled={isLoading}>
            {isLoading ? 'Submitting…' : 'Submit'}
          </Button>
          <Button
            type="button"
            variant="outlined"
            onClick={() =>
              reset({
                bucketName: '',
                fileName: '',
                startDate: new Date(),
              })
            }
            disabled={isLoading}
          >
            Reset
          </Button>
        </Stack>
      </Stack>
    </Box>
  )
}
