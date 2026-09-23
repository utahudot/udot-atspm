import {
  ImpactDto,
  RouteSpeed,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  RoutesResponse,
  SpeedManagementRoute,
} from '@/features/speedManagementTool/types/routes'
import html2canvas from 'html2canvas'
import { MemoExoticComponent } from 'react'
import { createRoot } from 'react-dom/client'
import { HotSpotForReportMap, ImpactHotspotForReportMap } from '../../types'

interface MapProps {
  routes: SpeedManagementRoute[]
  hotspots: HotSpotForReportMap[]
  impacts: ImpactHotspotForReportMap[]
}

export interface ReportMapHandler {
  generateMapSnapshot(
    routeSpeeds: RoutesResponse,
    report: RouteSpeed[],
    ReportMap: MemoExoticComponent<(props: MapProps) => JSX.Element>,
    impacts?: ImpactDto[]
  ): Promise<string>
}

export const useReportMapHandler = (): ReportMapHandler => {
  const getHotspotSegments = (
    segmentIds: string[],
    routes: SpeedManagementRoute[]
  ): HotSpotForReportMap[] => {
    if (segmentIds.length === 0) {
      return []
    }
    return segmentIds.map((id) => {
      const coordinates = routes.find(
        (route) => route.properties.route_id === id
      )?.geometry.coordinates as [number, number][]
      return {
        segmentId: id,
        coordinates: coordinates?.map((pair) => [pair[1], pair[0]]),
      }
    })
  }

  const getHotspotsBasedOnChartType = (
    speedSegments: RouteSpeed[],
    segmentSpeeds: SpeedManagementRoute[]
  ): HotSpotForReportMap[] => {
    let hotspotSegments: HotSpotForReportMap[] = []
    const segmentIds = speedSegments?.map((s) => s.segmentId as string)
    hotspotSegments = getHotspotSegments(segmentIds, segmentSpeeds)
    return hotspotSegments
  }

  const getAdjustedRoutes = (
    routeSpeeds: RoutesResponse
  ): SpeedManagementRoute[] => {
    const filteredRoutes = routeSpeeds?.features.filter(
      (route) => route?.geometry?.coordinates
    )

    const routes: SpeedManagementRoute[] =
      filteredRoutes?.map((feature) => ({
        ...feature,
        geometry: {
          ...feature.geometry,
          coordinates: feature.geometry.coordinates.map((coord) => [
            coord[1],
            coord[0],
          ]),
        },
        properties: feature.properties,
      })) || []

    return routes
  }

  const getImpactHostpot = (
    impacts: ImpactDto[],
    routeSpeeds: SpeedManagementRoute[]
  ): ImpactHotspotForReportMap[] => {
    const impactSegments: ImpactHotspotForReportMap[] = impacts.map(
      (impact) => {
        const segmentIds = impact.segmentIds as string[]
        const coordinates = getHotspotSegments(
          segmentIds.filter((id) =>
            routeSpeeds.find((r) => r.properties.route_id === id)
          ),
          routeSpeeds
        )
        return {
          impactId: impact.id as string,
          impactedSegments: coordinates,
        }
      }
    )
    return impactSegments
  }

  const createHotspotSegmentsFromImpacts = (
    impacts: ImpactHotspotForReportMap[]
  ): HotSpotForReportMap[] => {
    let hotspotSegments: HotSpotForReportMap[] = []
    impacts.forEach((impact) => {
      const middleImpactSegment = Math.floor(impact.impactedSegments.length / 2)
      const middleSegment = impact.impactedSegments[middleImpactSegment]
      hotspotSegments = [...hotspotSegments, middleSegment]
    })
    return hotspotSegments
  }

  const generateMapSnapshot = async (
    routeSpeeds: RoutesResponse,
    segmentSpeeds: RouteSpeed[],
    ReportMap: MemoExoticComponent<(props: MapProps) => JSX.Element>,
    impacts?: ImpactDto[]
  ): Promise<string> => {
    // const { default: ReportMap } = await import(
    //   '../../../ExportableReports/components/reportMap/ReportMap'
    // )
    const container = document.createElement('div')
    container.style.width = '1200px'
    container.style.height = '800px'
    container.style.position = 'absolute'
    container.style.top = '-10000px'
    container.style.left = '-10000px'
    document.body.appendChild(container)
    const root = createRoot(container)
    const speedSegments = getAdjustedRoutes(routeSpeeds)
    let impactSegments: ImpactHotspotForReportMap[] = []
    let hotspotSegments: HotSpotForReportMap[] = []
    if (impacts?.length) {
      impactSegments = getImpactHostpot(impacts, speedSegments)
      hotspotSegments = createHotspotSegmentsFromImpacts(impactSegments)
    } else {
      hotspotSegments = getHotspotsBasedOnChartType(
        segmentSpeeds,
        speedSegments
      )
    }

    try {
      root.render(
        <ReportMap
          routes={speedSegments || []}
          hotspots={hotspotSegments || []}
          impacts={impactSegments || []}
        />
      )
      await new Promise((resolve) => setTimeout(resolve, 500)) // Adjust delay if needed

      // Wait for the map to fully render
      const waitForMapToRender = async (): Promise<void> => {
        const promises: Promise<void>[] = []
        const tileLayer = document.querySelector(
          '.leaflet-tile-layer'
        ) as HTMLImageElement
        if (tileLayer) {
          promises.push(
            new Promise<void>((resolve) => {
              // Check if tileLayer is an image element and attach the onload event
              tileLayer.onload = () => resolve()
            })
          )
        }
      }

      await waitForMapToRender()

      // Capture the map as a base64 image
      const canvas = await html2canvas(container, {
        useCORS: true, // Enable if there are external resources like tiles
      })
      const base64 = canvas.toDataURL('image/png')
      return base64
    } catch (error) {
      console.error('Error generating map snapshot:', error)
      return ''
    } finally {
      if (root) {
        root.unmount()
      }
      container.remove()
    }
  }

  return { generateMapSnapshot }
}
