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
import { useMemo, useState } from 'react'

import { laneTypeOptions } from '@/features/locations/components/editDetector/LaneTypeCell'
import { movementTypeOptions } from '@/features/locations/components/editDetector/MovementTypeCell'
import {
  getAvailableTurningMovementDirections,
  normalizeTurningMovementDirection,
} from '../directions'
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
      peakHour?: {
        peakHourFactor: number | null
        peakHourData: TableRowT[]
      } | null
    }
  }
}

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
  if (x === 'thru + thru-right') return 'Thru + Thru-Right'
  if (x === 'thru-right' || x === 'tr') return 'TR'
  if (x === 'right' || x === 'r') return 'R'
  if (x === 'na' || x === 'unknown') return 'NA'
  return raw
}

const movementTypeOrder = ['L', 'TL', 'T', 'Thru + Thru-Right', 'TR', 'R']

function sortMovementTypes(movements: string[]) {
  return [...movements].sort((a, b) => {
    const indexA = movementTypeOrder.indexOf(normalizeMovementId(a))
    const indexB = movementTypeOrder.indexOf(normalizeMovementId(b))

    if (indexA !== indexB) {
      if (indexA === -1) return 1
      if (indexB === -1) return -1
      return indexA - indexB
    }

    return a.localeCompare(b)
  })
}

function syncSelectedValues(selected: string[], available: string[]) {
  const availableSet = new Set(available)
  const nextSelected = selected.filter((value) => availableSet.has(value))

  if (nextSelected.length > 0) return nextSelected
  return available
}

