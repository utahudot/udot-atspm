import {
  Box,
  Divider,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableCellProps,
  TableHead,
  TableRow,
  Typography,
  useTheme,
} from '@mui/material'
import React, { Fragment, ReactNode } from 'react'

interface Column {
  key: string
  label: string
}

interface VariableGroup {
  label: string
  before?: string
  after?: string
  change?: string
}

interface Hotspot {
  id?: string
  [key: string]: any
}

interface ImpactHotspotTableProps {
  hotspots: Hotspot[]
  handleRouteSelection: (id: string) => void
  setHoveredHotspot: (id: string | null) => void
  showBeforeAfter: boolean
  isLoading: boolean
}

const alwaysColumns: Column[] = [
  { key: 'startMile', label: 'Start Mile' },
  { key: 'endMile', label: 'End Mile' },
  { key: 'speedLimit', label: 'Speed Limit' },
]

const variableGroups: VariableGroup[] = [
  {
    label: 'Average Speed (mph)',
    before: 'beforeAverageSpeed',
    after: 'afterAverageSpeed',
    change: 'changeInAverageSpeed',
  },
  {
    label: '85th %ile Speed (mph)',
    before: 'beforeAverageEightyFifthSpeed',
    after: 'afterAverageEightyFifthSpeed',
    change: 'changeInEightyFifthPercentileSpeed',
  },
  {
    label: 'Variability (mph)',
    before: 'beforeVariability',
    after: 'afterVariability',
    change: 'changeInVariability',
  },
  {
    label: '% Violations',
    before: 'beforePercentViolations',
    after: 'afterPercentViolations',
    change: 'changeInPercentViolations',
  },
  {
    label: '% Ext. Violations',
    before: 'beforePercentExtremeViolations',
    after: 'afterPercentExtremeViolations',
    change: 'changeInPercentExtremeViolations',
  },
  {
    label: 'Flow',
    before: 'beforeFlow',
    after: 'afterFlow',
  },
  {
    label: 'Min Speed (mph)',
    before: 'beforeMinSpeed',
    after: 'afterMinSpeed',
  },
  {
    label: 'Max Speed (mph)',
    before: 'beforeMaxSpeed',
    after: 'afterMaxSpeed',
  },
]

const formatValue = (value: any): string => {
  if (typeof value === 'number') {
    return value.toLocaleString()
  }
  return value || 'N/A'
}

function renderText(
  text: string,
  {
    fontSize = '.70rem',
    fontWeight = 'bold',
    whiteSpace = 'nowrap',
  }: {
    fontSize?: string
    fontWeight?: string | number
    whiteSpace?: 'nowrap' | 'normal'
  } = {}
): ReactNode {
  return (
    <Typography
      fontSize={fontSize}
      fontWeight={fontWeight}
      whiteSpace={whiteSpace}
    >
      {text}
    </Typography>
  )
}

interface StyledHeaderCellProps extends Omit<TableCellProps, 'children'> {
  children: ReactNode
  noBorder?: boolean
}

const StyledHeaderCell: React.FC<StyledHeaderCellProps> = ({
  children,
  align = 'left',
  colSpan,
  noBorder = false,
  ...props
}) => (
  <TableCell
    align={align}
    colSpan={colSpan}
    sx={{
      py: 1,
      borderBottom: noBorder ? 'none' : '1px solid #d0d0d0',
    }}
    {...props}
  >
    {children}
  </TableCell>
)

interface StyledBodyCellProps extends Omit<TableCellProps, 'children'> {
  children: ReactNode
  highlight?: boolean
  isLoading: boolean
}

const StyledBodyCell: React.FC<StyledBodyCellProps> = ({
  children,
  align = 'center',
  highlight = false,
  isLoading,
  ...props
}) => {
  const theme = useTheme()
  if (isLoading) {
    return (
      <TableCell align="right">
        <Skeleton variant="text" width={50} />
      </TableCell>
    )
  }
  return (
    <TableCell
      align={align}
      sx={{
        borderRight: '1px solid #d0d0d0',
        height: 50,
        ...(highlight && { backgroundColor: theme.palette.grey[200] }),
      }}
      {...props}
    >
      <Typography noWrap fontSize=".7rem">
        {children}
      </Typography>
    </TableCell>
  )
}

