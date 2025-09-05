import { useGetEventLogDataTypes } from '@/api/data/aTSPMLogDataApi'
import OptionsWrapper from '@/components/OptionsWrapper'
import {
  Box,
  Divider,
  List,
  ListItemButton,
  ListItemText,
  Typography,
} from '@mui/material'

export interface DataTypeOption {
  name: string
  type: 'raw' | 'aggregation'
}

interface SelectDataTypeProps {
  selectedDataType: DataTypeOption | null
  setSelectedDataType: (dataType: DataTypeOption | null) => void
}

export const SelectDataType = ({
  selectedDataType,
  setSelectedDataType,
}: SelectDataTypeProps) => {
  // const { data: aggData, isLoading: aggLoading } = useGetAggDataTypes()
  const { data: rawData, isLoading: rawLoading } = useGetEventLogDataTypes()

  // build a flat list of options, raw first, then aggregations
  const dataTypes: DataTypeOption[] = []
  if (rawData) {
    dataTypes.push(...rawData.map((name) => ({ name, type: 'raw' })))
  }
  // if (aggData) {
  //   dataTypes.push(...aggData.map((name) => ({ name, type: 'aggregation' })))
  // }

  const formatPascalCase = (option: string): string => {
    if (option === 'ApproachPcdAggregation') {
      return 'Approach PCD'
    }
    return option
      .replace(/Aggregation$/, '')
      .replace(/([a-z])([A-Z])/g, '$1 $2')
  }

  const handleSelect = (opt: DataTypeOption) => {
    setSelectedDataType(opt)
  }

  const maxHeight = '600px'
  return (
    <Box display="flex" flexDirection="column">
      <OptionsWrapper header="Data Types" noPadding>
        <Box sx={{ maxHeight, overflow: 'auto' }}>
          <List sx={{ marginTop: '-8px' }}>
            {/* <Divider
              key="aggregations-divider"
              sx={{
                bgcolor: 'white',
                px: '2rem',
                pb: '.3rem',
                position: 'sticky',
                top: 0,
                zIndex: 1,
              }}
            >
              <Typography variant="caption">Raw</Typography>
            </Divider> */}
            {dataTypes.map((opt, idx) => {
              const nodes = []

              // insert divider before the first aggregation
              if (
                opt.type === 'aggregation' &&
                (idx === 0 || dataTypes[idx - 1].type !== 'aggregation')
              ) {
                nodes.push(
                  <Divider
                    key="aggregations-divider"
                    sx={{
                      bgcolor: 'white',
                      px: '2rem',
                      pb: '.3rem',
                      position: 'sticky',
                      top: 0,
                      zIndex: 1,
                    }}
                  >
                    <Typography variant="caption">Aggregations</Typography>
                  </Divider>
                )
              }

              nodes.push(
                <ListItemButton
                  key={`${opt.type}:${opt.name}`}
                  onClick={() => handleSelect(opt)}
                  selected={selectedDataType?.name === opt.name}
                >
                  <ListItemText primary={formatPascalCase(opt.name)} />
                </ListItemButton>
              )

              return nodes
            })}
          </List>
        </Box>
      </OptionsWrapper>
    </Box>
  )
}

export default SelectDataType
