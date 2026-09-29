export const compactReportTableContainerSx = {
  borderRadius: 2,
  borderColor: 'grey.200',
  overflowX: 'auto',
} as const

export const compactReportTableHeadSx = {
  '& .MuiTableCell-head': {
    fontSize: '0.8rem',
    bgcolor: 'grey.100',
    lineHeight: '1rem',
    padding: '0.5rem',
    borderBottom: '1px solid',
    borderColor: 'divider',
    fontWeight: 600,
    whiteSpace: 'nowrap',
  },
} as const

export const groupedReportTableRowSx = {
  '& .MuiTableCell-body': {
    fontSize: '0.9rem',
    borderRight: '1px solid #e0e0e0',
    padding: '.7rem',
    verticalAlign: 'middle',
  },
  '& .MuiTableCell-body:last-of-type': {
    borderRight: 0,
  },
} as const

export const compactReportTableRowSx = {
  ...groupedReportTableRowSx,
  '&:nth-of-type(odd)': { backgroundColor: '#f4f4f4' },
} as const

export const groupedReportTableRowBackgrounds = ['#ffffff', '#f4f4f4'] as const

export const groupedReportTableGroupStartSx = {
  '& .MuiTableCell-body': {
    borderTop: '2px solid',
    borderTopColor: 'grey.400',
  },
} as const

export const numericReportTableCellSx = {
  fontVariantNumeric: 'tabular-nums',
  whiteSpace: 'nowrap',
} as const
