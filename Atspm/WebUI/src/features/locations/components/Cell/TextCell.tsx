import { useCellNavigation } from '@/features/locations/components/Cell/CellNavigation'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import LockOutlined from '@mui/icons-material/LockOutlined'
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
  colCount: number | null
  value: string | number | null | undefined
  onUpdate: (v: string) => void
  error?: string | { error?: string; [key: string]: any }
  warning?: string | { error?: string; [key: string]: any }
  disabled?: boolean
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
  disabled = false,
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

  const enableNav = !disabled && !disabled
  const isFocused = enableNav && tabIndex === 0 && !isEditing

  const openEditorIfNotDisabled = useCallback(() => {
    if (!disabled) openEditor()
  }, [disabled, openEditor])

  // derive error/warning text
  const errorText =
    error == null
      ? undefined
      : typeof error === 'string'
        ? error
        : (error.error ?? JSON.stringify(error))
  const warningText =
    warning == null
      ? undefined
      : typeof warning === 'string'
        ? warning
        : (warning.error ?? JSON.stringify(warning))

  const isError = Boolean(errorText)

  const isWarning = Boolean(warningText) && !isError
  const hasIssue = isError || isWarning

  // show issue whenever not editing and hasIssue, and either not focused or hovered
  const showIssue = !isEditing && hasIssue

  // pick outline color
  const outlineColor =
    isEditing || isFocused
      ? theme.palette.primary.main
      : isError
        ? theme.palette.error.main
        : isWarning
          ? theme.palette.warning.main
          : theme.palette.primary.main

  const bgColor = isEditing
    ? alpha(theme.palette.primary.main, 0.15)
    : showIssue
      ? alpha(outlineColor, 0.15)
      : undefined

  useEffect(() => {
    if (!disabled && isFocused) {
      cellRef.current?.focus()
    }
  }, [disabled, isFocused])

  useEffect(() => {
    if (isEditing && inputRef.current) {
      const inp = inputRef.current
      inp.focus()
      inp.setSelectionRange(inp.value.length, inp.value.length)
    }
  }, [isEditing])

  const handleCellKeyDown = (e: KeyboardEvent<HTMLElement>) => {
    if (disabled) return
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
    },
    [closeEditor]
  )

  const navProps = enableNav
    ? {
        tabIndex,
        onFocusCapture: onFocus,
        onKeyDown: handleCellKeyDown,
        'aria-rowindex': row + 1,
        'aria-colindex': col + 1,
        'aria-selected': isFocused,
        'data-row': row,
        'data-col': col,
      }
    : {}

  return (
    <TableCell
      {...navProps}
      aria-disabled={disabled}
      ref={cellRef}
      role="gridcell"
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
        bgcolor: disabled ? 'grey.300' : bgColor,
      }}
    >
      <Tooltip
        title={disabled ? 'Pedestrian phase is locked to protected phase' : ''}
      >
        <Box>
          {!disabled && (isEditing || isFocused || showIssue) && (
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
                disabled={disabled}
                inputRef={inputRef}
                disableUnderline
                fullWidth
                value={value ?? ''}
                onChange={(e) => onUpdate(e.target.value)}
                onKeyDown={handleInputKeyDown}
                onBlur={handleInputBlur}
                error={isError}
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
                  isError ? (
                    <InputAdornment position="end">
                      <Tooltip title={errorText}>
                        <ErrorOutlineIcon
                          role="img"
                          color="error"
                          aria-label={errorText}
                        />
                      </Tooltip>
                    </InputAdornment>
                  ) : isWarning ? (
                    <InputAdornment position="end">
                      <Tooltip title={warningText}>
                        <WarningAmberOutlinedIcon
                          role="img"
                          color="warning"
                          aria-label={warningText}
                        />
                      </Tooltip>
                    </InputAdornment>
                  ) : (
                    <InputAdornment position="end" sx={{ width: 24 }} />
                  )
                }
              />
            ) : (
              <>
                <Typography
                  onDoubleClick={openEditorIfNotDisabled}
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
                {!disabled && showIssue && (
                  <Box
                    sx={{
                      position: 'absolute',
                      top: '50%',
                      right: 4,
                      transform: 'translateY(-50%)',
                    }}
                  >
                    {isError ? (
                      <Tooltip title={errorText}>
                        <ErrorOutlineIcon
                          role="img"
                          color="error"
                          aria-label={errorText}
                        />
                      </Tooltip>
                    ) : (
                      <Tooltip title={warningText}>
                        <WarningAmberOutlinedIcon
                          role="img"
                          color="warning"
                          aria-label={warningText}
                        />
                      </Tooltip>
                    )}
                  </Box>
                )}
              </>
            )}
          </Box>
          {disabled && (
            <LockOutlined
              fontSize="small"
              sx={{
                position: 'absolute',
                top: '50%',
                right: 4,
                transform: 'translateY(-50%)',
              }}
            />
          )}
        </Box>
      </Tooltip>
    </TableCell>
  )
}
