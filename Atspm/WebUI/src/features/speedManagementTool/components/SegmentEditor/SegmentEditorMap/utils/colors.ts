// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - colors.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion

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
