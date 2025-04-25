import {
  Box,
  Checkbox,
  MenuItem,
  Select,
  SelectChangeEvent,
  TableCell,
  Tooltip,
  alpha,
  useTheme,
} from '@mui/material'
import React, { KeyboardEvent, useEffect, useRef } from 'react'
import { useCellNavigation } from './CellNavigation'

interface MultiSelectCellProps<T> {
  approachId: number
  row: number
  col: number
  rowCount: number
  colCount: number
  value: T[] | undefined
  onUpdate: (newVals: T[]) => void
  options: { value: T; label: string }[]
  renderValue: (selected: T[]) => React.ReactNode
  error?: string
  warning?: string
}

export function MultiSelectCell<T>({
  approachId,
  row,
  col,
  rowCount,
  colCount,
  value,
  onUpdate,
  options,
  renderValue,
  error,
  warning,
}: MultiSelectCellProps<T>) {
  const theme = useTheme()
  const {
    tabIndex,
    onFocus,
    onKeyDown: navKeyDown,
    isEditing,
    openEditor,
    closeEditor,
  } = useCellNavigation(approachId, row, col, rowCount, colCount)

  const cellRef = useRef<HTMLElement>(null)

  useEffect(() => {
    if (tabIndex === 0 && !isEditing) cellRef.current?.focus()
  }, [tabIndex, isEditing])

  const handleCellClick = () => {
    if (!isEditing) openEditor()
  }
  const handleCellKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
    if (isEditing && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
      e.preventDefault()
      return
    }
    if (!isEditing && e.key === 'Enter') {
      e.preventDefault()
      openEditor()
      return
    }
    if (!isEditing && e.key.startsWith('Arrow')) {
      e.preventDefault()
      navKeyDown(e)
    }
  }
  const handleSelectKeyDown = (
    e: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    if (e.key === 'Escape') {
      e.preventDefault()
      closeEditor()
      setTimeout(() => cellRef.current?.focus())
    }
  }
  const handleChange = (e: SelectChangeEvent<T[]>) => {
    onUpdate(e.target.value as T[])
  }

  const outlineColor = theme.palette.primary.main
  const innerColor = alpha(outlineColor, 0.15)
  const isFocused = tabIndex === 0 && !isEditing

  return (
    <TableCell
      ref={cellRef}
      role="gridcell"
      aria-rowindex={row + 1}
      aria-colindex={col + 1}
      aria-selected={isFocused}
      tabIndex={tabIndex}
      onFocusCapture={onFocus}
      onKeyDown={handleCellKeyDown}
      onClick={handleCellClick}
      data-row={row}
      data-col={col}
      sx={{
        height: 48,
        width: 140,
        p: 0,
        position: 'relative',
        outline: 'none',
        caretColor: isEditing ? theme.palette.text.primary : 'transparent',
        bgcolor: isEditing ? innerColor : '',
        borderRight: '0.5px solid lightgrey',
        '&:focus-visible': { outline: 'none' },
      }}
    >
      {(isEditing || isFocused) && (
        <Box
          sx={{
            pointerEvents: 'none',
            position: 'absolute',
            inset: 0,
            border: `2px solid ${outlineColor}`,
            borderRadius: 1,
            zIndex: 1,
          }}
        />
      )}

      <Tooltip title={error ?? warning ?? ''}>
        <Box sx={{ width: '100%', height: '100%' }}>
          <Select
            multiple
            displayEmpty
            open={isEditing}
            value={value}
            onChange={handleChange}
            onKeyDown={handleSelectKeyDown}
            variant="standard"
            disableUnderline
            renderValue={renderValue}
            MenuProps={{
              disablePortal: true,
              onClose: () => closeEditor(),
            }}
            sx={{
              height: '100%',
              width: '100%',
              px: 1,
              boxSizing: 'border-box',
              '& .MuiSelect-select': {
                display: 'flex',
                alignItems: 'center',
                height: '100%',
                p: 0,
              },
            }}
          >
            {options.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                <Checkbox
                  checked={(value ?? []).includes(opt.value)}
                  tabIndex={-1}
                />
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        </Box>
      </Tooltip>
    </TableCell>
  )
}
