import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import { Box, Typography, useTheme } from '@mui/material'
import React, { useMemo } from 'react'

const PedestrianVolumeTimeSeriesTable = ({
  data,
  phase,
  timeUnit,
}: PedatChartsContainerProps) => {
  const theme = useTheme()
  const headerStyle: React.CSSProperties = {
    ...thStyle,
    backgroundColor:
      theme.palette.mode === 'light' ? '#f2f2f2' : theme.palette.secondary.dark,
    borderBottom: `2px solid ${theme.palette.mode === 'light' ? '#ccc' : theme.palette.divider}`,
  }
  const cellStyle: React.CSSProperties = {
    ...tdStyle,
    borderBottom: `1px solid ${theme.palette.mode === 'light' ? '#eee' : theme.palette.divider}`,
  }
  const rows = useMemo(() => {
    return (data ?? [])
      .flatMap((loc) => {
        const {
          locationIdentifier,
          names,
          areas,
          latitude,
          longitude,
          rawData,
        } = loc ?? {}

        return (rawData ?? []).map((r) => {
          const baseRow = {
            locationIdentifier: String(locationIdentifier ?? ''),
            address: String(names ?? ''),
            timestamp: r?.timestamp,
            pedestrian: Number(r?.pedestrianCount ?? 0),
            city: String(areas ?? ''),
            latitude,
            longitude,
          }

          return phase != null && phase !== 'All'
            ? { ...baseRow, phase }
            : baseRow
        })
      })
      .filter((r) => r.timestamp)
      .sort((a, b) => {
        const ta = new Date(a.timestamp).getTime()
        const tb = new Date(b.timestamp).getTime()
        if (ta !== tb) return ta - tb
        return a.locationIdentifier.localeCompare(b.locationIdentifier)
      })
  }, [data, phase])

  return (
    <Box sx={{ mb: 8 }}>
      <Box sx={{ textAlign: 'center', mb: 2 }}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }} gutterBottom>
          Data By {timeUnit} By Location
        </Typography>
      </Box>

      <Box
        sx={{
          overflow: 'auto',
          maxHeight: 600,
          border: `1px solid ${theme.palette.mode === 'light' ? '#ccc' : theme.palette.divider}`,
          borderRadius: 2,
        }}
      >
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th style={headerStyle}>Signal ID</th>
              {phase && phase !== 'All' && <th style={headerStyle}>Phase</th>}
              <th style={headerStyle}>Address</th>
              <th style={headerStyle}>Timestamp</th>
              <th style={headerStyle}>Pedestrians</th>
              <th style={headerStyle}>City</th>
              <th style={headerStyle}>Latitude</th>
              <th style={headerStyle}>Longitude</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, idx) => (
              <tr key={`${row.locationIdentifier}-${row.timestamp}-${idx}`}>
                <td style={cellStyle}>{row.locationIdentifier}</td>
                {phase && phase !== 'All' && (
                  <td style={cellStyle}>{row.phase}</td>
                )}
                <td style={cellStyle}>{row.address}</td>
                <td style={cellStyle}>
                  {new Date(row.timestamp).toLocaleString()}
                </td>
                <td style={cellStyle}>{row.pedestrian}</td>
                <td style={cellStyle}>{row.city}</td>
                <td style={cellStyle}>
                  {Number.isFinite(row.latitude as number)
                    ? (row.latitude as number).toFixed(6)
                    : ''}
                </td>
                <td style={cellStyle}>
                  {Number.isFinite(row.longitude as number)
                    ? (row.longitude as number).toFixed(6)
                    : ''}
                </td>
              </tr>
            ))}
            {!rows.length && (
              <tr>
                <td style={cellStyle} colSpan={7}>
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

export const thStyle: React.CSSProperties = {
  position: 'sticky',
  top: 0,
  zIndex: 1,
  backgroundColor: '#f2f2f2',
  borderBottom: '2px solid #ccc',
  textAlign: 'left',
  padding: '8px',
}

export const tdStyle: React.CSSProperties = {
  borderBottom: '1px solid #eee',
  padding: '8px',
  fontSize: '14px',
}

export default PedestrianVolumeTimeSeriesTable
