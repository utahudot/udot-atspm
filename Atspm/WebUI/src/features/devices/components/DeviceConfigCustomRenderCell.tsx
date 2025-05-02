import { CustomCellConfig } from '@/features/identity/components/users/UserRolesCell'
import { Box, Chip } from '@mui/material'

export const knownKeys = new Set([
  'id',
  'description',
  'notes',
  'protocol',
  'port',
  'path',
  'query',
  'connectionTimeout',
  'operationTimeout',
  'loggingOffset',
  'decoders',
  'userName',
  'password',
  'productId',
  'productName',
  'product',
  'created',
  'createdBy',
  'modified',
  'modifiedBy',
  'name',
])

export const DeviceConfigCustomCellRender: CustomCellConfig[] = [
  {
    headerKey: 'query',
    component: (value: any, row: any) => {
      if (Array.isArray(value) && value.length > 0) {
        return (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {value.map((item: string, idx: number) => (
              <Chip key={idx} label={item} size="small" />
            ))}
          </Box>
        )
      }
      return ''
    },
  },
  {
    headerKey: 'connectionProperties',
    component: (_: any, row: any) => {
      const extraEntries = Object.entries(row).filter(
        ([key]) => !knownKeys.has(key)
      )
      if (extraEntries.length === 0) return ''

      return (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
          {extraEntries.map(([key, val]) => (
            <Chip key={key} label={`${key}: ${val}`} size="small" />
          ))}
        </Box>
      )
    },
  },
  {
    headerKey: 'decoders',
    component: (value: any, row: any) => {
      if (Array.isArray(value) && value.length > 0) {
        return (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {value.map((decoder: string, idx: number) => {
              // Add a space before each capital letter except the first one.
              const formattedDecoder = decoder.replace(/(?!^)([A-Z])/g, ' $1')
              return <Chip key={idx} label={formattedDecoder} size="small" />
            })}
          </Box>
        )
      }
      return ''
    },
  },
]
