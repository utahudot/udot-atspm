// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - formatters.ts
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
import { formatNumber } from '@/utils/numberFormat'

export function formatSpeed(value: number | null | undefined, decimals = 2) {
  if (value == null) return ''
  return `${formatNumber(value, decimals)} mph`
}

export function formatPercent(
  value: string | number | null | undefined,
  decimals = 2
) {
  if (value == null) return ''
  const numericValue = typeof value === 'string' ? Number(value) : value
  return `${formatNumber(numericValue * 100, decimals)}%`
}

export function formatCount(
  value: string | number | null | undefined,
  decimals = 0
) {
  if (value == null) return ''
  return `${formatNumber(value, decimals)}`
}
