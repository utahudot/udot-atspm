import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import { Box, Typography } from '@mui/material'
import React, { useMemo } from 'react'

const fmt = (v: unknown) =>
  Number.isFinite(v as number)
    ? (v as number).toLocaleString(undefined, { maximumFractionDigits: 2 })
    : ''

const DescriptiveStatsByHourByLocationTable = ({
  data,
  timeUnit,
}: PedatChartsContainerProps) => {
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
          border: '1px solid #ccc',
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
              <th style={th}>Signal ID</th>
              <th style={th}>Total Sum</th>
              <th style={th}>Mean</th>
              <th style={th}>Std Dev</th>
              <th style={th}>Min</th>
              <th style={th}>25%</th>
              <th style={th}>50%</th>
              <th style={th}>75%</th>
              <th style={th}>Max</th>
              <th style={th}>Missing</th>
            </tr>
          </thead>
          <tbody>
            {rows.length ? (
              rows.map((row) => (
                <tr key={row.locationId}>
                  <td style={td}>{row.locationId}</td>
                  <td style={td}>{fmt(row.count)}</td>
                  <td style={td}>{fmt(row.mean)}</td>
                  <td style={td}>{fmt(row.stdDev)}</td>
                  <td style={td}>{fmt(row.min)}</td>
                  <td style={td}>{fmt(row.q1)}</td>
                  <td style={td}>{fmt(row.median)}</td>
                  <td style={td}>{fmt(row.q3)}</td>
                  <td style={td}>{fmt(row.max)}</td>
                  <td style={td}>{fmt(row.missing)}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td style={td} colSpan={10}>
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
