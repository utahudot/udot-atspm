// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - numberFormat.ts
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
export function roundTo(
  value: number | null | undefined,
  decimals: number
): number | null {
  if (value == null) return null
  const factor = 10 ** decimals
  return Math.round(value * factor) / factor
}

export function formatNumber(
  value: number | string | null | undefined,
  decimals = 0
): string {
  if (value == null) return ''

  const numeric = typeof value === 'number' ? value : Number(String(value))

  if (!Number.isFinite(numeric)) return ''

  if (decimals === 0) {
    return String(Math.round(numeric))
  }

  return numeric.toFixed(decimals)
}
