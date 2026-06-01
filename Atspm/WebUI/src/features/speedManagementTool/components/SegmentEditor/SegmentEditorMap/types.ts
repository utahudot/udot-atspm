// SegmentEditorMap/types.ts
export interface UdotRoute {
  geometry: {
    paths: number[][][]
  }
  attributes: {
    ROUTE_ID: string
    ROUTE_DIRECTION: string
    BEG_MILEAGE: number
    END_MILEAGE: number
    ROUTE_DESC: string
  }
}

export interface SegmentProperties {
  entities?: string[]
  // Add other properties as needed
}
