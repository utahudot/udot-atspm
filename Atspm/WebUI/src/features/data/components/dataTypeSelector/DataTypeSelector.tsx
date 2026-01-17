import {
  DataTypeMeta,
  useGetAggregationDataTypes,
  useGetEventLogDataTypes,
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
  Table,
  TableBody,
  TableCell,
  TableRow,
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

export const DataTypeSelector = ({
  selectedDataType,
  setSelectedDataType,
}: SelectDataTypeProps) => {
  const { data: aggData, isLoading: aggLoading } = useGetAggregationDataTypes()
  const { data: rawData, isLoading: rawLoading } = useGetEventLogDataTypes()

  const rawOptions = useMemo(() => toOptions(rawData, 'raw'), [rawData])
  const aggregateOptions = useMemo(
    () => toOptions(aggData, 'aggregation'),
    [aggData]
  )

  const [detailsOpen, setDetailsOpen] = useState<Record<string, boolean>>({})

  const handleSelect = (opt: DataTypeOption) => setSelectedDataType(opt)

  const toggleDetails = (key: string) =>
    setDetailsOpen((s) => ({ ...s, [key]: !s[key] }))

  const renderDetails = (opt: DataTypeOption, key: string) => {
    const typeDescription = opt.description?.trim()

    const sortedFields = [...(opt.fields ?? [])]
      .filter((f): f is FieldMeta => !!f?.name)
      .sort((a, b) => {
        const rank = (n: string) => {
          const k = n.toLowerCase()
          if (k === 'start') return 0
          if (k === 'end') return 1
          return 2
        }

        const ar = rank(a.name)
        const br = rank(b.name)
        if (ar !== br) return ar - br

        return a.name.localeCompare(b.name)
      })

    // pair fields into rows of 2
    const rows: Array<[FieldMeta?, FieldMeta?]> = []
    for (let i = 0; i < sortedFields.length; i += 2) {
      rows.push([sortedFields[i], sortedFields[i + 1]])
    }

    const renderFieldCell = (field?: FieldMeta) => {
      if (!field) return <Box sx={{ height: 18 }} />

      const desc = field.description?.trim()
      const hasDesc = !!desc

      const content = (
        <Typography
          variant="caption"
          sx={{
            color: 'black',
            lineHeight: 1.3,
            width: 'fit-content',
            ...(hasDesc
              ? {
                  textDecoration: 'underline',
                  textDecorationStyle: 'dotted',
                  textUnderlineOffset: '2px',
                  cursor: 'help',
                }
              : {}),
          }}
        >
          {field.name}
        </Typography>
      )

      return hasDesc ? (
        <Tooltip title={desc} arrow placement="right" enterDelay={250}>
          {/* Tooltip needs a single element */}
          <Box sx={{ display: 'inline-flex' }}>{content}</Box>
        </Tooltip>
      ) : (
        content
      )
    }

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
            <Divider sx={{ mt: 0.75, mb: 0.75 }} />

            <Table size="small" sx={{ tableLayout: 'fixed' }}>
              <TableBody>
                {rows.length ? (
                  rows.map(([left, right], idx) => (
                    <TableRow
                      key={`${left?.name ?? 'empty'}:${right?.name ?? 'empty'}:${idx}`}
                      hover
                    >
                      <TableCell
                        sx={{
                          borderBottom: 'none',
                          py: 0.5,
                          px: 0,
                          pr: 2,
                          verticalAlign: 'top',
                          width: '50%',
                        }}
                      >
                        {renderFieldCell(left)}
                      </TableCell>

                      <TableCell
                        sx={{
                          borderBottom: 'none',
                          py: 0.5,
                          px: 0,
                          verticalAlign: 'top',
                          width: '50%',
                        }}
                      >
                        {renderFieldCell(right)}
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={2}
                      sx={{ borderBottom: 'none', py: 0.5, px: 0 }}
                    >
                      <Typography variant="caption" sx={{ opacity: 0.75 }}>
                        —
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
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
            {/* RAW */}
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

            {/* AGGREGATIONS */}
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
  data: DataTypeMeta[] | undefined,
  type: DataTypeOption['type']
) =>
  data
    ? data.map((dataType) => {
        const props = dataType.properties ?? []

        const isStart = (n: string | undefined | null) =>
          n?.toLowerCase() === 'start'
        const isEnd = (n: string | undefined | null) =>
          n?.toLowerCase() === 'end'

        const start = props.find((p) => isStart(p?.name))
        const end = props.find((p) => isEnd(p?.name))
        const rest = props.filter((p) => !isStart(p?.name) && !isEnd(p?.name))

        const ordered = [
          ...(start ? [start] : []),
          ...(end ? [end] : []),
          ...rest,
        ]

        return {
          name: dataType.name,
          displayName: createDisplayName(dataType.name),
          description: dataType.description ?? null,
          fields: ordered.map((f) => ({
            name: f.name,
            description: f.description ?? null,
          })),
          type,
        }
      })
    : []

const createDisplayName = (name: string) => {
  // Convert camelCase or PascalCase to words with spaces
  const withSpaces = name.replace(/([a-z])([A-Z])/g, '$1 $2')
  // Capitalize the first letter of each word
  const withoutAggregation = withSpaces.replace(/Aggregation$/i, '').trim()
  return withoutAggregation.replace(/\b\w/g, (char) => char.toUpperCase())
}
