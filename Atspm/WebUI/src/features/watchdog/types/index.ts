// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - index.ts
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
export type Watchdog = {
  Id: number
  LocationId: number
  LocationIdentifier: number
  Timestamp: string
  ComponentType: number
  ComponentId: number
  IssueType: number
  Details: string
  Phase: string | null
  Name: string
}


//intefaces for watchdog Dashobard 
export enum IssueType {
  RecordCount = 1,
  LowDetectorHits = 2,
  StuckPed = 3,
  ForceOffThreshold = 4,
  MaxOutThreshold = 5,
  UnconfiguredApproach = 6,
  UnconfiguredDetector = 7,
}

interface FirmwareData {
  name: string;
  counts: number;
}

interface ModelData {
  name: string;
  firmware: FirmwareData[];
}

interface ProductData {
  name: string;
  model: ModelData[];
}

export interface IssueTypeGroup {
  issueType: IssueType;
  products: ProductData[];
  name: string;
}

interface HardwareData {
  name: string;
  counts: number;
}

interface DetectionTypeGroup {
  detectionType: number;
  hardware: HardwareData[];
  name: string;
}

interface IssueTypeData {
  name: string;
  counts: number;
}

interface ControllerFirmwareData {
  name: string;
  issueType: IssueTypeData[];
}

export interface ControllerModelData {
  name: string;
  firmware: ControllerFirmwareData[];
}

export interface ControllerTypeGroup {
  name: string;
  model: ControllerModelData[];
}

export interface WatchdogDashboardData {
  issueTypeGroup: IssueTypeGroup[];
  detectionTypeGroup: DetectionTypeGroup[];
  controllerTypeGroup: ControllerTypeGroup[];
}


export interface DeviceCount {
  manufacturer: string;
  model: string;
  firmware: string;
  count: number;
}

export interface DetectionTypeCount {
  id: string;
  count: number;
}