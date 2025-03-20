// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - checkDetectors.ts
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

export const hasUniqueDetectorChannels = (
  channelMap: Map<number, number>
): {
  isValid: boolean
  errors: Record<string, { error: string; id: string }>
} => {
  const channelToIds = new Map<number, string[]>()
  const errors: Record<string, { error: string; id: string }> = {}

  for (const [detectorId, channel] of channelMap.entries()) {
    if (channel !== 0 && channel !== null && channel !== undefined) {
      const detectorIdStr = String(detectorId)
      if (!channelToIds.has(channel)) {
        channelToIds.set(channel, [detectorIdStr])
      } else {
        channelToIds.get(channel)!.push(detectorIdStr)
      }
    }
  }

  for (const [_, ids] of channelToIds.entries()) {
    if (ids.length > 1) {
      ids.forEach((id) => {
        errors[id] = { error: `Duplicate detector channel`, id }
      })
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  }
}
