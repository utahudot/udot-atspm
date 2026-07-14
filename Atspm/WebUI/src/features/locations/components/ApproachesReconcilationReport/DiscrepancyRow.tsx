// DiscrepancyRow.tsx
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { Box, Tooltip } from '@mui/material'
import { useTheme } from '@mui/material/styles'
import DiscrepancyButton from './DiscrepancyButton'

export interface DiscrepancyItem {
  id: string | number
  label: string | number
  kind?: string
}

export type ItemStatus = 'pending' | 'ignored' | 'added' | 'deleted' | 'unsaved'

export interface DiscrepancyRowProps {
  items: DiscrepancyItem[]
  itemStatuses: Record<string, ItemStatus>
  isSelected: (item: DiscrepancyItem) => boolean
  onToggleSelected: (item: DiscrepancyItem) => void
  showEmptyIndicator?: boolean
  getSortKey?: (item: DiscrepancyItem) => number | string
  buttonWidth?: number
}

const defaultSortKey = (item: DiscrepancyItem) => {
  const s = item.label.toString().trim()
  const n = Number(s)
  return Number.isFinite(n) ? n : s
}

const DiscrepancyRow = ({
  items,
  itemStatuses,
  isSelected,
  onToggleSelected,
  showEmptyIndicator = true,
  getSortKey = defaultSortKey,
  buttonWidth,
}: DiscrepancyRowProps) => {
  const theme = useTheme()

  const displayItems = items
    .filter((item) => {
      const status = itemStatuses[item.id.toString()] || 'pending'
      return status === 'pending' || status === 'unsaved'
    })
    .slice()
    .sort((a, b) => {
      const ak = getSortKey(a)
      const bk = getSortKey(b)
      if (typeof ak === 'number' && typeof bk === 'number') return ak - bk
      return ak
        .toString()
        .localeCompare(bk.toString(), undefined, { numeric: true })
    })

  if (!displayItems.length) {
    if (!showEmptyIndicator) return null
    return (
      <Tooltip title="No discrepancies found" arrow placement="top">
        <Box display="inline-flex" alignItems="center">
          <CheckCircleIcon
            fontSize="small"
            sx={{ color: theme.palette.success.main }}
          />
        </Box>
      </Tooltip>
    )
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'flex-start',
      }}
    >
      {displayItems.map((item) => (
        <DiscrepancyButton
          key={item.id.toString()}
          item={item}
          selected={isSelected(item)}
          onToggle={() => onToggleSelected(item)}
          width={buttonWidth}
        />
      ))}
    </Box>
  )
}

export default DiscrepancyRow
