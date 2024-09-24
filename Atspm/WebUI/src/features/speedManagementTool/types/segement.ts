export interface Segment {
  id: string;
  udotRouteNumber: string;
  startMilePoint: number;
  endMilePoint: number;
  functionalType: string;
  name: string;
  direction?: string;
  speedLimit: number;
  region?: string;
  city?: string;
  county?: string;
  shape?: GeoJSON.Geometry | any;
  shapeWKT?: string;
  alternateIdentifier?: string | null;
  accessCategory?: string;
  offset?: number;
  routeEntities?: SegmentEntity[] | null;
}  
  export interface SegmentEntity {
    entityId: number;
    sourceId: number;
    segmentId: string;
    entityType: string;
    length: number;
    segment?: Segment;
  }