import L from 'leaflet'

export const markerIcon = L.divIcon({
  className: 'selected-point-icon',
  html: `<div style="
    width: 10px;
    height: 10px;
    background: #1859b5;
    border-radius: 50%;
    cursor: pointer;
  "></div>`,
  iconSize: [10, 10],
  iconAnchor: [5, 5],
})
