// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - sortApproaches.ts
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

import { ConfigApproach } from '@/features/locations/components/editLocation/locationStore'

export const sortApproachesByPhaseNumber = (approaches: ConfigApproach[]) => {
  return [...approaches].sort((a, b) => {
    // Extract the digit from the description, if present
    const aNum = a?.description?.match(/\d+/) // Match all digits to handle multi-digit numbers
    const bNum = b?.description?.match(/\d+/)

    // Handle cases where the description might not contain any digits
    if (!aNum && !bNum) return 0 // Both are without numbers, maintain order
    if (!aNum) return -1 // a without a number, should come first
    if (!bNum) return 1 // b without a number, should come first

    // Compare by numerical value if both have numbers
    return Number(aNum[0]) - Number(bNum[0])
  })
}

export const sortDetectorsByChannel = (approaches: ConfigApproach[]) => {
  return approaches.map((approach) => {
    const sortedDetectors = [...approach.detectors].sort((a, b) => {
      return a.detectorChannel - b.detectorChannel
    })
    return { ...approach, detectors: sortedDetectors }
  })
}

export const sortApproachesAndDetectors = (approaches: ConfigApproach[]) => {
  const sortedApproaches = sortApproachesByPhaseNumber(approaches)
  return sortDetectorsByChannel(sortedApproaches)
}
