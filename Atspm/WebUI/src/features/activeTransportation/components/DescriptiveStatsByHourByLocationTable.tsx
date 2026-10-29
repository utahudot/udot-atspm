import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import { Box, Typography, useTheme } from '@mui/material'
import React, { useMemo } from 'react'

const fmt = (v: unknown) =>
  Number.isFinite(v as number)
    ? (v as number).toLocaleString(undefined, { maximumFractionDigits: 2 })
    : ''

const DescriptiveStatsByHourByLocationTable = ({
  data,
  timeUnit,
}: PedatChartsContainerProps) => {
  const theme = useTheme()
  const headerStyle: React.CSSProperties = {
    ...th,
    backgroundColor:
      theme.palette.mode === 'light' ? '#f2f2f2' : theme.palette.secondary.dark,
    borderBottom: `2px solid ${theme.palette.mode === 'light' ? '#ccc' : theme.palette.divider}`,
  }
  const cellStyle: React.CSSProperties = {
    ...td,
    borderBottom: `1px solid ${theme.palette.mode === 'light' ? '#eee' : theme.palette.divider}`,
  }
  const rows = useMemo(() => {
    return (data ?? []).map((loc) => {
      const s = loc?.statisticData ?? {}
      return {
        locationId: loc?.locationIdentifier,
        count: s.count,
        mean: s.mean,
        stdDev: s.std,
        min: s.min,
        q1: s.twentyFifthPercentile,
        median: s.fiftiethPercentile,
        q3: s.seventyFifthPercentile,
        max: s.max,
        missing: s.missingCount,
      }
    })
  }, [data])

  return (
    <Box sx={{ mb: 2 }}>
      <Box sx={{ textAlign: 'center', mb: 2 }}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }} gutterBottom>
          Descriptive Statistics By {timeUnit} by Location
        </Typography>
      </Box>

      <Box
        sx={{
          overflow: 'auto',
          maxHeight: '600px',
          border: `1px solid ${theme.palette.mode === 'light' ? '#ccc' : theme.palette.divider}`,
          borderRadius: '8px',
        }}
      >
        <table
          style={{
            width: '100%',
            borderCollapse: 'collapse',
            fontSize: '0.875rem',
          }}
        >
          <thead>
            <tr>
              <th style={headerStyle}>Signal ID</th>
              <th style={headerStyle}>Total Sum</th>
              <th style={headerStyle}>Mean</th>
              <th style={headerStyle}>Std Dev</th>
              <th style={headerStyle}>Min</th>
              <th style={headerStyle}>25%</th>
              <th style={headerStyle}>50%</th>
              <th style={headerStyle}>75%</th>
              <th style={headerStyle}>Max</th>
              <th style={headerStyle}>Missing</th>
            </tr>
          </thead>
          <tbody>
            {rows.length ? (
              rows.map((row) => (
                <tr key={row.locationId}>
                  <td style={cellStyle}>{row.locationId}</td>
                  <td style={cellStyle}>{fmt(row.count)}</td>
                  <td style={cellStyle}>{fmt(row.mean)}</td>
                  <td style={cellStyle}>{fmt(row.stdDev)}</td>
                  <td style={cellStyle}>{fmt(row.min)}</td>
                  <td style={cellStyle}>{fmt(row.q1)}</td>
                  <td style={cellStyle}>{fmt(row.median)}</td>
                  <td style={cellStyle}>{fmt(row.q3)}</td>
                  <td style={cellStyle}>{fmt(row.max)}</td>
                  <td style={cellStyle}>{fmt(row.missing)}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td style={cellStyle} colSpan={10}>
                  No data
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </Box>
    </Box>
  )
}

const th: React.CSSProperties = {
  position: 'sticky',
  top: 0,
  zIndex: 1,
  backgroundColor: '#f2f2f2',
  borderBottom: '2px solid #ccc',
  textAlign: 'left',
  padding: '8px',
  fontWeight: 600,
}

const td: React.CSSProperties = {
  padding: '8px',
  borderBottom: '1px solid #eee',
}

export default DescriptiveStatsByHourByLocationTable
