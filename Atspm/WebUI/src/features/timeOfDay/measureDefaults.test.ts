import {
  buildTimeOfDaySchedulePresets,
  findMatchingTimeOfDaySchedulePreset,
} from './measureDefaults'
import { timeOfDayDefaultTuningOptions } from './types'

const presetRecords = [
  {
    id: 4101,
    name: 'Commuter Arterial',
    option: {
      amEntryPctOfPeak: 0.6,
      amExitPctOfPeak: 0.42,
      pmEntryPctOfPeak: 0.72,
      pmExitPctOfPeak: 0.4,
      freeEntryPctOfDailyPeak: 0.2,
      freeEntryPctOfDynamicRange: 0.16,
      entrySustainedBins: 2,
      freeSustainedBins: 4,
    },
  },
  {
    id: 4102,
    name: 'Suburban Mixed-Use',
    option: {
      amEntryPctOfPeak: 0.55,
      amExitPctOfPeak: 0.4,
      pmEntryPctOfPeak: 0.68,
      pmExitPctOfPeak: 0.38,
      freeEntryPctOfDailyPeak: 0.22,
      freeEntryPctOfDynamicRange: 0.18,
      entrySustainedBins: 2,
      freeSustainedBins: 4,
    },
  },
  {
    id: 4103,
    name: 'Retail / Commercial',
    option: {
      amEntryPctOfPeak: 0.5,
      amExitPctOfPeak: 0.38,
      pmEntryPctOfPeak: 0.62,
      pmExitPctOfPeak: 0.36,
      freeEntryPctOfDailyPeak: 0.25,
      freeEntryPctOfDynamicRange: 0.2,
      entrySustainedBins: 3,
      freeSustainedBins: 4,
    },
  },
  {
    id: 4104,
    name: 'Weekend / Recreation',
    option: {
      amEntryPctOfPeak: 0.48,
      amExitPctOfPeak: 0.36,
      pmEntryPctOfPeak: 0.6,
      pmExitPctOfPeak: 0.34,
      freeEntryPctOfDailyPeak: 0.24,
      freeEntryPctOfDynamicRange: 0.2,
      entrySustainedBins: 3,
      freeSustainedBins: 5,
    },
  },
]

describe('time-of-day schedule presets', () => {
  test('parses the seeded measure-option presets', () => {
    const presets = buildTimeOfDaySchedulePresets(presetRecords)

    expect(presets).toHaveLength(4)
    expect(presets.map(({ id, name }) => ({ id, name }))).toEqual([
      { id: 4101, name: 'Commuter Arterial' },
      { id: 4102, name: 'Suburban Mixed-Use' },
      { id: 4103, name: 'Retail / Commercial' },
      { id: 4104, name: 'Weekend / Recreation' },
    ])
    expect(presets[3].options).toEqual(presetRecords[3].option)
  })

  test('matches a preset only while all schedule values remain unchanged', () => {
    const presets = buildTimeOfDaySchedulePresets(presetRecords)

    expect(
      findMatchingTimeOfDaySchedulePreset(
        timeOfDayDefaultTuningOptions,
        presets
      )?.id
    ).toBe(4102)
    expect(
      findMatchingTimeOfDaySchedulePreset(
        { ...timeOfDayDefaultTuningOptions, amEntryPctOfPeak: 0.56 },
        presets
      )
    ).toBeUndefined()
  })
})
