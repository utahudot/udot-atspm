import {
  useGetAggregationDataTypes, // Record<string, DataTypeMetaDto>
  useGetEventLogDataTypes, // Record<string, DataTypeMetaDto>
} from '@/api/data'
import OptionsWrapper from '@/components/OptionsWrapper'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import {
  Box,
  Collapse,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Skeleton,
  Tooltip,
  Typography,
} from '@mui/material'
import { useMemo, useState } from 'react'

export interface FieldMeta {
  name: string
  description?: string | null
}

export interface DataTypeOption {
  name: string
  displayName: string
  description?: string | null
  fields: FieldMeta[]
  type: 'raw' | 'aggregation'
}

interface SelectDataTypeProps {
  selectedDataType: DataTypeOption | null
  setSelectedDataType: (dataType: DataTypeOption | null) => void
}

// matches backend DataTypeMetaDto
type DataTypeMeta = {
  displayName: string
  description?: string | null
  fields: Array<{
    name: string
    description?: string | null
  }>
}

export const DataTypeSelector = ({
  selectedDataType,
  setSelectedDataType,
}: SelectDataTypeProps) => {
  const { data: aggData, isLoading: aggLoading } = useGetAggregationDataTypes()
  const { data: rawData, isLoading: rawLoading } = useGetEventLogDataTypes()

  const rawOptions = useMemo(
    () => toOptions(rawData as Record<string, DataTypeMeta> | undefined, 'raw'),
    [rawData]
  )
  const aggregateOptions = useMemo(
    () =>
      toOptions(
        aggData as Record<string, DataTypeMeta> | undefined,
        'aggregation'
      ),
    [aggData]
  )

  // which items have their details expanded
  const [detailsOpen, setDetailsOpen] = useState<Record<string, boolean>>({})

  const handleSelect = (opt: DataTypeOption) => setSelectedDataType(opt)

  const toggleDetails = (key: string) =>
    setDetailsOpen((s) => ({ ...s, [key]: !s[key] }))

  const renderDetails = (opt: DataTypeOption, key: string) => {
    const typeDescription = opt.description?.trim()
    const fieldNames = (opt.fields ?? [])
      .map((f) => f?.name)
      .filter(Boolean) as string[]

    const sorted = [...fieldNames].sort((a, b) => a.localeCompare(b))

    return (
      <Collapse in={!!detailsOpen[key]} timeout="auto" unmountOnExit>
        <Box sx={{ pl: 3, pr: 2, pb: 1.25 }}>
          {typeDescription && (
            <Typography variant="caption" sx={{ display: 'block', mb: 1 }}>
              {typeDescription}
            </Typography>
          )}

          <Paper
            sx={{
              px: 2,
              py: 1,
              my: 1,
              bgcolor: 'background.default',
              flexGrow: 1,
              display: 'flex',
              flexDirection: 'column',
            }}
            elevation={0}
          >
            <Typography variant="caption">Fields</Typography>

            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                flexGrow: 1,
                mt: 0.75,
                gap: 0.5,
              }}
            >
              <Divider sx={{ mb: 0.5 }} />
              {sorted.length ? (
                sorted.map((name) => (
                  <Typography
                    key={name}
                    variant="caption"
                    sx={{
                      color: 'black',
                      lineHeight: 1.3,
                    }}
                  >
                    {name}
                  </Typography>
                ))
              ) : (
                <Typography variant="caption" sx={{ opacity: 0.75 }}>
                  —
                </Typography>
              )}
            </Box>
          </Paper>
        </Box>
      </Collapse>
    )
  }

  const renderRow = (opt: DataTypeOption) => {
    const key = `${opt.type}:${opt.name}`
    const hasAnyDetails =
      !!opt.description?.trim() || (opt.fields ?? []).some((f) => !!f?.name)
    const isDetailsOpen = !!detailsOpen[key]

    return (
      <Box key={key}>
        <ListItemButton
          onClick={() => handleSelect(opt)}
          selected={
            selectedDataType?.type === opt.type &&
            selectedDataType?.name === opt.name
          }
          sx={{ pl: 3 }}
        >
          <ListItemText primary={opt.displayName} />

          {hasAnyDetails && (
            <Tooltip
              title={isDetailsOpen ? 'Hide details' : 'Show details'}
              arrow
            >
              <IconButton
                size="small"
                onClick={(e) => {
                  e.stopPropagation()
                  toggleDetails(key)
                }}
                sx={{ ml: 1 }}
              >
                <InfoOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </ListItemButton>

        {hasAnyDetails && renderDetails(opt, key)}
      </Box>
    )
  }

  return (
    <Box display="flex" flexDirection="column" width={'450px'}>
      <OptionsWrapper header="Data Types" noPadding>
        <Box sx={{ height: '350px', overflow: 'auto' }}>
          <List disablePadding>
            {/* RAW (always open) */}
            <Box sx={{ px: 2, pt: 1.25, pb: 0.75 }}>
              <Typography sx={{ fontSize: '.9rem', fontWeight: 'bold' }}>
                Raw ({rawOptions.length})
              </Typography>
            </Box>

            {rawLoading ? (
              <Box sx={{ px: 2, py: 1 }}>
                <Skeleton height={160} />
              </Box>
            ) : (
              <List disablePadding>{rawOptions.map(renderRow)}</List>
            )}

            {/* AGGREGATIONS (always open) */}
            <Box
              sx={{
                px: 2,
                pt: 1.25,
                pb: 0.75,
                borderTop: '1px solid',
                borderColor: 'divider',
              }}
            >
              <Typography sx={{ fontSize: '.9rem', fontWeight: 'bold' }}>
                Aggregations ({aggregateOptions.length})
              </Typography>
            </Box>

            {aggLoading ? (
              <Box sx={{ px: 2, py: 1 }}>
                <Skeleton height={160} />
              </Box>
            ) : (
              <List disablePadding>{aggregateOptions.map(renderRow)}</List>
            )}
          </List>
        </Box>
      </OptionsWrapper>
    </Box>
  )
}

const toOptions = (
  data: Record<string, DataTypeMeta> | undefined,
  type: DataTypeOption['type']
): DataTypeOption[] =>
  data
    ? Object.entries(data).map(([modelName, meta]) => ({
        name: modelName,
        displayName: meta?.displayName || modelName,
        description: meta?.description ?? null,
        fields: (meta?.fields ?? []).map((f) => ({
          name: f.name,
          description: f.description ?? null,
        })),
        type,
      }))
    : []
