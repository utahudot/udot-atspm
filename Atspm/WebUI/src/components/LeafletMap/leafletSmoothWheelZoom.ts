import L from 'leaflet'

import {
  clampZoom,
  getEasedZoom,
} from './smoothWheelZoomUtils'

declare module 'leaflet' {
  interface MapOptions {
    smoothSensitivity?: number
    smoothWheelZoom?: boolean | 'center'
  }

  interface Map {
    smoothWheelZoom?: SmoothWheelZoomHandler
  }
}

type InternalMap = L.Map & {
  _limitZoom: (zoom: number) => number
  _move: (center: L.LatLng, zoom: number) => void
  _moveEnd: (zoomChanged?: boolean) => void
  _moveStart: (zoomChanged?: boolean, noMoveStart?: boolean) => void
  _panAnim?: { stop: () => void }
  _stop: () => void
}

export class SmoothWheelZoomHandler extends L.Handler {
  declare _map: L.Map

  _centerPoint?: L.Point
  _goalZoom?: number
  _isWheeling = false
  _moved = false
  _prevCenter?: L.LatLng
  _prevZoom?: number
  _startLatLng?: L.LatLng
  _timeoutId?: number
  _wheelMousePosition?: L.Point
  _wheelStartLatLng?: L.LatLng
  _zoomAnimationId?: number

  addHooks() {
    L.DomEvent.on(
      this._map.getContainer(),
      'wheel',
      this._onWheelScroll,
      this
    )
  }

  removeHooks() {
    L.DomEvent.off(
      this._map.getContainer(),
      'wheel',
      this._onWheelScroll,
      this
    )
  }

  _onWheelScroll(event: WheelEvent) {
    L.DomEvent.preventDefault(event)

    if (!this._isWheeling) {
      this._onWheelStart(event)
    }
    this._onWheeling(event)
  }

  _onWheelStart(event: WheelEvent) {
    const map = this._map as InternalMap

    this._isWheeling = true
    this._wheelMousePosition = map.mouseEventToContainerPoint(event)
    this._centerPoint = map.getSize().divideBy(2)
    this._startLatLng = map.containerPointToLatLng(this._centerPoint)
    this._wheelStartLatLng = map.containerPointToLatLng(
      this._wheelMousePosition
    )
    this._moved = false

    map._stop()
    map._panAnim?.stop()

    this._goalZoom = map.getZoom()
    this._prevCenter = map.getCenter()
    this._prevZoom = map.getZoom()

    this._zoomAnimationId = requestAnimationFrame(
      this._updateWheelZoom.bind(this)
    )
  }

  _onWheeling(event: WheelEvent) {
    const map = this._map as InternalMap
    const sensitivity = map.options.smoothSensitivity ?? 1

    this._goalZoom =
      (this._goalZoom ?? map.getZoom()) -
      event.deltaY * 0.003 * sensitivity

    if (
      this._goalZoom < map.getMinZoom() ||
      this._goalZoom > map.getMaxZoom()
    ) {
      this._goalZoom = map._limitZoom(this._goalZoom)
    }

    this._wheelMousePosition = map.mouseEventToContainerPoint(event)

    clearTimeout(this._timeoutId)
    this._timeoutId = window.setTimeout(this._onWheelEnd.bind(this), 200)
  }

  _onWheelEnd() {
    this._isWheeling = false
    cancelAnimationFrame(this._zoomAnimationId ?? 0)
    ;(this._map as InternalMap)._moveEnd(true)
  }

  _updateWheelZoom() {
    const map = this._map as InternalMap
    const currentCenter = map.getCenter()
    const currentZoom = map.getZoom()

    if (
      !this._prevCenter ||
      !currentCenter.equals(this._prevCenter) ||
      currentZoom !== this._prevZoom
    ) {
      const remainingZoom =
        this._goalZoom != null && this._prevZoom != null
          ? this._goalZoom - this._prevZoom
          : 0
      const wheelPosition = this._wheelMousePosition

      this._centerPoint = map.getSize().divideBy(2)
      this._startLatLng = map.containerPointToLatLng(this._centerPoint)
      if (wheelPosition) {
        this._wheelStartLatLng =
          map.containerPointToLatLng(wheelPosition)
      }
      this._goalZoom = clampZoom(
        currentZoom + remainingZoom,
        map.getMinZoom(),
        map.getMaxZoom()
      )
      this._prevCenter = currentCenter
      this._prevZoom = currentZoom
      this._moved = false
    }

    const goalZoom = this._goalZoom ?? map.getZoom()
    const zoom = getEasedZoom(map.getZoom(), goalZoom)
    const wheelPosition = this._wheelMousePosition
    const centerPoint = this._centerPoint
    const wheelStartLatLng = this._wheelStartLatLng

    if (!wheelPosition || !centerPoint || !wheelStartLatLng) return

    const delta = wheelPosition.subtract(centerPoint)
    if (delta.x === 0 && delta.y === 0) return

    const center =
      map.options.smoothWheelZoom === 'center'
        ? (this._startLatLng ?? map.getCenter())
        : map.unproject(
            map.project(wheelStartLatLng, zoom).subtract(delta),
            zoom
          )

    if (!this._moved) {
      map._moveStart(true, false)
      this._moved = true
    }

    map._move(center, zoom)
    this._prevCenter = map.getCenter()
    this._prevZoom = map.getZoom()

    this._zoomAnimationId = requestAnimationFrame(
      this._updateWheelZoom.bind(this)
    )
  }
}

L.Map.mergeOptions({
  smoothSensitivity: 1,
  smoothWheelZoom: true,
})

;(L.Map as unknown as { SmoothWheelZoom: typeof SmoothWheelZoomHandler })
  .SmoothWheelZoom = SmoothWheelZoomHandler

;(L.Map.addInitHook as (...args: unknown[]) => void)(
  'addHandler',
  'smoothWheelZoom',
  SmoothWheelZoomHandler
)
