import { useCellNavigation } from '@/features/locations/components/Cell/CellNavigation'
import { Box, TableCell, alpha, useTheme } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import React, {
  FocusEvent,
  KeyboardEvent,
  useEffect,
  useRef,
  useState,
} from 'react'

interface CalendarCellProps {
  approachId: number
  row: number
  col: number
  rowCount: number
  colCount: number
  value: string
  onUpdate: (newValue: Date) => void
}

const CalendarCell: React.FC<CalendarCellProps> = ({
  approachId,
  row,
  col,
  rowCount,
  colCount,
  value,
  onUpdate,
}) => {
  const theme = useTheme()
  const {
    tabIndex,
    onFocus,
    onKeyDown: navKeyDown,
    isEditing: cellIsActive,
    openEditor: activateCell,
    closeEditor: deactivateCell,
  } = useCellNavigation(approachId, row, col, rowCount, colCount)
  const cellRef = useRef<HTMLElement>(null)
  const [pickerOpen, setPickerOpen] = useState(false)

  const date = value ? new Date(value) : null

  useEffect(() => {
    if (tabIndex === 0 && !pickerOpen) {
      cellRef.current?.focus()
    }
  }, [tabIndex, pickerOpen])

  const handleCellKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (pickerOpen) return
    if (e.key === 'Enter') {
      e.preventDefault()
      activateCell()
      setPickerOpen(true)
      return
    }
    if (e.key.startsWith('Arrow')) {
      e.preventDefault()
      navKeyDown(e)
      return
    }
    navKeyDown(e)
  }

  const handlePickerClose = () => {
    setPickerOpen(false)
    deactivateCell()
  }

  const handleDateChange = (d: Date | null) => {
    if (!d) return
    onUpdate(d)
  }

  const outlineColor = theme.palette.primary.main
  const innerColor = alpha(outlineColor, 0.15)
  const isFocused = tabIndex === 0 && !pickerOpen
  const showBorder = isFocused || pickerOpen

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
      data-row={row}
      data-col={col}
      sx={{
        minWidth: 160,
        height: 48,
        p: 0,
        position: 'relative',
        outline: 'none',
        bgcolor: cellIsActive ? innerColor : undefined,
        caretColor: 'transparent',
        borderRight: `1px solid ${theme.palette.divider}`,
      }}
    >
      {showBorder && (
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
      <DatePicker
        open={pickerOpen}
        onClose={handlePickerClose}
        value={date}
        onChange={handleDateChange}
        slotProps={{
          textField: {
            inputProps: { 'aria-label': 'date-added' },
            onFocus: (e: FocusEvent<HTMLInputElement>) => {
              e.stopPropagation()
              activateCell()
            },
            onPointerDown: (e) => {
              e.stopPropagation()
            },
          },
          openPickerButton: {
            onClick: (e) => {
              e.stopPropagation()
              activateCell()
              setPickerOpen(true)
            },
          },
        }}
        sx={{ '& fieldset': { border: 'none' } }}
      />
    </TableCell>
  )
}

export default CalendarCell
