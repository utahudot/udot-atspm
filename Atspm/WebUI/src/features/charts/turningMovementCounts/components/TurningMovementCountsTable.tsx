import { Labels } from '@/features/charts/types'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Typography,
} from '@mui/material'
import { format } from 'date-fns'
import { useEffect, useMemo, useState } from 'react'

import { laneTypeOptions } from '@/features/locations/components/editDetector/LaneTypeCell'
import { movementTypeOptions } from '@/features/locations/components/editDetector/MovementTypeCell'
import TurningMovementCountsFilters, {
  type FilterOption,
} from './TurningMovementCountsFilters'
import TurningMovementCountsResultsTable from './TurningMovementCountsResultsTable'
import TurningMovementCountsTableToolbar from './TurningMovementCountsTableToolbar'

type TableRowT = (string | number)[]

type NewVolumePoint = { timestamp: string; value: number }
type NewLaneSeries = {
  direction: string
  movementType: string
  laneType: string
  volumes: NewVolumePoint[]
}

interface TurningMovementCountsTableProps {
  chartData: {
    data: {
      labels: Labels
      table: NewLaneSeries[]
      peakHourFactor: number
      peakHour?: { key: string; value: number } | null
    }
  }
}

const DIRECTION_ORDER = ['Northbound', 'Southbound', 'Eastbound', 'Westbound']

function formatTime(ts: string) {
  return format(new Date(ts), 'HH:mm')
}

function normalizeLaneTypeId(raw: string) {
  const x = raw.trim().toLowerCase()
  if (x === 'vehicle' || x === 'v') return 'V'
  if (x === 'bus') return 'Bus'
  if (x === 'bike') return 'Bike'
  if (x === 'ped' || x === 'pedestrian') return 'Ped'
  if (x === 'exit' || x === 'e') return 'E'
  if (x === 'lrt') return 'LRT'
  if (x === 'hdv') return 'HDV'
  if (x === 'na' || x === 'unknown') return 'NA'
  return raw
}

function normalizeMovementId(raw: string) {
  const x = raw.trim().toLowerCase()
  if (x === 'left' || x === 'l') return 'L'
  if (x === 'thru-left' || x === 'tl') return 'TL'
  if (x === 'thru' || x === 't') return 'T'
  if (x === 'thru-right' || x === 'tr') return 'TR'
  if (x === 'right' || x === 'r') return 'R'
  if (x === 'na' || x === 'unknown') return 'NA'
  return raw
}

function normalizeDirectionId(raw: string) {
  const x = raw.trim().toLowerCase()
  if (['northbound', 'north', 'nb', 'n'].includes(x)) return 'Northbound'
  if (['southbound', 'south', 'sb', 's'].includes(x)) return 'Southbound'
  if (['eastbound', 'east', 'eb', 'e'].includes(x)) return 'Eastbound'
  if (['westbound', 'west', 'wb', 'w'].includes(x)) return 'Westbound'
  return raw
}

