import L from 'leaflet'

import { useEffect } from 'react'
import { useMap } from 'react-leaflet'

import './leafletSmoothWheelZoom'
import {
  classifyLineWheelInput,
  NOISY_LINE_BURST_GRACE_MS,
} from './smoothWheelZoomUtils'

type SmoothWheelOptions = {
  smoothSensitivity?: number
  smoothWheelZoom?: boolean | 'center'
}

type SmoothWheelHandler = L.Handler & {
  _centerPoint?: L.Point
  _goalZoom?: number
  _isWheeling?: boolean
  _onWheelStart: (event: WheelEvent) => void
  _onWheeling: (event: WheelEvent) => void
  _prevCenter?: L.LatLng
  _prevZoom?: number
  _startLatLng?: L.LatLng
  _timeoutId?: number
  _wheelMousePosition?: L.Point
  _wheelStartLatLng?: L.LatLng
  _zoomAnimationId?: number
}

type SmoothWheelMap = L.Map & {
  options: L.MapOptions & SmoothWheelOptions
  smoothWheelZoom?: SmoothWheelHandler
}

// The vendored handler enables itself globally when imported. Keep maps opt-in so
// static/report maps do not unexpectedly start responding to the mouse wheel.
L.Map.mergeOptions({ smoothWheelZoom: false } as L.MapOptions)

const SmoothWheelZoom = () => {
  const map = useMap()

  useEffect(() => {
    const smoothMap = map as SmoothWheelMap
    const handler = smoothMap.smoothWheelZoom

    if (!handler) return

    const wasEnabled = handler.enabled()
    if (wasEnabled) handler.disable()

    const originalOnWheelStart = handler._onWheelStart
    const originalOnWheeling = handler._onWheeling
    let gesturePosition: L.Point | null = null
    let wheelDirection = 0
    let lastWheelEventAt = 0
    let noisyLineBurst = false

    const handleWheelStart = (event: WheelEvent) => {
      const continuesNoisyLineBurst =
        event.deltaMode === WheelEvent.DOM_DELTA_LINE &&
        noisyLineBurst &&
        performance.now() - lastWheelEventAt <=
          NOISY_LINE_BURST_GRACE_MS

      if (!continuesNoisyLineBurst) {
        wheelDirection = 0
        noisyLineBurst = false
      }

      originalOnWheelStart.call(handler, event)

      const position = handler._wheelMousePosition
      const center = handler._centerPoint

      // The package stops its animation when the wheel is exactly centered.
      // A subpixel offset preserves center zooming without a visible pan.
      gesturePosition =
        position && center && position.equals(center)
          ? L.point(position.x + 0.0001, position.y)
          : (position?.clone() ?? null)
    }

    const handleWheeling = (event: WheelEvent) => {
      const nextPosition = smoothMap.mouseEventToContainerPoint(event)
      const pointerMoved =
        gesturePosition != null &&
        nextPosition.distanceTo(gesturePosition) >= 4

      if (pointerMoved) {
        const centerPoint = smoothMap.getSize().divideBy(2)
        const anchoredPosition = nextPosition.equals(centerPoint)
          ? L.point(nextPosition.x + 0.0001, nextPosition.y)
          : nextPosition

        // A second wheel input can arrive before the previous burst's timeout.
        // Rebase it to the current map and pointer instead of continuing to
        // zoom around the stale position where the burst began.
        gesturePosition = anchoredPosition
        handler._centerPoint = centerPoint
        handler._startLatLng =
          smoothMap.containerPointToLatLng(centerPoint)
        handler._wheelStartLatLng =
          smoothMap.containerPointToLatLng(anchoredPosition)
        handler._wheelMousePosition = anchoredPosition
        handler._goalZoom = smoothMap.getZoom()
        handler._prevCenter = smoothMap.getCenter()
        handler._prevZoom = smoothMap.getZoom()
      }

      const sensitivity = smoothMap.options.smoothSensitivity ?? 1
      const packageZoomChange =
        -event.deltaY * 0.003 * sensitivity
      const direction = Math.sign(packageZoomChange)
      const now = performance.now()
      const isLineMode = event.deltaMode === WheelEvent.DOM_DELTA_LINE

      // Some inexpensive wheels emit a long stream of line-mode events for one
      // physical notch. Give the first event normal weight, then keep honoring
      // the noisy tail at a deliberately tiny rate. A new gesture that only
      // follows a single event still receives normal weight.
      let zoomChange = packageZoomChange
      if (isLineMode) {
        const classification = classifyLineWheelInput({
          direction,
          elapsedMs: now - lastWheelEventAt,
          noisyLineBurst,
          previousDirection: wheelDirection,
        })
        noisyLineBurst = classification.noisyLineBurst

        zoomChange =
          direction *
          (classification.isTailEvent
            ? Math.min(Math.abs(packageZoomChange), 0.0005)
            : 0.3)
      }

      if (
        direction !== 0 &&
        wheelDirection !== 0 &&
        direction !== wheelDirection &&
        handler._goalZoom != null
      ) {
        const reversalDamping = 0.25
        const maxReversalZoomChange = 0.03
        const dampedZoomChange =
          Math.sign(zoomChange) *
          Math.min(
            Math.abs(zoomChange) * reversalDamping,
            maxReversalZoomChange
          )

        // Cancel queued movement in the old direction and damp the first event
        // after reversal. The handler adds zoomChange immediately afterward.
        handler._goalZoom =
          smoothMap.getZoom() + dampedZoomChange - packageZoomChange
      } else if (handler._goalZoom != null) {
        // The handler will add its own delta below; offset it so it sees the
        // normalized amount while retaining its bounds and timeout handling.
        handler._goalZoom += zoomChange - packageZoomChange
      }

      if (direction !== 0) wheelDirection = direction
      lastWheelEventAt = now

      originalOnWheeling.call(handler, event)

      if (gesturePosition) {
        handler._wheelMousePosition = gesturePosition
      }
    }

    handler._onWheelStart = handleWheelStart
    handler._onWheeling = handleWheeling
    smoothMap.options.smoothSensitivity = 1
    handler.enable()

    return () => {
      handler.disable()

      if (handler._zoomAnimationId != null) {
        cancelAnimationFrame(handler._zoomAnimationId)
      }
      if (handler._timeoutId != null) {
        clearTimeout(handler._timeoutId)
      }
      handler._isWheeling = false

      if (handler._onWheelStart === handleWheelStart) {
        handler._onWheelStart = originalOnWheelStart
      }
      if (handler._onWheeling === handleWheeling) {
        handler._onWheeling = originalOnWheeling
      }

      if (wasEnabled) handler.enable()
    }
  }, [map])

  return null
}

export default SmoothWheelZoom
