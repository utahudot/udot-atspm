import { Chip, Stack } from '@mui/material'

export interface UserAssignmentValue {
  id: number
  name?: string
  description?: string
}

interface CustomCellProps {
  value: UserAssignmentValue[]
}

export const UserAssignmentCell = ({ value }: CustomCellProps) => {
  if (!Array.isArray(value) || value.length === 0) return null

  return (
    <Stack direction="row" flexWrap="wrap" gap={1}>
      {value.map((item) => (
        <Chip
          key={item.id}
          label={item.name ?? item.description ?? String(item.id)}
          size="small"
        />
      ))}
    </Stack>
  )
}
