import { useGetDeviceConfigurationEventLogDecoders } from '@/api/config'
import { knownKeys } from '@/features/devices/components/DeviceConfigCustomRenderCell'
import { DeviceConfiguration } from '@/features/devices/types/index'
import { useGetProducts } from '@/features/products/api'
import { ConfigEnum, useConfigEnums } from '@/hooks/useConfigEnums'
import { zodResolver } from '@hookform/resolvers/zod'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'

import DeleteIcon from '@mui/icons-material/Delete'
import {
  Alert,
  Box,
  Button,
  Chip,
  ClickAwayListener,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormHelperText,
  IconButton,
  InputAdornment,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import { z } from 'zod'

interface ModalProps {
  data?: DeviceConfiguration
  isOpen: boolean
  onClose: () => void
  onSave: (device: DeviceConfiguration) => void
}

const deviceConfigSchema = z.object({
  id: z.number().nullable().optional(),
  description: z.string().min(1),
  notes: z.string().optional(),
  protocol: z.string().min(1),
  port: z.number().min(1),
  path: z.string().min(1),
  query: z.array(z.string()),
  connectionProperties: z
    .array(
      z.object({
        key: z.string(),
        value: z.string(),
      })
    )
    .nullable(),
  connectionTimeout: z.number(),
  operationTimeout: z.number(),
  loggingOffset: z.number().nullable(),
  decoders: z.array(z.string()).nullable(),
  userName: z.string(),
  password: z.string(),
  productId: z.number({ required_error: 'Product is required' }).nullable(),
})

type DeviceConfigFormData = z.infer<typeof deviceConfigSchema>

const DeviceConfigModal = ({
  data: deviceConfiguration,
  isOpen,
  onClose,
  onSave,
}: ModalProps) => {
  // State for query suggestions
  // State and helper arrays
  const [suggestionsOpen, setSuggestionsOpen] = useState(false)
  const [currentQueryIndex, setCurrentQueryIndex] = useState<number | null>(
    null
  )
  const [pathSuggestionsOpen, setPathSuggestionsOpen] = useState(false)
  const [formatGroup, setFormatGroup] = useState<
    'DateTime' | 'LogStartTime' | null
  >(null)

  const deviceOptions = [
    'DeviceIdentifier',
    'Ipaddress',
    'Notes',
    'LoggingEnabled',
    'DeviceStatus',
    'DeviceType',
    'LocationId',
    'DeviceConfigurationId',
    'Id',
  ]

  const dateTimeFormats = [
    'yyyy-MM-dd',
    'MM/dd/yyyy',
    'yyyy-MM-dd HH:mm:ss',
    'MM/dd/yyyy HH:mm:ss',
    '{Custom Format}',
  ]

  const handleSuggestionClick = (token: string) => {
    if (pathSuggestionsOpen) {
      const currentValue = getValues('path')
      const newValue = currentValue.endsWith('[')
        ? currentValue.slice(0, -1) + token
        : currentValue + token
      setValue('path', newValue)
      setPathSuggestionsOpen(false)
      setFormatGroup(null)
    } else if (currentQueryIndex !== null) {
      const currentValue = getValues(`query.${currentQueryIndex}`)
      const newValue = currentValue.endsWith('[')
        ? currentValue.slice(0, -1) + token
        : currentValue + token
      setValue(`query.${currentQueryIndex}`, newValue)
      setSuggestionsOpen(false)
      setCurrentQueryIndex(null)
      setFormatGroup(null)
    }
  }

  const { data: productData } = useGetProducts()
  const { data: allDecodersData } = useGetDeviceConfigurationEventLogDecoders()
  const { data: transportProtocols } = useConfigEnums(
    ConfigEnum.TransportProtocols
  )
  const defaultConnectionProperties = deviceConfiguration
    ? Object.entries(deviceConfiguration)
        .filter(([key]) => !knownKeys.has(key))
        .map(([key, value]) => ({ key, value: String(value) }))
    : null

  const {
    register,
    handleSubmit,
    setValue,
    getValues,
    watch,
    control,
    formState: { errors },
  } = useForm<DeviceConfigFormData>({
    resolver: zodResolver(deviceConfigSchema),
    defaultValues: {
      description: deviceConfiguration?.description,
      notes: deviceConfiguration?.notes || '',
      protocol: deviceConfiguration?.protocol || '',
      port: deviceConfiguration?.port,
      path: deviceConfiguration?.path || '',
      query: deviceConfiguration?.query,
      connectionTimeout: deviceConfiguration?.connectionTimeout,
      operationTimeout: deviceConfiguration?.operationTimeout,
      loggingOffset: deviceConfiguration?.loggingOffset ?? 0,
      decoders: deviceConfiguration?.decoders || [],
      userName: deviceConfiguration?.userName || '',
      password: deviceConfiguration?.password || '',
      productId: deviceConfiguration?.productId,
      id: deviceConfiguration?.id ?? null,
      // Use the computed extra properties
      connectionProperties: defaultConnectionProperties ?? null,
    },
  })

  useEffect(() => {
    if (!deviceConfiguration) return

    setValue('protocol', deviceConfiguration.protocol)
    setValue('productId', deviceConfiguration.productId ?? null)

    Object.entries(deviceConfiguration).forEach(([key, value]) => {
      if (key !== 'protocol' && key !== 'productId') {
        setValue(key as keyof DeviceConfigFormData, value as any)
      }
    })
  }, [deviceConfiguration, setValue])

  const onSubmit = async (data: DeviceConfigFormData) => {
    try {
      const selectedProduct = productData?.value.find(
        (product) => product.id === data.productId
      )

      const { connectionProperties, ...rest } = data

      const flattenedConnectionProperties = Array.isArray(connectionProperties)
        ? connectionProperties.reduce(
            (acc, { key, value }) => {
              if (key) acc[key] = value
              return acc
            },
            {} as Record<string, any>
          )
        : connectionProperties

      const mergedDevice = {
        ...rest,
        ...flattenedConnectionProperties,
        productName: selectedProduct?.model || '',
      }

      onSave(mergedDevice as DeviceConfiguration)
      onClose()
    } catch (error) {
      console.error('Error occurred while editing/creating device:', error)
    }
  }

  const {
    fields: queryFields,
    append: appendQuery,
    remove: removeQuery,
  } = useFieldArray({
    control,
    name: 'query',
  })

  const {
    fields: connectionFields,
    append: appendConnection,
    remove: removeConnection,
  } = useFieldArray({
    control,
    name: 'connectionProperties',
  })

  return (
    <Dialog open={isOpen} onClose={onClose}>
      <DialogTitle sx={{ fontSize: '1.3rem' }} id="role-permissions-label">
        Device Configuration Details
      </DialogTitle>

      <DialogContent sx={{ width: '700px', maxWidth: '100%' }}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <FormControl fullWidth margin="dense" error={!!errors.productId}>
              <InputLabel id="product-label">Product*</InputLabel>
              <Select
                labelId="product-label"
                id="product-select"
                label="Product*"
                error={!!errors.productId}
                value={watch('productId') || ''}
                onChange={(e) =>
                  setValue('productId', Number(e.target.value), {
                    shouldValidate: true,
                  })
                }
              >
                {productData?.value.map((product) => (
                  <MenuItem key={product.id} value={product.id}>
                    {product.model}
                  </MenuItem>
                ))}
              </Select>
              {errors.productId && (
                <FormHelperText>
                  {errors.productId?.message || 'Product Required'}
                </FormHelperText>
              )}
            </FormControl>
            <TextField
              {...register('description')}
              autoFocus
              margin="dense"
              id="description"
              label="Description*"
              type="text"
              fullWidth
              error={!!errors.description}
              helperText={errors.description ? 'Description Required' : ''}
            />
          </Box>

          <Paper
            variant="outlined"
            sx={{ my: 2, bgcolor: 'background.default', p: 2 }}
          >
            <Alert severity="info" variant="outlined">
              <Typography
                sx={{ fontSize: '0.8rem' }}
                variant="caption"
                component="div"
              >
                You can insert dynamic values for both the path and query inputs
                by enclosing them in square brackets with either{' '}
                <strong>Device</strong> or <strong>DateTime</strong>, followed
                by a colon and the property or format. For example,{' '}
                <code>[Device:Ipaddress]</code>
                for device properties, or <code>[DateTime:yyyy-MM-dd]</code> for
                .NET-supported date/time formats. See{' '}
                <a
                  href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Microsoft’s documentation
                </a>{' '}
                for details about date/time formats.
              </Typography>
            </Alert>
            <InputLabel sx={{ mt: 2 }}>Base URL Path</InputLabel>
            <Box sx={{ position: 'relative' }}>
              <TextField
                {...register('path')}
                margin="dense"
                id="path"
                label="Path*"
                type="text"
                fullWidth
                error={!!errors.path}
                helperText={errors.path ? 'Path Required' : ''}
                onKeyUp={(e) => {
                  if (e.key === '[') {
                    setPathSuggestionsOpen(true)
                  }
                }}
              />
              {pathSuggestionsOpen && (
                <ClickAwayListener
                  onClickAway={() => {
                    setPathSuggestionsOpen(false)
                    setFormatGroup(null)
                  }}
                >
                  <Box
                    sx={{
                      position: 'absolute',
                      top: '100%',
                      left: 0,
                      zIndex: 1000,
                      bgcolor: 'background.paper',
                      border: '1px solid #ccc',
                      borderRadius: 1,
                      p: 1,
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 1,
                      overflowY: 'auto',
                      maxHeight: '200px',
                      width: '80%',
                    }}
                  >
                    {formatGroup === null ? (
                      <>
                        {deviceOptions.map((opt) => (
                          <Typography
                            key={`Device-${opt}`}
                            onClick={() =>
                              handleSuggestionClick(`[Device:${opt}]`)
                            }
                            sx={{
                              cursor: 'pointer',
                              p: 0.5,
                              textAlign: 'left',
                            }}
                          >
                            [Device:{opt}]
                          </Typography>
                        ))}
                        <Typography
                          key="DateTime"
                          onClick={() => setFormatGroup('DateTime')}
                          sx={{
                            cursor: 'pointer',
                            p: 0.5,
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                            width: '100%',
                          }}
                        >
                          <span>[DateTime]</span>
                          <ChevronRightIcon />
                        </Typography>

                        <Typography
                          key="LogStartTime"
                          onClick={() => setFormatGroup('LogStartTime')}
                          sx={{
                            cursor: 'pointer',
                            p: 0.5,
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                            width: '100%',
                          }}
                        >
                          <span>[LogStartTime]</span>
                          <ChevronRightIcon />
                        </Typography>
                      </>
                    ) : (
                      <>
                        <Button
                          onClick={() => setFormatGroup(null)}
                          variant="text"
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            alignSelf: 'flex-start',
                            minWidth: 'auto',
                            p: 0,
                          }}
                        >
                          <ChevronLeftIcon />
                          <span style={{ marginLeft: 4 }}>Back</span>
                        </Button>
                        {dateTimeFormats.map((fmt) => (
                          <Typography
                            key={`${formatGroup}-${fmt}`}
                            onClick={() =>
                              handleSuggestionClick(`[${formatGroup}:${fmt}]`)
                            }
                            sx={{
                              cursor: 'pointer',
                              p: 0.5,
                              textAlign: 'left',
                            }}
                          >
                            [{formatGroup}:{fmt}]
                          </Typography>
                        ))}
                        <Typography
                          variant="caption"
                          component="a"
                          href="https://learn.microsoft.com/en-us/system-center/orchestrator/standard-activities/format-date-time?view=sc-orch-2025"
                          target="_blank"
                          rel="noopener noreferrer"
                          sx={{
                            textDecoration: 'underline',
                            cursor: 'pointer',
                            mt: 1,
                          }}
                        >
                          View all formats
                        </Typography>
                      </>
                    )}
                  </Box>
                </ClickAwayListener>
              )}
            </Box>
            <InputLabel sx={{ mt: 2 }}>Queries</InputLabel>

            {queryFields.map((field, index) => (
              <Box
                key={field.id}
                sx={{
                  position: 'relative',
                  mb: 0,
                  display: 'flex',
                  alignItems: 'center',
                }}
              >
                <TextField
                  {...register(`query.${index}`)}
                  margin="dense"
                  label={`Query ${index + 1}`}
                  fullWidth
                  error={!!errors.query?.[index]}
                  helperText={
                    errors.query?.[index] ? errors.query[index].message : ''
                  }
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => removeQuery(index)}
                          sx={{ ml: 1 }}
                        >
                          <DeleteIcon />
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                  onKeyUp={(e) => {
                    if (e.key === '[') {
                      setSuggestionsOpen(true)
                      setCurrentQueryIndex(index)
                    }
                  }}
                />
                {suggestionsOpen && currentQueryIndex === index && (
                  <ClickAwayListener
                    onClickAway={() => {
                      setSuggestionsOpen(false)
                      setFormatGroup(null)
                      setCurrentQueryIndex(null)
                    }}
                  >
                    <Box
                      sx={{
                        position: 'absolute',
                        top: '100%',
                        left: 0,
                        zIndex: 1000,
                        bgcolor: 'background.paper',
                        border: '1px solid #ccc',
                        borderRadius: 1,
                        p: 1,
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 1,
                        overflowY: 'auto',
                        maxHeight: '200px',
                        width: '80%',
                      }}
                    >
                      {formatGroup === null ? (
                        <>
                          {deviceOptions.map((opt) => (
                            <Typography
                              key={`Device-${opt}`}
                              onClick={() =>
                                handleSuggestionClick(`[Device:${opt}]`)
                              }
                              sx={{
                                cursor: 'pointer',
                                p: 0.5,
                                textAlign: 'left',
                              }}
                            >
                              [Device:{opt}]
                            </Typography>
                          ))}
                          <Typography
                            key="DateTime"
                            onClick={() => setFormatGroup('DateTime')}
                            sx={{
                              cursor: 'pointer',
                              p: 0.5,
                              display: 'flex',
                              justifyContent: 'space-between',
                              alignItems: 'center',
                              width: '100%',
                            }}
                          >
                            <span>[DateTime]</span>
                            <ChevronRightIcon />
                          </Typography>
                          <Typography
                            key="LogStartTime"
                            onClick={() => setFormatGroup('LogStartTime')}
                            sx={{
                              cursor: 'pointer',
                              p: 0.5,
                              display: 'flex',
                              justifyContent: 'space-between',
                              alignItems: 'center',
                              width: '100%',
                            }}
                          >
                            <span>[LogStartTime]</span>
                            <ChevronRightIcon />
                          </Typography>
                        </>
                      ) : (
                        <>
                          <Button
                            onClick={() => setFormatGroup(null)}
                            variant="text"
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              alignSelf: 'flex-start',
                              minWidth: 'auto',
                              p: 0,
                            }}
                          >
                            <ChevronLeftIcon />
                            <span style={{ marginLeft: 4 }}>Back</span>
                          </Button>
                          {dateTimeFormats.map((fmt) => (
                            <Typography
                              key={`${formatGroup}-${fmt}`}
                              onClick={() =>
                                handleSuggestionClick(`[${formatGroup}:${fmt}]`)
                              }
                              sx={{
                                cursor: 'pointer',
                                p: 0.5,
                                textAlign: 'left',
                              }}
                            >
                              [{formatGroup}:{fmt}]
                            </Typography>
                          ))}
                          <Typography
                            variant="caption"
                            component="a"
                            href="https://learn.microsoft.com/en-us/system-center/orchestrator/standard-activities/format-date-time?view=sc-orch-2025"
                            target="_blank"
                            rel="noopener noreferrer"
                            sx={{
                              textDecoration: 'underline',
                              cursor: 'pointer',
                              mt: 1,
                            }}
                          >
                            View all formats
                          </Typography>
                        </>
                      )}
                    </Box>
                  </ClickAwayListener>
                )}
              </Box>
            ))}
            <Button
              variant="outlined"
              size="small"
              onClick={() => appendQuery('')}
              sx={{ mt: 1 }}
            >
              + Query
            </Button>
          </Paper>
          <Paper
            variant="outlined"
            sx={{ my: 2, bgcolor: 'background.default', p: 2 }}
          >
            <InputLabel>Connection Properties</InputLabel>
            {connectionFields.map((field, index) => (
              <Box
                key={field.id}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                }}
              >
                <TextField
                  {...register(`connectionProperties.${index}.key`)}
                  margin="dense"
                  label={`Key ${index + 1}`}
                  fullWidth
                  error={!!errors.connectionProperties?.[index]?.key}
                  helperText={
                    errors.connectionProperties?.[index]?.key
                      ? errors.connectionProperties[index].key?.message
                      : ''
                  }
                />
                <TextField
                  {...register(`connectionProperties.${index}.value`)}
                  margin="dense"
                  label={`Value ${index + 1}`}
                  fullWidth
                  error={!!errors.connectionProperties?.[index]?.value}
                  helperText={
                    errors.connectionProperties?.[index]?.value
                      ? errors.connectionProperties[index].value?.message
                      : ''
                  }
                />
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => removeConnection(index)}
                  sx={{ mt: 1 }}
                >
                  <DeleteIcon />
                </IconButton>
              </Box>
            ))}
            <Button
              variant="outlined"
              size="small"
              sx={{ mt: 1 }}
              onClick={() => appendConnection({ key: '', value: '' })}
            >
              + Connection Property
            </Button>
          </Paper>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              {...register('port', { valueAsNumber: true })}
              margin="dense"
              id="port"
              label="Port*"
              type="number"
              fullWidth
              error={!!errors.port}
              helperText={errors.port ? 'Port Required' : ''}
            />
            <FormControl fullWidth margin="dense" error={!!errors.protocol}>
              <InputLabel id="protocol-label">Protocol*</InputLabel>
              <Select
                labelId="protocol-label"
                id="protocol-select"
                label="Protocol*"
                error={!!errors.protocol}
                value={watch('protocol') || ''}
                onChange={(e) => setValue('protocol', e.target.value)}
              >
                {transportProtocols?.map((protocol) => (
                  <MenuItem key={protocol.value} value={protocol.name}>
                    {protocol.name}
                  </MenuItem>
                ))}
              </Select>

              {errors.protocol && (
                <FormHelperText>{'Protocol Required'}</FormHelperText>
              )}
            </FormControl>
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              {...register('connectionTimeout', { valueAsNumber: true })}
              margin="dense"
              id="connectionTimeout"
              label="Connection Timeout*"
              type="number"
              fullWidth
              error={!!errors.connectionTimeout}
              helperText={
                errors.connectionTimeout ? 'Connection Timeout Required' : ''
              }
            />
            <TextField
              {...register('operationTimeout', { valueAsNumber: true })}
              margin="dense"
              id="operationTimeout"
              label="Operation Timeout*"
              type="number"
              fullWidth
              error={!!errors.operationTimeout}
              helperText={
                errors.operationTimeout ? 'Operation Timeout Required' : ''
              }
            />
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              {...register('userName')}
              margin="dense"
              id="userName"
              label="Username"
              type="text"
              fullWidth
            />
            <TextField
              {...register('password')}
              margin="dense"
              id="password"
              en
              label="Password"
              type="text"
              fullWidth
            />
          </Box>
          <TextField
            {...register('loggingOffset', { valueAsNumber: true })}
            margin="dense"
            id="loggingOffset"
            label="Logging Offset (min)"
            type="number"
            fullWidth
            error={!!errors.loggingOffset}
          />
          <FormControl fullWidth margin="dense">
            <InputLabel id="decoder-label">Decoders</InputLabel>
            <Select
              labelId="decoder-label"
              label="Decoders"
              id="decoder-select"
              multiple
              value={watch('decoders') || []}
              onChange={(e) =>
                setValue('decoders', e.target.value, { shouldValidate: true })
              }
              renderValue={(selected) => (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(selected as string[]).map((value) => {
                    // Format the value by inserting a space before every capital letter except the first letter.
                    const formattedValue = value.replace(/(?!^)([A-Z])/g, ' $1')
                    return <Chip key={value} label={formattedValue} />
                  })}
                </Box>
              )}
            >
              {allDecodersData?.value?.map((decoder: string) => (
                <MenuItem key={decoder} value={decoder}>
                  {decoder.replace(/(?!^)([A-Z])/g, ' $1')}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <TextField
            {...register('notes')}
            margin="dense"
            id="notes"
            label="Notes"
            type="text"
            rows={3}
            multiline
            fullWidth
          />
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button variant="contained" type="submit">
              Save
            </Button>
          </DialogActions>
        </form>
      </DialogContent>
    </Dialog>
  )
}

export default DeviceConfigModal
