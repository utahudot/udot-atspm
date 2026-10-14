import type { TimeOfDayFormState } from './types'

export type TimeOfDayFormField = keyof TimeOfDayFormState

const areFormFieldValuesEqual = (left: unknown, right: unknown) =>
  Object.is(left, right) || JSON.stringify(left) === JSON.stringify(right)

export const getChangedTimeOfDayFormFields = (
  current: TimeOfDayFormState,
  next: TimeOfDayFormState
) => {
  const changedFields = new Set<TimeOfDayFormField>()

  ;(Object.keys(next) as TimeOfDayFormField[]).forEach((field) => {
    if (!areFormFieldValuesEqual(current[field], next[field])) {
      changedFields.add(field)
    }
  })

  return changedFields
}

export const mergeUntouchedTimeOfDayFormState = (
  current: TimeOfDayFormState,
  resolved: Partial<TimeOfDayFormState>,
  editedFields: ReadonlySet<TimeOfDayFormField>
) => {
  const next = { ...current }
  let changed = false

  ;(Object.keys(resolved) as TimeOfDayFormField[]).forEach((field) => {
    if (editedFields.has(field)) return

    const resolvedValue = resolved[field]
    if (areFormFieldValuesEqual(current[field], resolvedValue)) return
    ;(next as Record<TimeOfDayFormField, unknown>)[field] = resolvedValue
    changed = true
  })

  return changed ? next : current
}
