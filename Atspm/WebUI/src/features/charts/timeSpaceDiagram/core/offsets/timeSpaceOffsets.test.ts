import {
  formatOffsetSeconds,
  getEquivalentCycleOffsetVisuals,
  getOffsetSecondsValue,
  getOffsetUserAdjustmentSeconds,
  hasModifiedOffset,
  normalizeOffsetSeconds,
  normalizeOffsetToCycleLengthSeconds,
} from './timeSpaceOffsets'

describe('timeSpaceOffsets', () => {
  it('normalizes offsets to tenths and removes negative zero', () => {
    expect(normalizeOffsetSeconds(1.04)).toBe(1)
    expect(normalizeOffsetSeconds(1.06)).toBe(1.1)
    expect(normalizeOffsetSeconds(-0.04)).toBe(0)
    expect(normalizeOffsetSeconds(Number.NaN)).toBe(0)
  })

  it('wraps offsets only when a valid cycle length is present', () => {
    expect(normalizeOffsetToCycleLengthSeconds(125, 60)).toBe(5)
    expect(normalizeOffsetToCycleLengthSeconds(-125, 60)).toBe(-5)
    expect(normalizeOffsetToCycleLengthSeconds(12.34, null)).toBe(12.3)
    expect(normalizeOffsetToCycleLengthSeconds(12.34, 'bad')).toBe(12.3)
  })

  it('formats offset values and parses numeric inputs safely', () => {
    expect(formatOffsetSeconds(12.34)).toBe('12.3s')
    expect(formatOffsetSeconds('15')).toBe('15s')
    expect(formatOffsetSeconds(undefined)).toBe('unknown')
    expect(formatOffsetSeconds('bad')).toBe('unknown')

    expect(getOffsetSecondsValue('7.25')).toBe(7.3)
    expect(getOffsetSecondsValue('')).toBeNull()
    expect(getOffsetUserAdjustmentSeconds(undefined)).toBe(0)
    expect(getOffsetUserAdjustmentSeconds('-2.25')).toBe(-2.3)
  })

  it('treats user adjustments as modified even when the displayed and actual offsets match', () => {
    expect(hasModifiedOffset(10, 10, 0)).toBe(false)
    expect(hasModifiedOffset(10, 11, 0)).toBe(true)
    expect(hasModifiedOffset(10, 10, 5)).toBe(true)
    expect(hasModifiedOffset(null, 10, 5)).toBe(false)
  })

  it('returns the neutral equivalent-cycle visuals', () => {
    expect(getEquivalentCycleOffsetVisuals()).toEqual({
      direction: 'neutral',
      highlightFill: 'rgba(100, 116, 139, 0.12)',
      highlightStroke: 'rgba(100, 116, 139, 0.22)',
      valueColor: '#475569',
    })
  })
})
