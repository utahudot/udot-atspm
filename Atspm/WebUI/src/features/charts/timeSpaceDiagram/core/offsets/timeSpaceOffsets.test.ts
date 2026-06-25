// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceOffsets.test.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
