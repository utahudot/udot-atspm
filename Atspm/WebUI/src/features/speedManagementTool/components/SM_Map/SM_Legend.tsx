import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import L from 'leaflet'
import { useCallback, useEffect } from 'react'
import { useMap } from 'react-leaflet'

const SM_Legend = () => {
  const map = useMap()
  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()

  const getColor = (speed: number) => {
    if (speed < 20) return 'rgba(0, 115, 255, 1)'
    if (speed < 30) return 'rgba(0, 255, 170, 1)'
    if (speed < 35) return 'rgba(55, 255, 0, 1)'
    if (speed < 40) return 'rgba(175, 250, 0, 1)'
    if (speed < 45) return 'rgba(247, 214, 0, 1)'
    if (speed < 55) return 'rgba(245, 114, 0, 1)'
    if (speed < 65) return 'rgba(245, 57, 0, 1)'
    if (speed < 75) return 'rgba(245, 0, 0, 1)'
    return 'rgba(115, 0, 0, 1)'
  }

  const getViolationColor = (violation: number) => {
    if (violation < mediumMin) return ViolationColors.Low
    if (violation < mediumMax) return ViolationColors.Medium
    return ViolationColors.High
  }

  const createViolationsLegend = useCallback(() => {
    const legend = new L.Control({ position: 'bottomright' })

    legend.onAdd = () => {
      const div = L.DomUtil.create('div', 'map legend')
      const labels = []

      labels.push('<strong>Violations</strong> <br>')
      labels.push(
        `<i style="width: 18px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.Low}"></i> Low`
      )
      labels.push(
        `<i style="width: 18px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.Medium}"></i> Medium`
      )
      labels.push(
        `<i style="width: 18px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.High}"></i> High`
      )

      div.innerHTML = labels.join('<br>')
      div.style.backgroundColor = 'white'
      div.style.padding = '6px 8px'
      div.style.fontSize = '14px'
      div.style.lineHeight = '24px'
      div.style.borderRadius = '5px'
      div.style.boxShadow = '0 0 15px rgba(0,0,0,0.2)'
      div.style.lineHeight = '18px'
      div.style.opacity = '0.9'

      return div
    }

    legend.addTo(map)

    return () => {
      legend.remove()
    }
  }, [map])

  const createSpeedLegend = useCallback(() => {
    const legend = new L.Control({ position: 'bottomright' })

    legend.onAdd = () => {
      const div = L.DomUtil.create('div', 'map legend')
      const speeds = [20, 30, 35, 40, 45, 55, 65, 75]
      const labels = []

      labels.push('<strong>Speed (mph)</strong> <br>')

      for (let i = 0; i < speeds.length; i++) {
        const from = speeds[i - 1]
        const to = speeds[i]
        labels.push(
          `<i style="width: 18px; height: 18px; margin-right: 18px; float: left; background:${getColor(
            speeds[i] - 1
          )}"></i> ${from ? `${from}&ndash;${to}` : `<${to}`}`
        )
      }

      labels.push(
        `<i style="width: 18px; height: 18px; margin-right: 18px; float: left; background:${getColor(
          75
        )}"></i> 75+`
      )

      div.innerHTML = labels.join('<br>')
      div.style.backgroundColor = 'white'
      div.style.padding = '6px 8px'
      div.style.fontSize = '14px'
      div.style.lineHeight = '24px'
      div.style.borderRadius = '5px'
      div.style.boxShadow = '0 0 15px rgba(0,0,0,0.2)'
      div.style.lineHeight = '18px'
      div.style.opacity = '0.9'

      return div
    }

    legend.addTo(map)

    return () => {
      legend.remove()
    }
  }, [map])

  useEffect(() => {
    if (!map) return

    return routeRenderOption === RouteRenderOption.Violations
      ? createViolationsLegend()
      : createSpeedLegend()
  }, [
    map,
    routeRenderOption,
    mediumMin,
    mediumMax,
    createViolationsLegend,
    createSpeedLegend,
  ])

  return null
}

export default SM_Legend