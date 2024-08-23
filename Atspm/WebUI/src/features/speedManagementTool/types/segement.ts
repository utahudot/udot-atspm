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
    shape?: any;
    shapeWKT?: string;
    alternateIdentifier?: string;
    accessCategory?: string;
    offset?: number;
    routeEntities?: SegmentEntity[];
  }
  
  export interface SegmentEntity {
    entityId: number;
    sourceId: number;
    segmentId: string;
    entityType: string;
    length: number;
    segment?: Segment;
  }