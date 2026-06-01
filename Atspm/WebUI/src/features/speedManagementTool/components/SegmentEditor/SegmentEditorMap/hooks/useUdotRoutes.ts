// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - useUdotRoutes.ts
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
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { useEffect } from 'react'

export const useUdotRoutes = () => {
  const {
    udotRoutes,
    isLoadingUdotRoutes,
    setUdotRoutes,
    setIsLoadingUdotRoutes,
  } = useSegmentEditorStore()

  const fetchUdotRoutes = async () => {
    if (udotRoutes.length > 0 || isLoadingUdotRoutes) {
      return
    }

    setIsLoadingUdotRoutes(true)
    try {
      const queryUrl =
        'https://roads.udot.utah.gov/server/rest/services/Public/UDOT_Routes/FeatureServer/0/query'
      const response = await fetch(queryUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: new URLSearchParams({
          where: '1=1',
          outSR: '4326',
          outFields: '*',
          returnGeometry: 'true',
          f: 'json',
        }),
      })
      const data = await response.json()
      if (data.features && data.features.length > 0) {
        setUdotRoutes(data.features)
      } else {
        setUdotRoutes([])
      }
    } catch (error) {
      setUdotRoutes([])
    } finally {
      setIsLoadingUdotRoutes(false)
    }
  }

  useEffect(() => {
    fetchUdotRoutes()
  }, [])

  return {
    udotRoutes: Array.isArray(udotRoutes) ? udotRoutes : [], // Extra safety
    fetchUdotRoutes,
    isLoadingUdotRoutes,
  }
}
