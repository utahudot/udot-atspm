import {
  usePostApiV1EntityFileGeojsonFetchFile,
  usePostApiV1EntityFileShapefileFetchFile,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import { useNotificationStore } from '@/stores/notifications'
import { dateToTimestamp } from '@/utils/dateTime'
import { zodResolver } from '@hookform/resolvers/zod'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import {
  Alert,
  Box,
  Button,
  IconButton,
  Snackbar,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

type Mode = 'geojson' | 'shapefile'

// ---- forms
const convertSchema = z.object({
  bucketName: z.string().min(1, 'Bucket name is required'),
  fileName: z.string().min(1, 'File name is required'), // .zip or .shp
})
type ConvertForm = z.infer<typeof convertSchema>

const processSchema = z.object({
  bucketName: z.string().min(1, 'Bucket name is required'),
  fileName: z.string().min(1, 'File name is required'), // .geojson
  startDate: z.date(),
  endDate: z.date().optional(),
})
type ProcessForm = z.infer<typeof processSchema>

function predictGeoPath(shpPath: string) {
  const last = shpPath.split('/').pop() ?? shpPath
  const base = last.replace(/\.(zip|shp)$/i, '')
  return shpPath.replace(last, `${base}.geojson`)
}

export default function ClearGuideSourcePanel() {
  const { addNotification } = useNotificationStore()
  const [mode, setMode] = useState<Mode>('geojson')
  const [predicted, setPredicted] = useState<{
    bucket: string
    geoPath: string
  } | null>(null)
  const [copied, setCopied] = useState(false)

  const { mutateAsync: startConvert, isLoading: converting } =
    usePostApiV1EntityFileShapefileFetchFile()
  const { mutateAsync: processGeo, isLoading: processing } =
    usePostApiV1EntityFileGeojsonFetchFile()

  // Convert form
  const {
    register: regConv,
    handleSubmit: handleSubmitConv,
    reset: resetConv,
    watch: watchConv,
  } = useForm<ConvertForm>({
    resolver: zodResolver(convertSchema),
    defaultValues: { bucketName: '', fileName: '' },
  })
  const convVals = watchConv()
  const predictedPath = useMemo(
    () => (convVals.fileName ? predictGeoPath(convVals.fileName) : ''),
    [convVals.fileName]
  )

  // GeoJSON form
  const {
    register: regProc,
    control,
    handleSubmit: handleSubmitProc,
    reset: resetProc,
    setValue: setProcValue,
  } = useForm<ProcessForm>({
    resolver: zodResolver(processSchema),
    defaultValues: { bucketName: '', fileName: '', startDate: undefined },
  })

  // Actions
  const onConvert = async (v: ConvertForm) => {
    try {
      await startConvert({
        params: { bucketName: v.bucketName, fileName: v.fileName, sourceId: 3 },
      })
      const geoPath = predictGeoPath(v.fileName)
      setPredicted({ bucket: v.bucketName, geoPath })
      addNotification({
        title: 'Shapefile conversion started',
        type: 'success',
      })
    } catch (e: any) {
      addNotification({
        title:
          e?.response?.data?.message ??
          e?.message ??
          'Failed to start conversion',
        type: 'error',
      })
    }
  }

  const onProcess = async (v: ProcessForm) => {
    try {
      await processGeo({
        params: {
          bucketName: v.bucketName,
          fileName: v.fileName,
          startDate: dateToTimestamp(v.startDate),
          sourceId: 3,
        },
      })
      addNotification({
        title: 'GeoJSON submitted for processing',
        type: 'success',
      })
      resetProc({ ...v })
    } catch (e: any) {
      addNotification({
        title:
          e?.response?.data?.message ??
          e?.message ??
          'Failed to submit GeoJSON',
        type: 'error',
      })
    }
  }

  const fillAndSwitchToGeo = () => {
    if (!predicted) return
    setMode('geojson')
    setProcValue('bucketName', predicted.bucket)
    setProcValue('fileName', predicted.geoPath)
  }

  const tryProcessPredictedNow = async () => {
    if (!predicted) return
    await onProcess({
      bucketName: predicted.bucket,
      fileName: predicted.geoPath,
      startDate: new Date(), // user can change after
      endDate: undefined,
    })
  }

  const busy = converting || processing

  return (
    <>
      <Stack spacing={2} sx={{ maxWidth: 400, mb: 3 }}>
        <Typography variant="h6">ClearGuide</Typography>
        <Stack direction="row" spacing={1}>
          <Button
            variant={mode === 'geojson' ? 'contained' : 'outlined'}
            onClick={() => setMode('geojson')}
          >
            GeoJSON
          </Button>
          <Button
            variant={mode === 'shapefile' ? 'contained' : 'outlined'}
            onClick={() => setMode('shapefile')}
          >
            Shapefile
          </Button>
        </Stack>
      </Stack>

      {/* GeoJSON — single step */}
      {mode === 'geojson' && (
        <Box
          component="form"
          onSubmit={handleSubmitProc(onProcess)}
          noValidate
          sx={{ maxWidth: 640 }}
        >
          <Typography variant="h6" sx={{ mb: 2 }}>
            Process GeoJSON
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
              {...regProc('bucketName')}
              placeholder="e.g., my-bucket"
            />
            <TextField
              label="GeoJSON Path"
              fullWidth
              required
              {...regProc('fileName')}
              placeholder="e.g., path/to/file.geojson"
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
              <Button type="submit" variant="contained" disabled={busy}>
                {processing ? 'Submitting…' : 'Submit'}
              </Button>
              <Button
                type="button"
                variant="outlined"
                onClick={() =>
                  resetProc({
                    bucketName: '',
                    fileName: '',
                    startDate: new Date(),
                  })
                }
                disabled={busy}
              >
                Reset
              </Button>
            </Stack>
          </Stack>
        </Box>
      )}

      {/* Shapefile — convert */}
      {mode === 'shapefile' && (
        <Box
          component="form"
          onSubmit={handleSubmitConv(onConvert)}
          noValidate
          sx={{ maxWidth: 640 }}
        >
          <Typography variant="h6" sx={{ mb: 2 }}>
            Convert Shapefile to GeoJSON
          </Typography>
          <Alert severity="info" sx={{ mb: 3 }}>
            This page does not upload files. Place your shapefile and other
            necessary files (.shx and .dbf) in your cloud bucket, then provide
            the bucket and object path below to process and create a new GeoJSON
            file. After conversion, you can use the GeoJSON in the other tab to
            create a new dataset version.
          </Alert>
          <Stack spacing={2.5}>
            <TextField
              label="Bucket Name"
              fullWidth
              required
              {...regConv('bucketName')}
              placeholder="e.g., my-bucket"
            />
            <TextField
              label="Shapefile Path"
              fullWidth
              required
              {...regConv('fileName')}
              placeholder="e.g., path/to/archive.zip or path/to/file.shp"
              helperText={
                predictedPath
                  ? `Predicted GeoJSON path: ${predictedPath}`
                  : undefined
              }
            />
            <Stack direction="row" spacing={1.5}>
              <Button type="submit" variant="contained" disabled={busy}>
                {converting ? 'Starting…' : 'Start Conversion'}
              </Button>
              <Button
                type="button"
                variant="outlined"
                onClick={() => resetConv({ bucketName: '', fileName: '' })}
                disabled={busy}
              >
                Reset
              </Button>
            </Stack>
          </Stack>

          {predicted && (
            <Alert severity="success" sx={{ mt: 3 }}>
              <Stack
                direction="row"
                alignItems="center"
                spacing={1}
                sx={{ flexWrap: 'wrap' }}
              >
                <Typography variant="body2" sx={{ mr: 1 }}>
                  Predicted GeoJSON: <strong>{predicted.geoPath}</strong>
                </Typography>
                <IconButton
                  size="small"
                  onClick={async () => {
                    await navigator.clipboard.writeText(predicted.geoPath)
                    setCopied(true)
                  }}
                >
                  <ContentCopyIcon fontSize="small" />
                </IconButton>
                <Button size="small" onClick={fillAndSwitchToGeo}>
                  Fill GeoJSON form & switch
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  onClick={tryProcessPredictedNow}
                  disabled={busy}
                >
                  Try processing now
                </Button>
              </Stack>
            </Alert>
          )}
        </Box>
      )}

      <Snackbar
        open={copied}
        autoHideDuration={1500}
        onClose={() => setCopied(false)}
        message="Copied"
      />
    </>
  )
}
