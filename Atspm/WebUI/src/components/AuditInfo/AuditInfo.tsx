import { toDateStamp } from '@/utils/dateTime'
import HistoryIcon from '@mui/icons-material/History'
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import { Box, Chip, Tooltip, Typography } from '@mui/material'

type DS = Date | string | null | undefined

const fmt = (d: DS) => {
  if (!d) return 'Unknown'
  const date = d instanceof Date ? d : new Date(d)
  return isNaN(date.getTime()) ? '' : toDateStamp(date)
}

export default function AuditBadge({
  obj,
  dense = false,
}: {
  obj?: {
    created?: DS
    createdBy?: string | null
    modified?: DS
    modifiedBy?: string | null
  }
  dense?: boolean
}) {
  const hasCreated = !!fmt(obj?.created) || !!obj?.createdBy
  const hasModified = !!fmt(obj?.modified) || !!obj?.modifiedBy
  const nothing = !hasCreated && !hasModified

  const label = nothing
    ? 'No history available'
    : hasModified
      ? `Updated ${fmt(obj?.modified) || ''}${obj?.modifiedBy ? ` • ${obj.modifiedBy}` : ''}`.trim()
      : `Created ${fmt(obj?.created) || ''}${obj?.createdBy ? ` • ${obj.createdBy}` : ''}`.trim()

  const tooltip = nothing ? (
    <Box sx={{ p: 0.5 }}>
      <Typography variant="caption" display="block">
        No information is available for this item.
      </Typography>
    </Box>
  ) : (
    <Box sx={{ p: 0.5 }}>
      <Typography variant="caption" display="block">
        Created: {fmt(obj?.created)}
        {obj?.createdBy ? ` • ${obj.createdBy}` : ''}
      </Typography>
      <Typography variant="caption" display="block">
        Last Modified: {fmt(obj?.modified)}
        {obj?.modifiedBy ? ` • ${obj?.modifiedBy}` : ''}
      </Typography>
    </Box>
  )

  return (
    <Tooltip arrow title={tooltip} disableInteractive>
      <Chip
        size={dense ? 'small' : 'medium'}
        variant="outlined"
        sx={{ backgroundColor: '#dae5f0' }}
        icon={
          nothing ? (
            <InfoOutlinedIcon fontSize="small" />
          ) : (
            <HistoryIcon fontSize="small" />
          )
        }
        label={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
            <Typography variant="caption">{label}</Typography>
          </Box>
        }
        aria-label={nothing ? 'No history available' : `Audit info: ${label}`}
      />
    </Tooltip>
  )
}
