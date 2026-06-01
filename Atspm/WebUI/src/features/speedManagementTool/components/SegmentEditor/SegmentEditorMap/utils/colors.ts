// Define a standalone palette of colors you can tweak independently
export enum PALETTE {
  Red = '#dc4b00',
  RedHover = '#9a3400',
  Blue = '#3c6ccc',
  BlueHover = '#2a4b8e ',
  Yellow = '#d9dc00',
  YellowHover = '#979A00',
  LightGreen = '#BDE866',
  Green = '#4caf50',
  GreenHover = '#84A247',
  Purple = '#986ba1',
  PurpleHover = '#6A4A70',
  Gray = 'lightgrey',
}

export type RouteType =
  | 'Lrs'
  | 'Draft'
  | 'Nearby'
  | 'ClearGuide'
  | 'Default'
  | 'Selected'

// Main and hover colors for each route type
export interface RouteColors {
  main: string
  hover: string
}

export const ROUTE_COLORS: Record<RouteType, RouteColors> = {
  Draft: {
    main: PALETTE.Blue,
    hover: PALETTE.Red,
  },
  Lrs: {
    main: PALETTE.Red,
    hover: PALETTE.RedHover,
  },
  Nearby: {
    main: PALETTE.Purple,
    hover: PALETTE.PurpleHover,
  },
  ClearGuide: {
    main: PALETTE.LightGreen,
    hover: PALETTE.GreenHover,
  },
  Default: {
    main: PALETTE.Gray,
    hover: PALETTE.Gray,
  },
  Selected: {
    main: PALETTE.Blue,
    hover: PALETTE.BlueHover,
  },
}
