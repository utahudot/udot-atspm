import { PedatLocationData } from '@/api/reports'
import BoxPlotByLocationChart from '@/features/activeTransportation/components/charts/BoxPlotByLocationChart'
import { Box, Button, Tab, Tabs } from '@mui/material'
import { useMemo, useRef, useState } from 'react'
import AverageDailyPedVolByLocationChart from './charts/AverageDailyPedVolByLocationChart'
import DailyPedVolByMonthChart from './charts/DailyPedVolByMonthChart'
import HourlyPedVolByDayOfWeekChart from './charts/HourlyPedVolByDayOfWeekChart'
import HourlyPedVolByHourOfDayChart from './charts/HourlyPedVolByHourOfDayChart'
import TimeSeriesByHourByLocationChart from './charts/TimeSeriesByHourByLocationChart'
import TotalPedVolByLocationCharts from './charts/TotalPedVolByLocationCharts'
import DescriptiveStatsByHourByLocationTable from './DescriptiveStatsByHourByLocationTable'
import PedatMapContainer from './PedatMapContainer'
import PedestrianVolumeTimeSeriesTable from './PedestrianVolumeTimeSeriesTable'

export interface PedatChartsContainerProps {
  data?: PedatLocationData[]
  phase?: string
  timeUnit?: string
  printMode?: boolean
}

function escapeCsv(val: unknown): string {
  if (val === null || val === undefined) return ''
  const s = String(val)
  return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s
}

