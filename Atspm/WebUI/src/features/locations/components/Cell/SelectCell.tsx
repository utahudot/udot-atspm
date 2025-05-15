import { useCellNavigation } from '@/features/locations/components/Cell/CellNavigation'
import {
  alpha,
  Box,
  MenuItem,
  TableCell,
  Tooltip,
  useTheme,
} from '@mui/material'
import Select from '@mui/material/Select'
import React, { KeyboardEvent, useEffect, useRef } from 'react'

interface SelectCellProps {
  approachId: number
  row: number
  col: number
  rowCount: number
  colCount: number
  value: string | number | null | undefined
  onUpdate: (v: string) => void
  options: { value: string; label: string; icon?: React.ReactElement }[]
  error?: string
  warning?: string
}

const SelectCell = ({
  approachId,
  row,
  col,
  rowCount,
  colCount,
  value,
  onUpdate,
  options,
  error,
  warning,
}: SelectCellProps) => {
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
  const isFocused = tabIndex === 0 && !isEditing

  useEffect(() => {
    if (isFocused) cellRef.current?.focus()
  }, [isFocused])

  const handleCellClick = () => {
    if (!isEditing) openEditor()
  }

  const handleCellKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (isEditing) return
    if (e.key === 'Backspace') {
      e.preventDefault()
      onUpdate('')
      openEditor()
      return
    }
    if (
      e.key.length === 1 &&
      !e.ctrlKey &&
      !e.metaKey &&
      !e.key.startsWith('Arrow')
    ) {
      e.preventDefault()
      onUpdate((value ?? '') + e.key)
      openEditor()
      return
    }
    navKeyDown(e)
  }

  const handleSelectKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (e.key === 'Tab') {
      e.preventDefault()

      // grab whatever <li role="option"> currently has focus
      const active = document.activeElement as HTMLElement | null
      if (active?.getAttribute('role') === 'option') {
        const newVal = active.getAttribute('data-value')
        if (newVal != null) {
          onUpdate(newVal)
        }
      }

      closeEditor()
      navKeyDown(e)
      return
    }

    if (e.key === 'Enter') {
      e.preventDefault()
      closeEditor()
    }
  }

  const handleClose = () => {
    closeEditor()
  }

  const outlineColor = theme.palette.primary.main
  const innerColor = alpha(outlineColor, 0.15)

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
        boxSizing: 'border-box',
        borderRight: '0.5px solid lightgrey',
        p: 0,
        position: 'relative',
        outline: 'none',
        caretColor: isEditing ? theme.palette.text.primary : 'transparent',
        '&:focus, &:focus-visible': { outline: 'none' },
        ...(isEditing && { bgcolor: innerColor }),
      }}
    >
      <Tooltip title={error ?? warning ?? ''}>
        <>
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
          <Box sx={{ position: 'relative', width: '100%', height: '100%' }}>
            <Select
              open={isEditing}
              onClose={handleClose}
              value={value ?? ''}
              onChange={(e) => {
                onUpdate(e.target.value)
                closeEditor()
              }}
              onKeyDown={handleSelectKeyDown}
              MenuProps={{
                MenuListProps: {
                  onKeyDown: handleSelectKeyDown,
                },
              }}
              variant="standard"
              disableUnderline
              sx={{
                height: '100%',
                width: '100%',
                px: 1,
                boxSizing: 'border-box',
                '& .MuiSelect-select': {
                  display: 'flex',
                  alignItems: 'center',
                  height: '100%',
                  lineHeight: '44px',
                  padding: 0,
                },
              }}
            >
              {options.map((opt) => (
                <MenuItem
                  key={opt.value}
                  value={opt.value}
                  sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
                >
                  {opt.icon}
                  {opt.label}
                </MenuItem>
              ))}
            </Select>
          </Box>
        </>
      </Tooltip>
    </TableCell>
  )
}

export default SelectCell
