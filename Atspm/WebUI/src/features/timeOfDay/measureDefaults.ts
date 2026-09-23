import type { MeasureOptionPreset } from '@/api/config'
import { TimeOfDayTuningOptionKey, TimeOfDayTuningOptions } from './types'

export const timeOfDayMeasureTypeId = 41

export const timeOfDaySchedulePresetOptionKeys = [
  'amEntryPctOfPeak',
  'amExitPctOfPeak',
  'pmEntryPctOfPeak',
  'pmExitPctOfPeak',
  'freeEntryPctOfDailyPeak',
  'freeEntryPctOfDynamicRange',
  'entrySustainedBins',
  'freeSustainedBins',
] as const satisfies readonly TimeOfDayTuningOptionKey[]

export type TimeOfDaySchedulePresetOptions = Pick<
  TimeOfDayTuningOptions,
  (typeof timeOfDaySchedulePresetOptionKeys)[number]
>

export interface TimeOfDaySchedulePreset {
  id: number
  name: string
  options: TimeOfDaySchedulePresetOptions
}

const parseSchedulePresetOptions = (
  option: MeasureOptionPreset['option']
): TimeOfDaySchedulePresetOptions | null => {
  if (!option) return null

  const options = {} as TimeOfDaySchedulePresetOptions

  for (const key of timeOfDaySchedulePresetOptionKeys) {
    const rawValue = option[key]
    if (rawValue === null || rawValue === undefined || rawValue === '') {
      return null
    }

    const value = Number(rawValue)
    if (!Number.isFinite(value)) return null
    options[key] = value
  }

  return options
}

export const buildTimeOfDaySchedulePresets = (
  presets: MeasureOptionPreset[] | null | undefined
): TimeOfDaySchedulePreset[] =>
  (presets ?? []).flatMap((preset) => {
    const name = preset.name?.trim()
    const options = parseSchedulePresetOptions(preset.option)

    return preset.id === undefined || !name || !options
      ? []
      : [{ id: preset.id, name, options }]
  })

export const findMatchingTimeOfDaySchedulePreset = (
  options: TimeOfDayTuningOptions,
  presets: TimeOfDaySchedulePreset[]
) =>
  presets.find((preset) =>
    timeOfDaySchedulePresetOptionKeys.every(
      (key) => options[key] === preset.options[key]
    )
  )
