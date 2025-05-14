// NavigationContext.tsx
import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react'

type Focus = { approachId: number; row: number; col: number }

interface NavContext {
  focused: Focus
  setFocused: React.Dispatch<React.SetStateAction<Focus>>
}

const NavigationContext = createContext<NavContext | null>(null)

export const NavigationProvider = ({ children }: { children: ReactNode }) => {
  const [focused, setFocused] = useState<Focus>({
    approachId: -1,
    row: -1,
    col: -1,
  })

  // clear focus if you click anywhere that isn't a [data-row][data-col] cell
  useEffect(() => {
    const handler = (e: PointerEvent) => {
      const tgt = e.target as HTMLElement
      // 1) keep focus if you clicked inside a cell:
      if (tgt.closest('[data-row][data-col]')) return
      // 2) also ignore clicks inside any MUI Popover/Menu
      if (tgt.closest('.MuiPopover-root, .MuiMenu-list, .MuiMenuItem-root'))
        return

      // otherwise clear focus:
      setFocused({ approachId: -1, row: -1, col: -1 })
    }
    document.addEventListener('pointerdown', handler)
    return () => document.removeEventListener('pointerdown', handler)
  }, [])

  const value = useMemo(() => ({ focused, setFocused }), [focused])

  return (
    <NavigationContext.Provider value={value}>
      {children}
    </NavigationContext.Provider>
  )
}

export function useCellNavigation(
  approachId: number,
  row: number,
  col: number,
  rowCount: number,
  colCount: number
) {
  const ctx = useContext(NavigationContext)
  if (!ctx) throw new Error('Must be inside NavigationProvider')
  const { focused, setFocused } = ctx
  const [isEditing, setIsEditing] = useState(false)

  useEffect(() => {
    if (
      (focused.approachId !== approachId ||
        focused.row !== row ||
        focused.col !== col) &&
      isEditing
    ) {
      setIsEditing(false)
    }
  }, [focused, approachId, row, col, isEditing])

  const openEditor = useCallback(() => setIsEditing(true), [])
  const closeEditor = useCallback(() => setIsEditing(false), [])

  const tabIndex =
    focused.approachId === approachId &&
    focused.row === row &&
    focused.col === col
      ? 0
      : -1

  const onFocus = useCallback(
    () => setFocused({ approachId, row, col }),
    [approachId, row, col, setFocused]
  )

  const onKeyDown: React.KeyboardEventHandler<HTMLElement> = useCallback(
    (e) => {
      if (focused.approachId < 0) return

      // always handle Tab, even when editing
      if (e.key === 'Tab') {
        e.preventDefault()
        if (isEditing) {
          closeEditor()
        }
        let nextRow = row
        let nextCol = col

        if (e.shiftKey) {
          if (nextCol === 0) {
            nextCol = colCount - 1
            nextRow = nextRow === 0 ? rowCount - 1 : nextRow - 1
          } else {
            nextCol--
          }
        } else {
          if (nextCol === colCount - 1) {
            nextCol = 0
            nextRow = nextRow === rowCount - 1 ? 0 : nextRow + 1
          } else {
            nextCol++
          }
        }

        setFocused({ approachId, row: nextRow, col: nextCol })
        return
      }

      if (!isEditing && e.key === 'Enter') {
        e.preventDefault()
        openEditor()
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
        return
      }

      if (e.key.startsWith('Arrow')) {
        e.preventDefault()
        let nextRow = row
        let nextCol = col

        switch (e.key) {
          case 'ArrowRight':
            if (nextCol === colCount - 1) {
              nextCol = 0
              nextRow = nextRow === rowCount - 1 ? 0 : nextRow + 1
            } else {
              nextCol++
            }
            break
          case 'ArrowLeft':
            if (nextCol === 0) {
              nextCol = colCount - 1
              nextRow = nextRow === 0 ? rowCount - 1 : nextRow - 1
            } else {
              nextCol--
            }
            break
          case 'ArrowDown':
            nextRow = nextRow === rowCount - 1 ? 0 : nextRow + 1
            break
          case 'ArrowUp':
            nextRow = nextRow === 0 ? rowCount - 1 : nextRow - 1
            break
        }

        setFocused({ approachId, row: nextRow, col: nextCol })
      }
    },
    [
      focused,
      isEditing,
      closeEditor,
      openEditor,
      row,
      col,
      rowCount,
      colCount,
      approachId,
      setFocused,
    ]
  )

  return { tabIndex, onFocus, onKeyDown, isEditing, openEditor, closeEditor }
}
