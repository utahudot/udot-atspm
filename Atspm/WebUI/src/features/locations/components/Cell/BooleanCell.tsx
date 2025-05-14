import { useCellNavigation } from '@/features/locations/components/Cell/CellNavigation'
import CheckIcon from '@mui/icons-material/Check'
import ClearIcon from '@mui/icons-material/Clear'
import { Box, TableCell, useTheme } from '@mui/material'
import { KeyboardEvent, MouseEvent, useEffect, useRef } from 'react'

interface BooleanCellProps {
  approachId: number
  row: number
  col: number
  rowCount: number
  colCount: number
  value: boolean | null | undefined
  onUpdate: (v: boolean) => void
}

const BooleanCell = ({
  approachId,
  row,
  col,
  rowCount,
  colCount,
  value,
  onUpdate,
}: BooleanCellProps) => {
  const theme = useTheme()
  const {
    tabIndex,
    onFocus,
    onKeyDown: navKeyDown,
  } = useCellNavigation(approachId, row, col, rowCount, colCount)
  const cellRef = useRef<HTMLElement>(null)
  const isFocused = tabIndex === 0

  useEffect(() => {
    if (isFocused) cellRef.current?.focus()
  }, [isFocused])

  const handleToggle = (
    e: MouseEvent<HTMLElement> | KeyboardEvent<HTMLElement>
  ) => {
    e.preventDefault()
    onUpdate(value !== true)
  }

  const handleKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault()
      onFocus()
      handleToggle(e)
    } else {
      navKeyDown(e)
    }
  }

  const outlineColor = theme.palette.primary.main

  return (
    <TableCell
      ref={cellRef}
      role="gridcell"
      aria-rowindex={row + 1}
      aria-colindex={col + 1}
      aria-selected={isFocused}
      tabIndex={tabIndex}
      onFocusCapture={onFocus}
      onKeyDown={handleKeyDown}
      onClick={(e) => {
        onFocus()
        handleToggle(e)
      }}
      data-row={row}
      data-col={col}
      sx={{
        height: 48,
        width: 100,
        p: 0,
        position: 'relative',
        cursor: 'pointer',
        caretColor: 'transparent',
        borderRight: '1px solid lightgrey',
        textAlign: 'center',
        verticalAlign: 'middle',
        outline: 'none',
        '&:focus, &:focus-visible': { outline: 'none' },
      }}
    >
      {isFocused && (
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
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          width: '100%',
          height: '100%',
        }}
      >
        {value ? <CheckIcon /> : <ClearIcon />}
      </Box>
    </TableCell>
  )
}

export default BooleanCell
