import { useCellNavigation } from '@/features/locations/components/Cell/CellNavigation'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import WarningAmberOutlinedIcon from '@mui/icons-material/WarningAmberOutlined'
import {
  alpha,
  Box,
  Input,
  InputAdornment,
  TableCell,
  Tooltip,
  Typography,
  useTheme,
} from '@mui/material'
import {
  FocusEvent,
  KeyboardEvent,
  useCallback,
  useEffect,
  useRef,
} from 'react'

interface TextCellProps {
  approachId: number
  row: number
  col: number
  rowCount: number
  colCount: number
  value: string | number | null | undefined
  onUpdate: (v: string) => void
  error?: string
  warning?: string
}

export const TextCell = ({
  approachId,
  row,
  col,
  rowCount,
  colCount,
  value,
  onUpdate,
  error,
  warning,
}: TextCellProps) => {
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
  const inputRef = useRef<HTMLInputElement>(null)
  const isFocused = tabIndex === 0 && !isEditing

  useEffect(() => {
    if (isFocused) cellRef.current?.focus()
  }, [isFocused])

  useEffect(() => {
    if (isEditing && inputRef.current) {
      const inp = inputRef.current
      inp.focus()
      const len = inp.value.length
      inp.setSelectionRange(len, len)
    }
  }, [isEditing])

  const handleCellKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (!isEditing && e.key === 'Backspace') {
      e.preventDefault()
      openEditor()
      onUpdate('')
      return
    }
    if (
      !isEditing &&
      e.key.length === 1 &&
      !e.ctrlKey &&
      !e.metaKey &&
      !e.key.startsWith('Arrow')
    ) {
      e.preventDefault()
      openEditor()
      onUpdate(e.key)
      return
    }
    navKeyDown(e)
  }

  const handleInputKeyDown = useCallback(
    (e: KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter') {
        e.preventDefault()
        closeEditor()
        setTimeout(() => cellRef.current?.focus())
      } else if (e.key.startsWith('Arrow')) {
        e.preventDefault()
        closeEditor()
        navKeyDown(e)
      }
    },
    [closeEditor, navKeyDown]
  )

  const handleInputBlur = useCallback(
    (e: FocusEvent<HTMLInputElement>) => {
      const to = e.relatedTarget as HTMLElement | null
      if (to?.hasAttribute('data-row') && to.hasAttribute('data-col')) return
      closeEditor()
      setTimeout(() => cellRef.current?.focus())
    },
    [closeEditor]
  )

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
            {isEditing ? (
              <Input
                inputRef={inputRef}
                disableUnderline
                fullWidth
                value={value}
                onChange={(e) => onUpdate(e.target.value)}
                onKeyDown={handleInputKeyDown}
                onBlur={handleInputBlur}
                error={!!error}
                sx={{
                  height: '100%',
                  py: 0,
                  px: 1,
                  boxSizing: 'border-box',
                  '& .MuiInput-input': {
                    height: '100%',
                    padding: 0,
                    lineHeight: '44px',
                    outline: 'none',
                  },
                }}
                endAdornment={
                  error ? (
                    <InputAdornment position="end">
                      <ErrorOutlineIcon role="img" aria-label={error} />
                    </InputAdornment>
                  ) : warning ? (
                    <InputAdornment position="end">
                      <WarningAmberOutlinedIcon
                        role="img"
                        aria-label={warning}
                      />
                    </InputAdornment>
                  ) : (
                    <InputAdornment position="end" sx={{ width: 24 }} />
                  )
                }
              />
            ) : (
              <Typography
                onDoubleClick={openEditor}
                noWrap
                sx={{
                  height: '100%',
                  lineHeight: '44px',
                  px: 1,
                  cursor: 'text',
                }}
              >
                {value}
              </Typography>
            )}
          </Box>
        </>
      </Tooltip>
    </TableCell>
  )
}