export default function TurningMovementCountsTable({
  chartData,
}: TurningMovementCountsTableProps) {
  const { labels, table } = chartData.data

  const laneOptById = useMemo(() => {
    const map = new Map<string, (typeof laneTypeOptions)[number]>()
    laneTypeOptions.forEach((option) => map.set(option.id, option))
    return map
  }, [])

  const movOptById = useMemo(() => {
    const map = new Map<string, (typeof movementTypeOptions)[number]>()
    movementTypeOptions.forEach((option) => map.set(option.id, option))
    return map
  }, [])

  const [activeLaneType, setActiveLaneType] = useState<string>('Vehicle')
  const [selectedMovementTypes, setSelectedMovementTypes] = useState<string[]>(
    []
  )
  const [selectedDirections, setSelectedDirections] = useState<string[]>([])
  const [directionMode, setDirectionMode] = useState<'combine' | 'split'>(
    'split'
  )
  const [movementMode, setMovementMode] = useState<'combine' | 'split'>('split')

  const availableLaneTypes = useMemo(() => {
    const set = new Set<string>()
    table.forEach((row) => set.add(row.laneType))
    return Array.from(set).sort((a, b) => a.localeCompare(b))
  }, [table])

  const resolvedActiveLaneType = useMemo(() => {
    if (availableLaneTypes.includes(activeLaneType)) return activeLaneType
    if (availableLaneTypes.includes('Vehicle')) return 'Vehicle'
    return availableLaneTypes[0] ?? ''
  }, [activeLaneType, availableLaneTypes])

  const activeLaneRows = useMemo(
    () => table.filter((row) => row.laneType === resolvedActiveLaneType),
    [resolvedActiveLaneType, table]
  )

  const availableMovementTypes = useMemo(() => {
    const set = new Set<string>()
    activeLaneRows.forEach((row) => set.add(row.movementType))
    return sortMovementTypes(Array.from(set))
  }, [activeLaneRows])

  const availableDirections = useMemo(
    () =>
      getAvailableTurningMovementDirections(
        activeLaneRows.map((row) => row.direction)
      ),
    [activeLaneRows]
  )

  const effectiveSelectedMovementTypes = useMemo(
    () => syncSelectedValues(selectedMovementTypes, availableMovementTypes),
    [availableMovementTypes, selectedMovementTypes]
  )

  const effectiveSelectedDirections = useMemo(
    () => syncSelectedValues(selectedDirections, availableDirections),
    [availableDirections, selectedDirections]
  )

  const directionFilterOptions = useMemo<FilterOption[]>(
    () =>
      availableDirections.map((direction) => ({
        value: direction,
        label: direction,
      })),
    [availableDirections]
  )

  const laneTypeTabOptions = useMemo<FilterOption[]>(
    () =>
      [
        ...availableLaneTypes.map((laneType) => {
          const laneId = normalizeLaneTypeId(laneType)
          const laneOption = laneOptById.get(laneId)

          return {
            value: laneType,
            label: laneOption?.description ?? laneType,
          }
        }),
      ].sort((a, b) => b.label.localeCompare(a.label)),
    [availableLaneTypes, laneOptById]
  )

  const movementFilterOptions = useMemo<FilterOption[]>(
    () =>
      availableMovementTypes.map((movement) => {
        const movementId = normalizeMovementId(movement)
        const movementOption = movOptById.get(movementId)

        return {
          value: movement,
          label: movementOption?.description ?? movement,
        }
      }),
    [availableMovementTypes, movOptById]
  )

  const toggleSelection = (value: string, selected: string[]) =>
    selected.includes(value)
      ? selected.length === 1
        ? selected
        : selected.filter((x) => x !== value)
      : [...selected, value]

  const handleToggleDirection = (value: string) => {
    setSelectedDirections(toggleSelection(value, effectiveSelectedDirections))
  }

  const handleToggleMovement = (value: string) => {
    setSelectedMovementTypes(
      toggleSelection(value, effectiveSelectedMovementTypes)
    )
  }

  const movementsByDir = useMemo(() => {
    const map = new Map<string, string[]>()

    availableDirections.forEach((direction) => {
      const movements = activeLaneRows
        .filter(
          (row) =>
            normalizeTurningMovementDirection(row.direction) === direction
        )
        .map((row) => row.movementType)

      map.set(direction, sortMovementTypes(Array.from(new Set(movements))))
    })

    return map
  }, [activeLaneRows, availableDirections])

  const { headerRow1, headerRow2, rows, dirTotalIdx, binTotalIdx } =
    useMemo(() => {
      const timestamps = Array.from(
        new Set(
          activeLaneRows.flatMap((row) => row.volumes.map((v) => v.timestamp))
        )
      ).sort((a, b) => a.localeCompare(b))

      const valueMap = new Map<string, number>()
      for (const row of activeLaneRows) {
        for (const volume of row.volumes) {
          valueMap.set(
            `${normalizeTurningMovementDirection(row.direction)}|${row.movementType}|${row.laneType}|${volume.timestamp}`,
            volume.value ?? 0
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
      const enabledLaneTypes = [resolvedActiveLaneType]
      const enabledMovementTypes = new Set(effectiveSelectedMovementTypes)
      const enabledDirections = new Set(effectiveSelectedDirections)
      const activeDirections = availableDirections.filter((direction) =>
        enabledDirections.has(direction)
      )

      const getMovementsForDirections = (dirsForGroup: string[]) => {
        const seen = new Set<string>()
        const ordered: string[] = []

        dirsForGroup.forEach((dir) => {
          const movements = movementsByDir.get(dir) ?? []
          movements.forEach((movement) => {
            if (!enabledMovementTypes.has(movement) || seen.has(movement))
              return
            seen.add(movement)
            ordered.push(movement)
          })
        })

        return ordered
      }

      const directionGroups =
        directionMode === 'combine'
          ? activeDirections.length
            ? [{ label: 'Combined Directions', dirs: activeDirections }]
            : []
          : activeDirections.map((direction) => ({
              label: direction,
              dirs: [direction],
            }))

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
            ? []
            : movementList.map((movement) => ({
                label: movement,
                movs: [movement],
              }))

        const dirColSpan = movementGroups.length + 1
        headerRow1.push({
          label: directionGroup.label,
          colSpan: dirColSpan,
          divider: true,
        })

        for (const movementGroup of movementGroups) {
          const showMovementIcon = movementMode !== 'combine'
          const movementId = normalizeMovementId(movementGroup.label)
          const movementOption = movOptById.get(movementId)

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
                {showMovementIcon ? (movementOption?.icon ?? null) : null}
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
      cols.forEach((col, index) => {
        if (col.kind === 'dirTotal') dirTotalIdx.add(index)
        if (col.kind === 'binTotal') binTotalIdx = index
      })

      const bodyRows: TableRowT[] = timestamps.map((ts) => {
        const row: (string | number)[] = new Array(cols.length).fill(0)
        row[0] = formatTime(ts)

        for (let i = 1; i < cols.length; i++) {
          const col = cols[i]

          if (col.kind === 'cell' || col.kind === 'dirTotal') {
            row[i] = sumValues(col.dirs, col.movs, enabledLaneTypes, ts)
          }
        }

        if (binTotalIdx >= 0) {
          let bin = 0
          dirTotalIdx.forEach((index) => {
            const value = row[index]
            if (typeof value === 'number') bin += value
          })
          row[binTotalIdx] = bin
        }

        return row as TableRowT
      })

      const footer = new Array(cols.length).fill(0) as TableRowT
      footer[0] = 'Total'
      for (let col = 1; col < cols.length; col++) {
        let sum = 0
        for (const row of bodyRows) {
          const value = row[col]
          if (typeof value === 'number') sum += value
        }
        footer[col] = sum
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
      activeLaneRows,
      availableDirections,
      effectiveSelectedDirections,
      effectiveSelectedMovementTypes,
      directionMode,
      movementMode,
      movOptById,
      movementsByDir,
      resolvedActiveLaneType,
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
    headerRow1.forEach((header) => {
      for (let i = 0; i < header.colSpan; i++)
        row1Expanded.push(header.label ?? '')
    })

    const row2Expanded: string[] = []
    headerRow2.forEach((header) => {
      const label =
        typeof header.label === 'string'
          ? header.label
          : (header.csvLabel ?? '')
      for (let i = 0; i < header.colSpan; i++) row2Expanded.push(label)
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

    rows.forEach((row) => {
      lines.push(row.map(csvEscape).join(','))
    })

    return lines.join('\n')
  }

  const handleDownloadCsv = () => {
    const csv = buildCsv()
    const filename =
      `turning-movement-counts_${resolvedActiveLaneType}_${directionMode}-${movementMode}.csv`
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
            selectedDirections={effectiveSelectedDirections}
            selectedMovementTypes={effectiveSelectedMovementTypes}
            directionMode={directionMode}
            movementMode={movementMode}
            onToggleDirection={handleToggleDirection}
            onToggleMovement={handleToggleMovement}
            onDirectionModeChange={setDirectionMode}
            onMovementModeChange={setMovementMode}
          />

          <TurningMovementCountsTableToolbar
            activeLaneType={resolvedActiveLaneType}
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
            showPeakHour={resolvedActiveLaneType === 'Vehicle'} // only show peak hour row for vehicle tab
            labels={labels}
          />
        </AccordionDetails>
      </Accordion>
    </Box>
  )
}
