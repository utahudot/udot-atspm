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
        'https://roads.udot.utah.gov/server/rest/services/Public/UDOT_Routes/MapServer/0/query'
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
