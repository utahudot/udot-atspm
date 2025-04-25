import { styled } from '@mui/material/styles'
import { DateCalendar, PickersDay, PickersDayProps } from '@mui/x-date-pickers'
import { addDays, eachDayOfInterval, isSameDay, startOfDay } from 'date-fns'
import { useMemo, useState } from 'react'

function getDatesInRange(start: Date, end: Date) {
  const s = start < end ? start : end
  const e = start < end ? end : start
  return eachDayOfInterval({ start: startOfDay(s), end: startOfDay(e) })
}

interface ExtendedProps {
  isSelected: boolean
  isPrevSelected: boolean
  isNextSelected: boolean
  isHovered: boolean
}

const ConnectedPickersDay = styled(PickersDay, {
  shouldForwardProp: (prop) =>
    prop !== 'isSelected' &&
    prop !== 'isPrevSelected' &&
    prop !== 'isNextSelected' &&
    prop !== 'isHovered',
})<ExtendedProps>(
  ({ theme, isSelected, isPrevSelected, isNextSelected, isHovered }) => ({
    borderRadius: 0,
    ...(isSelected && {
      backgroundColor: theme.palette.primary.main,
      color: theme.palette.primary.contrastText,
      '&:hover, &:focus': {
        backgroundColor: theme.palette.primary.dark,
      },
    }),
    ...(isHovered &&
      !isSelected && {
        backgroundColor: theme.palette.action.hover,
      }),
    ...(isSelected &&
      !isPrevSelected && {
        borderTopLeftRadius: '50%',
        borderBottomLeftRadius: '50%',
      }),
    ...(isSelected &&
      !isNextSelected && {
        borderTopRightRadius: '50%',
        borderBottomRightRadius: '50%',
      }),
  })
)

interface MultiSelectDayProps extends PickersDayProps<Date> {
  selectedDays: Date[]
  tempDays: Date[]
  hoveredDay?: Date | null
}

function MultiSelectDay(props: MultiSelectDayProps) {
  const {
    day,
    selectedDays,
    tempDays,
    hoveredDay,
    outsideCurrentMonth,
    ...other
  } = props
  const allSelected = [...selectedDays, ...tempDays]
  const isSelected = allSelected.some((d) => isSameDay(d, day))
  const prevDay = addDays(day, -1)
  const nextDay = addDays(day, 1)
  const isPrevSelected = allSelected.some((d) => isSameDay(d, prevDay))
  const isNextSelected = allSelected.some((d) => isSameDay(d, nextDay))
  const isHovered = hoveredDay ? isSameDay(day, hoveredDay) : false

  return (
    <ConnectedPickersDay
      {...other}
      day={day}
      disableMargin
      selected={false}
      outsideCurrentMonth={outsideCurrentMonth}
      isSelected={isSelected}
      isHovered={isHovered}
      isPrevSelected={isSelected && isPrevSelected}
      isNextSelected={isSelected && isNextSelected}
    />
  )
}

interface MultiDaySelectProps {
  selectedDays: Date[]
  onSelectedDaysChange: (days: Date[]) => void
}

export default function MultiDaySelect({
  selectedDays,
  onSelectedDaysChange,
}: MultiDaySelectProps) {
  const [dragging, setDragging] = useState(false)
  const [dragStart, setDragStart] = useState<Date | null>(null)
  const [dragCurrent, setDragCurrent] = useState<Date | null>(null)
  const [hoveredDay, setHoveredDay] = useState<Date | null>(null)

  const tempDays = useMemo(() => {
    if (dragging && dragStart && dragCurrent) {
      return getDatesInRange(dragStart, dragCurrent)
    }
    return []
  }, [dragging, dragStart, dragCurrent])

  const handleDayPointerDown = (day: Date) => {
    setDragging(true)
    setDragStart(day)
    setDragCurrent(day)
  }

  const handleDayPointerEnter = (day: Date) => {
    if (dragging && dragStart) {
      setDragCurrent(day)
    }
  }

  const handlePointerUp = () => {
    if (dragging && dragStart && dragCurrent) {
      if (isSameDay(dragStart, dragCurrent)) {
        onSelectedDaysChange(
          selectedDays.some((d) => isSameDay(d, dragStart))
            ? selectedDays.filter((d) => !isSameDay(d, dragStart))
            : [...selectedDays, dragStart]
        )
      } else {
        const range = getDatesInRange(dragStart, dragCurrent)
        const combined = [...selectedDays]
        range.forEach((dateInRange) => {
          if (!combined.some((d) => isSameDay(d, dateInRange))) {
            combined.push(dateInRange)
          }
        })
        onSelectedDaysChange(combined)
      }
    }
    setDragging(false)
    setDragStart(null)
    setDragCurrent(null)
  }

  return (
    <div onPointerUp={handlePointerUp}>
      <DateCalendar
        value={selectedDays.length ? selectedDays[0] : null}
        slots={{ day: MultiSelectDay }}
        slotProps={{
          day: (ownerState) => ({
            selectedDays,
            tempDays,
            hoveredDay,
            onPointerDown: () => handleDayPointerDown(ownerState.day),
            onPointerEnter: () => handleDayPointerEnter(ownerState.day),
            onMouseEnter: () => setHoveredDay(ownerState.day),
            onMouseLeave: () => setHoveredDay(null),
          }),
        }}
        showDaysOutsideCurrentMonth
      />
    </div>
  )
}
