// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceOffsets.ts
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
type OffsetDeltaDirection = 'positive' | 'negative' | 'neutral'

export type OffsetDeltaVisuals = {
  direction: OffsetDeltaDirection
  highlightFill: string
  highlightStroke: string
  valueColor: string
}

export function normalizeOffsetSeconds(value: number): number {
  if (!Number.isFinite(value)) {
    return 0
  }

  const normalized = Number.isInteger(value) ? value : Number(value.toFixed(1))
  return Object.is(normalized, -0) ? 0 : normalized
}

function getCycleLengthSecondsValue(value: unknown): number | null {
  if (value == null || value === '') {
    return null
  }

  const numericValue = typeof value === 'number' ? value : Number(value)
  if (!Number.isFinite(numericValue) || numericValue <= 0) {
    return null
  }

  return normalizeOffsetSeconds(numericValue)
}

export function normalizeOffsetToCycleLengthSeconds(
  value: number,
  cycleLengthValue: unknown
): number {
  const normalizedValue = normalizeOffsetSeconds(value)
  const cycleLengthSeconds = getCycleLengthSecondsValue(cycleLengthValue)

  if (cycleLengthSeconds == null) {
    return normalizedValue
  }

  return normalizeOffsetSeconds(normalizedValue % cycleLengthSeconds)
}

export function offsetsMatch(
  currentOffsetSeconds: number,
  actualOffsetSeconds: number
) {
  return Math.abs(currentOffsetSeconds - actualOffsetSeconds) < 0.0001
}

export function formatSignedOffsetSeconds(value: number): string {
  const normalized = normalizeOffsetSeconds(value)
  if (normalized === 0) {
    return '0s'
  }

  const absoluteValue = Math.abs(normalized)
  const formatted = Number.isInteger(absoluteValue)
    ? absoluteValue.toString()
    : absoluteValue.toFixed(1)

  return normalized > 0 ? `+${formatted}s` : `-${formatted}s`
}

export function formatOffsetSeconds(value: unknown): string {
  if (value == null || value === '') {
    return 'unknown'
  }

  const numericValue = typeof value === 'number' ? value : Number(value)
  if (!Number.isFinite(numericValue)) {
    return 'unknown'
  }

  const normalized = normalizeOffsetSeconds(numericValue)
  const formatted = Number.isInteger(normalized)
    ? normalized.toString()
    : normalized.toFixed(1)

  return `${formatted}s`
}

export function getOffsetSecondsValue(value: unknown): number | null {
  if (value == null || value === '') {
    return null
  }

  const numericValue = typeof value === 'number' ? value : Number(value)
  if (!Number.isFinite(numericValue)) {
    return null
  }

  return normalizeOffsetSeconds(numericValue)
}

export function getOffsetUserAdjustmentSeconds(value: unknown): number {
  const adjustmentSeconds = getOffsetSecondsValue(value)
  return adjustmentSeconds == null ? 0 : adjustmentSeconds
}

export function hasModifiedOffset(
  currentOffsetValue: unknown,
  actualOffsetValue: unknown,
  userAdjustmentValue?: unknown
) {
  const currentOffsetSeconds = getOffsetSecondsValue(currentOffsetValue)
  const actualOffsetSeconds =
    getOffsetSecondsValue(actualOffsetValue) ?? currentOffsetSeconds

  if (currentOffsetSeconds == null || actualOffsetSeconds == null) {
    return false
  }

  const userAdjustmentSeconds =
    getOffsetUserAdjustmentSeconds(userAdjustmentValue)

  return (
    Math.abs(userAdjustmentSeconds) >= 0.0001 ||
    !offsetsMatch(currentOffsetSeconds, actualOffsetSeconds)
  )
}

export function getOffsetDeltaVisuals(value: number): OffsetDeltaVisuals {
  const normalized = normalizeOffsetSeconds(value)

  if (normalized > 0) {
    return {
      direction: 'positive',
      highlightFill: 'rgba(22, 163, 74, 0.18)',
      highlightStroke: 'rgba(22, 163, 74, 0.32)',
      valueColor: '#357A60',
    }
  }

  if (normalized < 0) {
    return {
      direction: 'negative',
      highlightFill: 'rgba(220, 38, 38, 0.18)',
      highlightStroke: 'rgba(220, 38, 38, 0.32)',
      valueColor: '#B45757',
    }
  }

  return {
    direction: 'neutral',
    highlightFill: 'transparent',
    highlightStroke: 'transparent',
    valueColor: '#0F172A',
  }
}

export function getEquivalentCycleOffsetVisuals(): OffsetDeltaVisuals {
  return {
    direction: 'neutral',
    highlightFill: 'rgba(100, 116, 139, 0.12)',
    highlightStroke: 'rgba(100, 116, 139, 0.22)',
    valueColor: '#475569',
  }
}
