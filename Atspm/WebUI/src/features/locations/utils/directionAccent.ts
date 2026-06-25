// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - directionAccent.ts
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
import { Color } from '@/features/charts/utils'

const UNKNOWN_DIRECTION_ACCENT = 'lightgrey'

function normalizeDirectionLabel(directionLabel?: string | null) {
  return String(directionLabel ?? '')
    .trim()
    .split(/\s+/)[0]
    .replace(/[^A-Za-z]/g, '')
    .toUpperCase()
}

export function getDirectionAccentColor(directionLabel?: string | null) {
  const direction = normalizeDirectionLabel(directionLabel)

  if (!direction || direction === 'NA') {
    return UNKNOWN_DIRECTION_ACCENT
  }

  if (
    direction.startsWith('NORTH') ||
    direction.startsWith('NB') ||
    direction.startsWith('NE') ||
    direction.startsWith('NW') ||
    direction === 'N'
  ) {
    return Color.Blue
  }

  if (
    direction.startsWith('SOUTH') ||
    direction.startsWith('SB') ||
    direction.startsWith('SE') ||
    direction.startsWith('SW') ||
    direction === 'S'
  ) {
    return Color.BrightRed
  }

  if (
    direction.startsWith('EAST') ||
    direction.startsWith('EB') ||
    direction === 'E'
  ) {
    return Color.Yellow
  }

  if (
    direction.startsWith('WEST') ||
    direction.startsWith('WB') ||
    direction === 'W'
  ) {
    return Color.Orange
  }

  return UNKNOWN_DIRECTION_ACCENT
}

export function getDirectionAccentBorder(
  directionLabel?: string | null,
  width = '7px'
) {
  const color = getDirectionAccentColor(directionLabel)
  return color === UNKNOWN_DIRECTION_ACCENT ? 'none' : `${width} solid ${color}`
}

export function getDirectionAccentForegroundColor(
  directionLabel?: string | null
) {
  const color = getDirectionAccentColor(directionLabel)

  switch (color) {
    case Color.Blue:
    case Color.BrightRed:
      return Color.White
    case Color.Yellow:
    case Color.Orange:
    default:
      return Color.Black
  }
}
