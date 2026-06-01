import { ClearGuideSpiderLayer } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/ClearGuideSpiderLayer'
import { Entity } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { DataSource } from '@/features/speedManagementTool/enums'
import L from 'leaflet'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet/dist/leaflet.css'
import { useEffect, useRef } from 'react'
import { Pane } from 'react-leaflet'
import ClusterGroup from 'react-leaflet-cluster'
import ClearGuideSegment from './ClearGuideSegment'
import EntityMarker, {
  atspmIcon,
  atspmSelectedIcon,
  pemsIcon,
  pemsSelectedIcon,
} from './EntityMarker'

interface EntityClusterGroupProps {
  entities: Entity[]
  associatedEntityIds: string[]
  setAssociatedEntityIds: (ids: string[]) => void
  setHoveredEntity: (entity: Entity | null) => void
}

export default function EntityClusterGroup({
  entities,
  associatedEntityIds,
  setAssociatedEntityIds,
  setHoveredEntity,
}: EntityClusterGroupProps) {
  const idsRef = useRef<string[]>(associatedEntityIds)
  useEffect(() => {
    idsRef.current = associatedEntityIds
  }, [associatedEntityIds])

  const clearGuide = entities.filter(
    (e) => e.sourceId === DataSource.ClearGuide
  )
  const others = entities.filter((e) => e.sourceId !== DataSource.ClearGuide)

  const makeIcon = (markers: L.Marker[]) => {
    const total = markers.length
    const selected = markers.filter((m) =>
      idsRef.current.includes((m.options as any).entity.id)
    ).length
    const hasP = markers.some(
      (m) => (m.options as any).entity.sourceId === DataSource.PeMS
    )
    const hasA = markers.some(
      (m) => (m.options as any).entity.sourceId === DataSource.ATSPM
    )
    const red = '#ec3131',
      blue = '#0e70c6'
    const bg =
      hasP && hasA
        ? `linear-gradient(to right, ${red} 50%, ${blue} 50%)`
        : hasP
          ? red
          : blue

    return L.divIcon({
      html: `<div style="
        background:${bg};
        border-radius:12.5px;
        width:25px;height:25px;
        display:flex;align-items:center;justify-content:center;
        color:white;font-size:10px;font-weight:bold;
      ">${selected}/${total}</div>`,
      className: '',
      iconSize: L.point(25, 25),
    })
  }

  const handleClusterClick = (e: any) => {
    const cluster = e.layer
    const markers = cluster.getAllChildMarkers() as L.Marker[]
    const items = markers
      .map((mk) => {
        const d = (mk.options as any).entity as Entity
        const sel = idsRef.current.includes(d.id)
        const iconUrl =
          d.sourceId === DataSource.PeMS
            ? sel
              ? pemsSelectedIcon.options.iconUrl
              : pemsIcon.options.iconUrl
            : sel
              ? atspmSelectedIcon.options.iconUrl
              : atspmIcon.options.iconUrl
        return `<li data-id="${d.id}" style="
        display:flex;align-items:center;
        padding:8px 12px;
        border-bottom:1px solid rgba(0,0,0,0.05);
        cursor:pointer;transition:background 0.2s;
      ">
        <img src="${iconUrl}" style="width:24px;height:24px;margin-right:8px;" />
        <div style="line-height:1.2;">
          <div style="font-weight:500;">
            ${d.name ? `${d.name} - ` : ''}${d.entityId} (${d.direction})
          </div>
          <div style="font-size:0.85em;color:rgba(0,0,0,0.6);">
            ${d.entityType} • ${d.startDate}
          </div>
        </div>
      </li>`
      })
      .join('')

    const html = `<div style="
      max-height:360px;
      overflow-y:auto;
      box-sizing:border-box;
    "><ul style="list-style:none;margin:0;padding:0">${items}</ul></div>`

    cluster
      .bindPopup(html, {
        className: 'cluster-popup',
        pane: 'entity-popup',
      })
      .openPopup()

    setTimeout(() => {
      const popupEl = cluster.getPopup()?.getElement()
      if (!popupEl) return
      popupEl.querySelectorAll('li').forEach((li) => {
        li.addEventListener(
          'mouseenter',
          () => (li.style.background = 'rgba(0,0,0,0.05)')
        )
        li.addEventListener(
          'mouseleave',
          () => (li.style.background = 'transparent')
        )
        li.addEventListener('click', () => {
          const id = li.getAttribute('data-id')
          if (!id) return
          const curr = idsRef.current
          const updated = curr.includes(id)
            ? curr.filter((x) => x !== id)
            : [...curr, id]
          setAssociatedEntityIds(updated)
          idsRef.current = updated
          const img = li.querySelector('img')
          if (img) {
            const d = (
              markers.find((mk) => (mk.options as any).entity.id === id)
                .options as any
            ).entity as Entity
            img.src =
              d.sourceId === DataSource.PeMS
                ? updated.includes(id)
                  ? pemsSelectedIcon.options.iconUrl
                  : pemsIcon.options.iconUrl
                : updated.includes(id)
                  ? atspmSelectedIcon.options.iconUrl
                  : atspmIcon.options.iconUrl
          }
        })
      })
    }, 0)
  }

  return (
    <>
      <Pane name="entity-popup" style={{ zIndex: 12000 }} />
      <Pane
        name="clearguide-lines"
        style={{ zIndex: 3100, pointerEvents: 'none' }}
      >
        {clearGuide.map((e) => (
          <ClearGuideSegment
            key={e.id}
            entity={e}
            associatedEntityIds={associatedEntityIds}
            setAssociatedEntityIds={setAssociatedEntityIds}
            setHoveredEntity={setHoveredEntity}
          />
        ))}
      </Pane>
      <Pane name="entity-markers" style={{ zIndex: 7000 }}>
        <ClusterGroup
          clusterPane="entity-markers"
          markerPane="entity-markers"
          showCoverageOnHover={false}
          spiderfyOnMaxZoom={false}
          zoomToBoundsOnClick={false}
          maxClusterRadius={5}
          iconCreateFunction={(cluster) =>
            makeIcon(cluster.getAllChildMarkers() as L.Marker[])
          }
          eventHandlers={{ clusterclick: handleClusterClick }}
        >
          {others.map((e) => (
            <EntityMarker
              key={e.id}
              entity={e}
              isSelected={idsRef.current.includes(e.id)}
              associatedEntityIds={associatedEntityIds}
              setAssociatedEntityIds={setAssociatedEntityIds}
              setHoveredEntity={setHoveredEntity}
            />
          ))}
        </ClusterGroup>
      </Pane>
      <Pane name="clearguide-spider" style={{ zIndex: 10_000 }}>
        <ClearGuideSpiderLayer
          entities={clearGuide}
          associatedEntityIds={associatedEntityIds}
          setAssociatedEntityIds={setAssociatedEntityIds}
          setHoveredEntity={setHoveredEntity}
        />
      </Pane>
    </>
  )
}
