import * as LeafletRuntime from 'leaflet'

type LatLngLike = {
  lat: number
  lng: number
}

type PointLike = {
  x: number
  y: number
  add: (point: PointLike) => PointLike
  divideBy: (divisor: number) => PointLike
  subtract: (point: PointLike) => PointLike
}

type MarkerZoomEvent = {
  center: LatLngLike
  zoom: number
}

type MapInternals = {
  _getMapPanePos: () => PointLike
  getCenter: () => LatLngLike
  getSize: () => PointLike
  getZoom: () => number
  project: (latLng: LatLngLike, zoom: number) => PointLike
}

type FractionalZoomMarker = {
  _animateZoom: (event: MarkerZoomEvent) => void
  _atspmFractionalZoomVersion?: number
  _icon?: HTMLElement
  _latlng: LatLngLike
  _map: MapInternals
  _setPos: (position: PointLike) => void
  update: () => FractionalZoomMarker
}

type LeafletRuntimeModule = {
  Marker: {
    prototype: FractionalZoomMarker
  }
}

const installGlobalLeafletMarkerStability = () => {
  const markerPrototype = (LeafletRuntime as unknown as LeafletRuntimeModule)
    .Marker.prototype

  if (markerPrototype._atspmFractionalZoomVersion === 2) {
    return
  }

  const getPosition = (
    marker: FractionalZoomMarker,
    center: LatLngLike,
    zoom: number
  ) => {
    const map = marker._map

    return map
      .project(marker._latlng, zoom)
      .subtract(map.project(center, zoom))
      .add(map.getSize().divideBy(2))
      .subtract(map._getMapPanePos())
  }

  // Leaflet rounds both the marker and the map's pixel origin. Re-projecting
  // from the exact center avoids the subpixel reversals visible during zoom.
  markerPrototype._animateZoom = function (
    this: FractionalZoomMarker,
    event: MarkerZoomEvent
  ) {
    this._setPos(getPosition(this, event.center, event.zoom))
  }

  markerPrototype.update = function (this: FractionalZoomMarker) {
    if (this._icon && this._map) {
      this._setPos(
        getPosition(this, this._map.getCenter(), this._map.getZoom())
      )
    }

    return this
  }

  markerPrototype._atspmFractionalZoomVersion = 2
}

installGlobalLeafletMarkerStability()
