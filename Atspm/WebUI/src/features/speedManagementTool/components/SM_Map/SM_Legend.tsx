import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import L from 'leaflet'
import { useCallback, useEffect } from 'react'
import { useMap } from 'react-leaflet'

export const routeSpeeds = [
  {
    min: 0,
    max: 19,
    color: 'rgba(44, 123, 182, 1)',
    get display() {
      return `<${this.max + 1}`
    },
  }, // blue
  {
    min: 20,
    max: 29,
    color: 'rgba(0, 163, 163, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // teal
  {
    min: 30,
    max: 34,
    color: 'rgba(30, 142, 62, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // dark green
  {
    min: 35,
    max: 39,
    color: 'rgba(60, 180, 75, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // medium green
  {
    min: 40,
    max: 49,
    color: 'rgba(181, 189, 0, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // olive (not neon)
  {
    min: 50,
    max: 59,
    color: 'rgba(253, 174, 97, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // orange
  {
    min: 60,
    max: 69,
    color: 'rgba(244, 109, 67, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // orange-red
  {
    min: 70,
    max: 79,
    color: 'rgba(227, 26, 28, 1)',
    get display() {
      return `${this.min}–${this.max}`
    },
  }, // red
  {
    min: 80,
    max: Infinity,
    color: 'rgba(127, 0, 0, 1)',
    get display() {
      return `${this.min}+`
    },
  }, // dark red
] as const

export const getRouteColor = (speed: number) => {
  const rounded = Math.floor(speed)
  const range = routeSpeeds.find(
    (range) => rounded >= range.min && rounded <= range.max
  )
  return range && rounded !== 0 ? range.color : '#000'
}

const SM_Legend = () => {
  const map = useMap()
  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()

  const createViolationsLegend = useCallback(() => {
    const legend = new L.Control({ position: 'bottomright' })

    legend.onAdd = () => {
      const div = L.DomUtil.create('div', 'map legend')
      const labels = []

      labels.push('<strong>Violations</strong> <br>')
      labels.push(
        `<i style="width: 30px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.Low}"></i> Low`
      )
      labels.push(
        `<i style="width: 30px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.Medium}"></i> Medium`
      )
      labels.push(
        `<i style="width: 30px; height: 18px; margin-right: 18px; float: left; background:${ViolationColors.High}"></i> High`
      )

      div.innerHTML = labels.join('<br>')
      div.style.backgroundColor = 'white'
      div.style.padding = '6px 8px'
      div.style.fontSize = '14px'
      div.style.lineHeight = '24px'
      div.style.borderRadius = '5px'
      div.style.boxShadow = '0 0 15px rgba(0,0,0,0.2)'
      div.style.lineHeight = '18px'

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
      const labels = []

      labels.push(`<strong>${routeRenderOption.toString()} (mph)</strong> <br>`)

      // Loop through routeSpeeds and use display and color directly
      routeSpeeds.forEach(({ display, color }) => {
        labels.push(
          `<i style="width: 30px; height: 18px; margin-right: 18px; float: left; background:${color}"></i> ${display}`
        )
      })

      labels.push(
        `<div style="margin-top: 10px"><i style="width: 30px; height: 18px; margin-right: 18px; float: left; background:#000"></i> No Data</div>`
      )

      div.innerHTML = labels.join('<br>')

      Object.assign(div.style, {
        backgroundColor: 'white',
        padding: '6px 8px',
        fontSize: '14px',
        borderRadius: '5px',
        boxShadow: '0 0 15px rgba(0,0,0,0.2)',
        lineHeight: '18px',
        opacity: '1',
        width: '180px',
      })

      return div
    }

    legend.addTo(map)

    return () => {
      legend.remove()
    }
  }, [map, routeRenderOption])

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