function downloadCsv(filename: string, csv: string) {
  const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.style.display = 'none'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

function buildRawCsv(
  phase: string | undefined,
  data?: PedatLocationData[]
): string {
  const header =
    phase === 'All'
      ? [
          'Signal ID',
          'Address',
          'Timestamp',
          'Count',
          'City',
          'Latitude',
          'Longitude',
        ]
      : [
          'Signal ID',
          'Phase',
          'Address',
          'Timestamp',
          'Count',
          'City',
          'Latitude',
          'Longitude',
        ]
  const rows: string[] = [header.join(',')]
  if (!data?.length) return rows.join('\r\n')

  for (const loc of data) {
    const signalId = loc.locationIdentifier ?? ''
    const address = loc.names ?? ''
    const city = (loc.areas ?? '').split(',')[0]?.trim() || loc.areas || ''
    const lat = Number.isFinite(loc.latitude)
      ? (loc.latitude as number).toFixed(6)
      : ''
    const lng = Number.isFinite(loc.longitude)
      ? (loc.longitude as number).toFixed(6)
      : ''
    for (const pt of loc.rawData ?? []) {
      const ts = (pt as any).timestamp ?? (pt as any).timeStamp
      if (!ts) continue
      const timestamp = new Date(ts).toISOString()
      const count = (pt as any).pedestrianCount ?? ''
      if (phase && phase == 'All') {
        rows.push(
          [
            escapeCsv(signalId),
            escapeCsv(address),
            escapeCsv(timestamp),
            escapeCsv(count),
            escapeCsv(city),
            escapeCsv(lat),
            escapeCsv(lng),
          ].join(',')
        )
      } else {
        rows.push(
          [
            escapeCsv(signalId),
            escapeCsv(phase ?? ''),
            escapeCsv(address),
            escapeCsv(timestamp),
            escapeCsv(count),
            escapeCsv(city),
            escapeCsv(lat),
            escapeCsv(lng),
          ].join(',')
        )
      }
    }
  }
  return rows.join('\r\n')
}

function percentile(values: number[], p: number) {
  if (!values.length) return 0
  const a = [...values].sort((x, y) => x - y)
  const idx = (a.length - 1) * p
  const lo = Math.floor(idx)
  const hi = Math.ceil(idx)
  if (lo === hi) return a[lo]
  const t = idx - lo
  return a[lo] * (1 - t) + a[hi] * t
}

function buildStatsCsv(data?: PedatLocationData[]): string {
  const header = [
    'Signal ID',
    'count',
    'mean',
    'std',
    'min',
    '25%',
    '50%',
    '75%',
    'max',
    'Missing Count',
  ]
  const rows: string[] = [header.join(',')]
  if (!data?.length) return rows.join('\r\n')

  for (const loc of data) {
    const id = loc.locationIdentifier ?? ''
    const s: any = (loc as any).statisticData
    let count: number,
      mean: number,
      std: number,
      min: number,
      q1: number,
      med: number,
      q3: number,
      max: number,
      missing = 0

    if (s) {
      count = Number(s.count ?? 0)
      mean = Number(s.mean ?? 0)
      std = Number(s.std ?? 0)
      min = Number(s.min ?? 0)
      q1 = Number(s.twentyFifthPercentile)
      med = Number(s.fiftiethPercentile)
      q3 = Number(s.seventyFifthPercentile)
      max = Number(s.max ?? 0)
      missing = Number(s.missingCount ?? 0)
    } else {
      const vals =
        (loc.rawData ?? [])
          .map((r: any) => Number(r.pedestrianCount))
          .filter((v) => Number.isFinite(v)) || []
      const n = vals.length
      const sum = vals.reduce((a, b) => a + b, 0)
      const mu = n ? sum / n : 0
      const variance = n ? vals.reduce((a, b) => a + (b - mu) ** 2, 0) / n : 0
      count = sum
      mean = mu
      std = Math.sqrt(variance)
      min = n ? Math.min(...vals) : 0
      max = n ? Math.max(...vals) : 0
      q1 = percentile(vals, 0.25)
      med = percentile(vals, 0.5)
      q3 = percentile(vals, 0.75)
    }

    rows.push(
      [escapeCsv(id), count, mean, std, min, q1, med, q3, max, missing].join(
        ','
      )
    )
  }
  return rows.join('\r\n')
}

/* -------------------------------- PDF helpers -------------------------------- */

const COVER_TITLE = 'Pedestrian Volume in Utah'

function findDateBounds(data?: PedatLocationData[]) {
  let min = Number.POSITIVE_INFINITY
  let max = Number.NEGATIVE_INFINITY
  for (const d of data ?? []) {
    for (const r of d.rawData ?? []) {
      const t = Date.parse((r as any).timestamp ?? (r as any).timeStamp ?? '')
      if (Number.isFinite(t)) {
        if (t < min) min = t
        if (t > max) max = t
      }
    }
  }
  if (!Number.isFinite(min) || !Number.isFinite(max)) {
    const today = new Date()
    const iso = today.toISOString().slice(0, 10)
    return { start: iso, end: iso }
  }
  return {
    start: new Date(min).toISOString().slice(0, 10),
    end: new Date(max).toISOString().slice(0, 10),
  }
}

function fmtList(items: string[]) {
  return items.join('\n')
}

const PedatChartsContainer = ({
  data,
  phase,
  timeUnit,
}: PedatChartsContainerProps) => {
  const [tabIndex, setTabIndex] = useState(0)
  const [isStaging, setIsStaging] = useState(false)
  const stagingRef = useRef<HTMLDivElement>(null)

  // cover-page details
  const selectedLocations = useMemo(
    () =>
      (data ?? []).map(
        (d) => `${d.locationIdentifier ?? ''} -- ${d.names ?? ''}`
      ),
    [data]
  )
  const { start, end } = useMemo(() => findDateBounds(data), [data])

  const handleDownloadRaw = () => {
    const csv = buildRawCsv(phase, data)
    downloadCsv(
      `pedestrian_time_series_${new Date().toISOString().slice(0, 10)}.csv`,
      csv
    )
  }
  const handleDownloadStats = () => {
    const csv = buildStatsCsv(data)
    downloadCsv(
      `pedestrian_statistics_${new Date().toISOString().slice(0, 10)}.csv`,
      csv
    )
  }

  const handleGenerateReport = async () => {
    setIsStaging(true)
    await new Promise((r) => setTimeout(r, 400))

    const html2canvas = (await import('html2canvas')).default
    const { jsPDF } = await import('jspdf')

    const pdf = new jsPDF({
      orientation: 'landscape',
      unit: 'pt',
      format: 'letter',
    })
    const pageW = pdf.internal.pageSize.getWidth()
    const pageH = pdf.internal.pageSize.getHeight()
    const margin = 48

    pdf.setFont('helvetica', 'bold')
    pdf.setFontSize(22)
    pdf.text(COVER_TITLE, pageW / 2, 80, { align: 'center' })

    pdf.setFont('helvetica', 'normal')
    pdf.setFontSize(12)
    const intro =
      'This report provides data and visualizations of pedestrian volume at various locations in Utah. Pedestrian volume is an estimate of pedestrian crossing volume at an intersection, currently based on pedestrian push-button presses at traffic signals.'
    const split = pdf.splitTextToSize(intro, pageW - margin * 2)
    pdf.text(split, margin, 120)

    pdf.setFont('helvetica', 'bold')
    pdf.text('Selected location(s):', margin, 180)
    pdf.setFont('helvetica', 'normal')
    pdf.text(fmtList(selectedLocations), margin, 200)

    pdf.setFont('helvetica', 'bold')
    pdf.text('Selected parameters:', margin, 260)
    pdf.setFont('helvetica', 'normal')
    const params = [
      `Start date: ${start}`,
      `End date: ${end}`,
      'Location unit: All',
      'Time unit: Hour',
    ]
    pdf.text(params, margin, 280)

    pdf.setFontSize(8)
    pdf.text(
      `Report generated on ${new Date().toISOString().replace('T', ' ').slice(0, 19)}`,
      pageW / 2,
      pageH - 20,
      { align: 'center' }
    )

    const charts: { el: HTMLElement; title: string }[] = []
    stagingRef.current
      ?.querySelectorAll<HTMLElement>('[data-report-chart="true"]')
      .forEach((el) => {
        charts.push({ el, title: el.getAttribute('data-title') || '' })
      })

    for (const [idx, { el, title }] of charts.entries()) {
      if (idx === 0) {
        pdf.addPage('letter', 'landscape')
      } else {
        pdf.addPage('letter', 'landscape')
      }
      const lW = pdf.internal.pageSize.getWidth()
      const lH = pdf.internal.pageSize.getHeight()

      pdf.setFont('helvetica', 'bold')
      pdf.setFontSize(16)
      pdf.text(title || 'Chart', lW / 2, margin, { align: 'center' })

      const canvas = await html2canvas(el, {
        backgroundColor: '#ffffff',
        scale: 2,
        useCORS: true,
        logging: false,
      })
      const img = canvas.toDataURL('image/png')
      const maxW = lW - margin * 2
      const maxH = lH - margin * 2 - 16
      const ratio = Math.min(maxW / canvas.width, maxH / canvas.height)
      const w = canvas.width * ratio
      const h = canvas.height * ratio
      const x = (lW - w) / 2
      const y = (lH - h) / 2 + 8
      pdf.addImage(img, 'PNG', x, y, w, h, undefined, 'FAST')
    }

    // Remove initial (blank) portrait page if we added landscape pages after
    // (jsPDF starts with one page; we used it as cover, so keep it)

    pdf.save(`pedestrian_report_${new Date().toISOString().slice(0, 10)}.pdf`)

    setIsStaging(false)
  }

  const timeUnitToString = (unit: number) => {
    switch (unit) {
      case 0:
        return 'Hour'
      case 1:
        return 'Day'
      case 2:
        return 'Week'
      case 3:
        return 'Month'
      case 4:
        return 'Year'
      default:
        return 'Hour'
    }
  }

  const timeUnitString = timeUnitToString(timeUnit)

  return (
    <Box>
      <Tabs value={tabIndex} onChange={(_, val) => setTabIndex(val)}>
        <Tab label="Averages" />
        <Tab label="Figures" />
        <Tab label="Map" />
        <Tab label="Data" />
      </Tabs>

      {tabIndex === 0 && (
        <Box sx={{ mb: 2 }}>
          <Box sx={{ mb: 3 }}>
            <AverageDailyPedVolByLocationChart data={data} />
          </Box>
          <Box sx={{ mb: 3 }}>
            <HourlyPedVolByHourOfDayChart data={data} />
          </Box>
          <Box sx={{ mb: 3 }}>
            <HourlyPedVolByDayOfWeekChart data={data} />
          </Box>
          <DailyPedVolByMonthChart data={data} />
        </Box>
      )}

      {tabIndex === 1 && (
        <Box sx={{ mb: 4 }}>
          <Box sx={{ mb: 3 }}>
            <TotalPedVolByLocationCharts data={data} />
          </Box>
          <Box sx={{ mb: 3 }}>
            <TimeSeriesByHourByLocationChart
              printMode
              data={data}
              timeUnit={timeUnitString}
            />
          </Box>
          <Box sx={{ mb: 3 }}>
            <BoxPlotByLocationChart
              printMode
              data={data}
              timeUnit={timeUnitString}
            />
          </Box>
        </Box>
      )}

      {tabIndex === 2 && (
        <Box sx={{ mb: 4 }}>
          <PedatMapContainer data={data} />
        </Box>
      )}

      {tabIndex === 3 && (
        <Box sx={{ mb: 4 }}>
          <PedestrianVolumeTimeSeriesTable
            data={data}
            phase={phase}
            timeUnit={timeUnitString}
          />
          <DescriptiveStatsByHourByLocationTable
            data={data}
            timeUnit={timeUnitString}
          />
        </Box>
      )}

      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button variant="contained" onClick={handleDownloadRaw}>
          Download Data
        </Button>
        <Button variant="contained" onClick={handleDownloadStats}>
          Download Statistics
        </Button>
        <Button variant="contained" onClick={handleGenerateReport}>
          Generate Report
        </Button>
      </Box>

      {/* Off-screen staging area for report captures */}
      {isStaging && (
        <Box
          ref={stagingRef}
          sx={{
            position: 'fixed',
            left: -10000,
            top: 0,
            width: 1200,
            bgcolor: '#fff',
            p: 2,
            zIndex: -1,
          }}
        >
          <Box
            data-report-chart="true"
            data-title="Average daily pedestrian volume, by location"
            sx={{ mb: 2 }}
          >
            <AverageDailyPedVolByLocationChart printMode data={data} />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Hourly pedestrian volume, by Hour of day"
            sx={{ mb: 2 }}
          >
            <HourlyPedVolByHourOfDayChart printMode data={data} />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Hourly pedestrian volume, by day of week"
            sx={{ mb: 2 }}
          >
            <HourlyPedVolByDayOfWeekChart printMode data={data} />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Daily pedestrian volume, by month"
            sx={{ mb: 2 }}
          >
            <DailyPedVolByMonthChart printMode data={data} />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Total pedestrian volume, by location"
            sx={{ mb: 2 }}
          >
            <TotalPedVolByLocationCharts printMode data={data} />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Time series of pedestrian volume by location"
            sx={{ mb: 2 }}
          >
            <TimeSeriesByHourByLocationChart
              printMode
              data={data}
              timeUnit={timeUnitString}
            />
          </Box>
          <Box
            data-report-chart="true"
            data-title="Box plot of pedestrian volume by location"
            sx={{ mb: 2 }}
          >
            <BoxPlotByLocationChart
              printMode
              data={data}
              timeUnit={timeUnitString}
            />
          </Box>
        </Box>
      )}
    </Box>
  )
}

export default PedatChartsContainer
