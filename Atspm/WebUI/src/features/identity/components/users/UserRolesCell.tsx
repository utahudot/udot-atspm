import { Chip, Stack } from '@mui/material'

export interface CustomCellProps {
  value: any
  row: any
  headerKey: string
}

export const UserRolesCell = ({ value }: CustomCellProps) => {
  if (!Array.isArray(value)) return null

  const formatRoleName = (role: string) => {
    return role.replace(/([A-Z])/g, ' $1').trim()
  }

  return (
    <Stack direction="row" flexWrap="wrap" gap={1}>
      {value.map((role, index) => (
        <Chip key={index} label={formatRoleName(role)} size="small" />
      ))}
    </Stack>
  )
}

export type { CustomCellConfig }