export default function TurningMovementCountsTable({
  chartData,
}: TurningMovementCountsTableProps) {
  const { labels, table } = chartData.data

  const laneOptById = useMemo(() => {
    const map = new Map<string, (typeof laneTypeOptions)[number]>()
    laneTypeOptions.forEach((o) => map.set(o.id, o))
    return map
  }, [])

  const movOptById = useMemo(() => {
    const map = new Map<string, (typeof movementTypeOptions)[number]>()
    movementTypeOptions.forEach((o) => map.set(o.id, o))
    return map
  }, [])

  const availableLaneTypes = useMemo(() => {
    const set = new Set<string>()
    table.forEach((s) => set.add(s.laneType))
    return Array.from(set).sort((a, b) => a.localeCompare(b))
  }, [table])

  const availableMovementTypes = useMemo(() => {
    const set = new Set<string>()
    table.forEach((s) => set.add(s.movementType))
    return Array.from(set).sort((a, b) => a.localeCompare(b))
  }, [table])

  const directionFilterOptions = useMemo<FilterOption[]>(
    () => DIRECTION_ORDER.map((d) => ({ value: d, label: d })),
    []
  )

  const laneTypeTabOptions = useMemo<FilterOption[]>(
    () =>
      [
        ...availableLaneTypes.map((lt) => {
          const laneId = normalizeLaneTypeId(lt)
          const opt = laneOptById.get(laneId)
          return {
            value: lt,
            label: opt?.description ?? lt,
          }
        }),
      ].sort((a, b) => b.label.localeCompare(a.label)),
    [availableLaneTypes, laneOptById]
  )

  const movementFilterOptions = useMemo<FilterOption[]>(
    () => availableMovementTypes.map((m) => ({ value: m, label: m })),
    [availableMovementTypes]
  )

  const [activeLaneType, setActiveLaneType] = useState<string>('Vehicle')
  const [selectedMovementTypes, setSelectedMovementTypes] = useState<string[]>(
    []
  )
  const [selectedDirections, setSelectedDirections] =
    useState<string[]>(DIRECTION_ORDER)
  const [directionMode, setDirectionMode] = useState<'combine' | 'split'>(
    'split'
  )
  const [movementMode, setMovementMode] = useState<'combine' | 'split'>('split')
  const toggleSelection = (value: string, selected: string[]) =>
    selected.includes(value)
      ? selected.length === 1
        ? selected
        : selected.filter((x) => x !== value)
      : [...selected, value]

  const handleToggleDirection = (value: string) => {
    setSelectedDirections((prev) => toggleSelection(value, prev))
  }
  const handleToggleMovement = (value: string) => {
    setSelectedMovementTypes((prev) => toggleSelection(value, prev))
  }

  useEffect(() => {
    if (availableMovementTypes.length && selectedMovementTypes.length === 0) {
      setSelectedMovementTypes(availableMovementTypes)
    }
  }, [availableMovementTypes, selectedMovementTypes.length])

  const directions = useMemo(
    () =>
      labels.columnGroups
        .filter((g) => g.title)
        .map((g) => g.title!) as string[],
    [labels.columnGroups]
  )

  const movementsByDir = useMemo(() => {
    const map = new Map<string, string[]>()
    labels.columnGroups.forEach((g) => {
      if (!g.title) return
      map.set(
        g.title,
        g.columns.filter((c) => c !== 'Total')
      )
    })
    return map
  }, [labels.columnGroups])

  const { headerRow1, headerRow2, rows, dirTotalIdx, binTotalIdx } =
    useMemo(() => {
      const timestamps = Array.from(
        new Set(table.flatMap((d) => d.volumes.map((v) => v.timestamp)))
      ).sort((a, b) => a.localeCompare(b))

      const valueMap = new Map<string, number>()
      for (const s of table) {
        for (const v of s.volumes) {
          valueMap.set(
            `${s.direction}|${s.movementType}|${s.laneType}|${v.timestamp}`,
            v.value ?? 0
          )
        }
      }
      const getValue = (dir: string, mov: string, lane: string, ts: string) =>
        valueMap.get(`${dir}|${mov}|${lane}|${ts}`) ?? 0

      type ColKind =
        | { kind: 'hour' }
        | { kind: 'cell'; dirs: string[]; movs: string[] }
        | { kind: 'dirTotal'; dirs: string[]; movs: string[] }
        | { kind: 'binTotal' }

      const cols: ColKind[] = [{ kind: 'hour' }]

      // filters
      const enabledLaneTypes = [activeLaneType]
      const enabledMovementTypes = new Set(selectedMovementTypes)
      const enabledDirections = new Set(selectedDirections)
      const activeDirections = directions.filter((dir) =>
        enabledDirections.has(normalizeDirectionId(dir))
      )

      const getMovementsForDirections = (dirsForGroup: string[]) => {
        const seen = new Set<string>()
        const ordered: string[] = []
        dirsForGroup.forEach((dir) => {
          const movs = movementsByDir.get(dir) ?? []
          movs.forEach((mov) => {
            if (!enabledMovementTypes.has(mov) || seen.has(mov)) return
            seen.add(mov)
            ordered.push(mov)
          })
        })
        return ordered
      }

      const directionGroups =
        directionMode === 'combine'
          ? activeDirections.length
            ? [{ label: 'Combined Directions', dirs: activeDirections }]
            : []
          : activeDirections.map((dir) => ({ label: dir, dirs: [dir] }))

      const sumValues = (
        dirsForGroup: string[],
        movsForGroup: string[],
        lanesForGroup: string[],
        ts: string
      ) => {
        let sum = 0
        dirsForGroup.forEach((dir) => {
          movsForGroup.forEach((mov) => {
            lanesForGroup.forEach((lane) => {
              sum += getValue(dir, mov, lane, ts)
            })
          })
        })
        return sum
      }

      const headerRow1: Array<{
        label: string
        colSpan: number
        divider?: boolean
      }> = []
      const headerRow2: Array<{
        label: JSX.Element | string
        colSpan: number
        csvLabel?: string
      }> = []

      headerRow1.push({ label: '', colSpan: 1 })
      headerRow2.push({ label: 'Hour', colSpan: 1 })

      for (const directionGroup of directionGroups) {
        const movementList = getMovementsForDirections(directionGroup.dirs)
        const movementGroups =
          movementMode === 'combine'
            ? movementList.length
              ? [{ label: 'Combined Movements', movs: movementList }]
              : []
            : movementList.map((mov) => ({ label: mov, movs: [mov] }))

        const dirColSpan = movementGroups.length + 1
        headerRow1.push({
          label: directionGroup.label,
          colSpan: dirColSpan,
          divider: true,
        })

        for (const movementGroup of movementGroups) {
          const showMovementIcon = movementMode !== 'combine'
          const movId = normalizeMovementId(movementGroup.label)
          const movOpt = movOptById.get(movId)

          headerRow2.push({
            colSpan: 1,
            csvLabel: movementGroup.label,
            label: (
              <Box
                sx={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 0.75,
                  justifyContent: 'center',
                }}
              >
                {showMovementIcon ? (movOpt?.icon ?? null) : null}
                <span>{movementGroup.label}</span>
              </Box>
            ),
          })
          cols.push({
            kind: 'cell',
            dirs: directionGroup.dirs,
            movs: movementGroup.movs,
          })
        }

        headerRow2.push({
          colSpan: 1,
          label: 'Total',
        })
        cols.push({
          kind: 'dirTotal',
          dirs: directionGroup.dirs,
          movs: movementList,
        })
      }

      headerRow1.push({ label: '', colSpan: 1 })
      headerRow2.push({
        colSpan: 1,
        label: 'Bin Total',
      })
      cols.push({ kind: 'binTotal' })

      const dirTotalIdx = new Set<number>()
      let binTotalIdx = -1
      cols.forEach((c, i) => {
        if (c.kind === 'dirTotal') dirTotalIdx.add(i)
        if (c.kind === 'binTotal') binTotalIdx = i
      })

      const bodyRows: TableRowT[] = timestamps.map((ts) => {
        const row: (string | number)[] = new Array(cols.length).fill(0)
        row[0] = formatTime(ts)

        for (let i = 1; i < cols.length; i++) {
          const c = cols[i]

          if (c.kind === 'cell') {
            row[i] = sumValues(c.dirs, c.movs, enabledLaneTypes, ts)
            continue
          }

          if (c.kind === 'dirTotal') {
            row[i] = sumValues(c.dirs, c.movs, enabledLaneTypes, ts)
            continue
          }
        }

        if (binTotalIdx >= 0) {
          let bin = 0
          dirTotalIdx.forEach((idx) => {
            const v = row[idx]
            if (typeof v === 'number') bin += v
          })
          row[binTotalIdx] = bin
        }

        return row as TableRowT
      })

      const footer: TableRowT = new Array(cols.length).fill(0) as any
      footer[0] = 'Total'
      for (let c = 1; c < cols.length; c++) {
        let sum = 0
        for (const r of bodyRows) {
          const v = r[c]
          if (typeof v === 'number') sum += v
        }
        footer[c] = sum
      }
      bodyRows.push(footer)

      return {
        headerRow1,
        headerRow2,
        rows: bodyRows,
        dirTotalIdx,
        binTotalIdx,
      }
    }, [
      table,
      directions,
      movementsByDir,
      activeLaneType,
      selectedMovementTypes,
      selectedDirections,
      directionMode,
      movementMode,
      movOptById,
    ])

  function csvEscape(v: unknown) {
    const s = String(v ?? '')
    if (/[",\n]/.test(s)) return `"${s.replace(/"/g, '""')}"`
    return s
  }

  function downloadTextFile(
    filename: string,
    text: string,
    mime = 'text/csv;charset=utf-8'
  ) {
    const blob = new Blob([text], { type: mime })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = filename
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)
  }

  const buildCsv = () => {
    const row1Expanded: string[] = []
    headerRow1.forEach((h) => {
      for (let i = 0; i < h.colSpan; i++) row1Expanded.push(h.label ?? '')
    })

    const row2Expanded: string[] = []
    headerRow2.forEach((h) => {
      const label = typeof h.label === 'string' ? h.label : (h.csvLabel ?? '')
      for (let i = 0; i < h.colSpan; i++) row2Expanded.push(label)
    })

    const header = row2Expanded.map((h2, i) => {
      const h1 = (row1Expanded[i] ?? '').trim()
      const h2t = (h2 ?? '').trim()
      if (!h1) return h2t
      if (!h2t) return h1
      if (h1 === h2t) return h2t
      return `${h1} - ${h2t}`
    })

    const lines: string[] = []
    lines.push(header.map(csvEscape).join(','))

    rows.forEach((r) => {
      lines.push(r.map(csvEscape).join(','))
    })

    return lines.join('\n')
  }

  const handleDownloadCsv = () => {
    const csv = buildCsv()
    const filename =
      `turning-movement-counts_${activeLaneType}_${directionMode}-${movementMode}.csv`
        .replace(/\s+/g, '_')
        .toLowerCase()

    downloadTextFile(filename, csv)
  }

  return (
    <Box sx={{ mt: 4 }}>
      <Accordion disableGutters defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography variant="h4" component="h2">
            Table View
          </Typography>
        </AccordionSummary>

        <AccordionDetails>
          <TurningMovementCountsFilters
            directionOptions={directionFilterOptions}
            movementOptions={movementFilterOptions}
            selectedDirections={selectedDirections}
            selectedMovementTypes={selectedMovementTypes}
            directionMode={directionMode}
            movementMode={movementMode}
            onToggleDirection={handleToggleDirection}
            onToggleMovement={handleToggleMovement}
            onDirectionModeChange={setDirectionMode}
            onMovementModeChange={setMovementMode}
          />

          <TurningMovementCountsTableToolbar
            activeLaneType={activeLaneType}
            laneTypeTabOptions={laneTypeTabOptions}
            onLaneTypeChange={setActiveLaneType}
            onDownloadCsv={handleDownloadCsv}
          />

          <TurningMovementCountsResultsTable
            headerRow1={headerRow1}
            headerRow2={headerRow2}
            rows={rows}
            dirTotalIdx={dirTotalIdx}
            binTotalIdx={binTotalIdx}
            peakHour={chartData.data.peakHour}
            showPeakHour={activeLaneType === 'Vehicle'} // only show peak hour row for vehicle tab
            labels={labels}
          />
        </AccordionDetails>
      </Accordion>
    </Box>
  )
}
