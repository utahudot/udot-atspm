import { useGetAggregationDataTypes, useGetEventLogDataTypes } from '@/api/data'
import OptionsWrapper from '@/components/OptionsWrapper'
import {
  Box,
  Divider,
  List,
  ListItemButton,
  ListItemText,
  Skeleton,
  Typography,
} from '@mui/material'

export interface DataTypeOption {
  name: string
  displayName: string
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

  const rawOptions = toOptions(rawData, 'raw')
  const aggregateOptions = toOptions(aggData, 'aggregation')

  const handleSelect = (opt: DataTypeOption) => {
    setSelectedDataType(opt)
  }

  return (
    <Box display="flex" flexDirection="column">
      <OptionsWrapper header="Data Types" noPadding>
        <Box sx={{ maxHeight: '350px', overflow: 'auto' }}>
          <List sx={{ marginTop: '-8px' }}>
            <SectionDivider label="Raw" />
            {rawLoading && <Skeleton height={200} />}
            {rawOptions.map((opt) => (
              <ListItemButton
                key={`${opt.type}:${opt.name}`}
                onClick={() => handleSelect(opt)}
                selected={selectedDataType?.name === opt.name}
              >
                <ListItemText primary={opt.displayName} />
              </ListItemButton>
            ))}
            <SectionDivider label="Aggregations" />
            {aggLoading && <Skeleton height={200} />}
            {aggregateOptions.map((opt) => (
              <ListItemButton
                key={`${opt.type}:${opt.name}`}
                onClick={() => handleSelect(opt)}
                selected={selectedDataType?.name === opt.name}
              >
                <ListItemText primary={opt.displayName} />
              </ListItemButton>
            ))}
          </List>
        </Box>
      </OptionsWrapper>
    </Box>
  )
}

const SectionDivider = ({ label }: { label: string }) => (
  <Divider
    sx={{
      bgcolor: 'white',
      px: '2rem',
      pb: '.3rem',
      position: 'sticky',
      top: 0,
      zIndex: 1,
    }}
  >
    <Typography variant="caption">{label}</Typography>
  </Divider>
)

const toOptions = (
  data: string[] | undefined,
  type: DataTypeOption['type']
): DataTypeOption[] =>
  data
    ? Object.entries(data).map(([modelName, displayName]) => ({
        name: modelName,
        displayName: displayName || modelName,
        type,
      }))
    : []