const ImpactHotspotTable: React.FC<ImpactHotspotTableProps> = ({
  hotspots,
  handleRouteSelection,
  setHoveredHotspot,
  showBeforeAfter,
  isLoading,
}) => {
  if (!hotspots || hotspots.length === 0) {
    return (
      <Box>
        <Typography>No hotspots found</Typography>
      </Box>
    )
  }

  const displayedColumns: Array<Column> = [...alwaysColumns]

  const groupsToRender = variableGroups.map((group) => {
    if (showBeforeAfter) {
      const cols = [group.before, group.after].filter(Boolean) as string[]
      if (group.change) {
        cols.push(group.change)
      }
      return { ...group, columns: cols }
    } else {
      if (group.change) {
        return { ...group, columns: [group.change] }
      } else {
        return { ...group, columns: [] }
      }
    }
  })

  groupsToRender.forEach((group) => {
    group.columns.forEach((colKey) => {
      displayedColumns.push({ key: colKey, label: group.label })
    })
  })

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box>
        <Table
          stickyHeader
          sx={{ backgroundColor: 'white', minHeight: '100%' }}
        >
          <TableHead>
            {showBeforeAfter && (
              <TableRow>
                {alwaysColumns.map((col) => (
                  <StyledHeaderCell
                    key={col.key}
                    align="center"
                    colSpan={1}
                    noBorder
                  >
                    {null}
                  </StyledHeaderCell>
                ))}
                {groupsToRender.map((group, idx) => {
                  const colCount = group.columns.length
                  if (colCount === 0) return null
                  return (
                    <StyledHeaderCell
                      key={idx}
                      colSpan={colCount}
                      align="center"
                      noBorder
                    >
                      <Divider sx={{ width: '100%' }}>
                        <Typography variant="caption">{group.label}</Typography>
                      </Divider>
                    </StyledHeaderCell>
                  )
                })}
              </TableRow>
            )}

            <TableRow>
              {alwaysColumns.map((col) => (
                <StyledHeaderCell key={col.key} colSpan={1}>
                  {renderText(col.label)}
                </StyledHeaderCell>
              ))}

              {groupsToRender.map((group, idx) => {
                if (group.columns.length === 0) return null

                if (showBeforeAfter && group.columns.length > 1) {
                  const beforeCol = group.before
                  const afterCol = group.after
                  const changeCol = group.change

                  return (
                    <Fragment key={idx}>
                      {beforeCol && (
                        <StyledHeaderCell align="center" colSpan={1}>
                          {renderText('Before')}
                        </StyledHeaderCell>
                      )}
                      {afterCol && (
                        <StyledHeaderCell align="center" colSpan={1}>
                          {renderText('After')}
                        </StyledHeaderCell>
                      )}
                      {changeCol && (
                        <StyledHeaderCell align="center" colSpan={1}>
                          {renderText('Change')}
                        </StyledHeaderCell>
                      )}
                    </Fragment>
                  )
                } else {
                  return group.columns.map((colKey) => (
                    <StyledHeaderCell key={colKey} colSpan={1}>
                      {renderText(
                        showBeforeAfter ? 'Change' : `Change in ${group.label}`
                      )}
                    </StyledHeaderCell>
                  ))
                }
              })}
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading
              ? Array.from({ length: 15 }).map((_, rowIndex) => (
                  <TableRow key={rowIndex}>
                    {alwaysColumns.map((col, colIndex) => (
                      <StyledBodyCell key={colIndex} colSpan={1} isLoading>
                        <Skeleton variant="text" width={10} />
                      </StyledBodyCell>
                    ))}
                    {groupsToRender.map((group, gidx) => {
                      if (group.columns.length === 0) return null
                      return (
                        <Fragment key={gidx}>
                          {group.columns.map((_, colIndex) => (
                            <StyledBodyCell
                              key={colIndex}
                              colSpan={1}
                              isLoading
                            >
                              <Skeleton variant="text" width={15} />
                            </StyledBodyCell>
                          ))}
                        </Fragment>
                      )
                    })}
                  </TableRow>
                ))
              : hotspots.map((hotspot, index) => (
                  <TableRow
                    key={hotspot.id || index}
                    hover
                    sx={{ cursor: 'pointer', height: 50 }}
                    // onClick={() => handleRouteSelection(hotspot.id || '')}
                    onMouseEnter={() => setHoveredHotspot(hotspot.id || null)}
                    onMouseLeave={() => setHoveredHotspot(null)}
                  >
                    {alwaysColumns.map((col) => (
                      <StyledBodyCell key={col.key} colSpan={1}>
                        {formatValue(hotspot.properties[col.key])}
                      </StyledBodyCell>
                    ))}

                    {groupsToRender.map((group, gidx) => {
                      const { properties } = hotspot
                      if (group.columns.length === 0) return null
                      if (showBeforeAfter && group.columns.length > 1) {
                        const beforeCol = group.before
                        const afterCol = group.after
                        const changeCol = group.change

                        return (
                          <Fragment key={gidx}>
                            {beforeCol && (
                              <StyledBodyCell colSpan={1}>
                                {formatValue(properties[beforeCol])}
                              </StyledBodyCell>
                            )}
                            {afterCol && (
                              <StyledBodyCell colSpan={1}>
                                {formatValue(properties[afterCol])}
                              </StyledBodyCell>
                            )}
                            {changeCol && (
                              <StyledBodyCell colSpan={1} highlight>
                                {formatValue(properties[changeCol])}
                              </StyledBodyCell>
                            )}
                          </Fragment>
                        )
                      } else {
                        return group.columns.map((colKey) => (
                          <StyledBodyCell key={colKey} colSpan={1}>
                            {formatValue(properties[colKey])}
                          </StyledBodyCell>
                        ))
                      }
                    })}
                  </TableRow>
                ))}
          </TableBody>
        </Table>
      </Box>
      {/* Extra space below the table */}
      <Box sx={{ flex: '1 1 auto' }} />
    </Box>
  )
}

export default ImpactHotspotTable
