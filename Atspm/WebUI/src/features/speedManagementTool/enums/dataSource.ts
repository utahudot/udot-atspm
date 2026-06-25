// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - dataSource.ts
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
export enum DataSource {
  ATSPM = 1,
  PeMS = 2,
  ClearGuide = 3,
}

export function getDataSourceName(dataSource: number | undefined) {
  switch (dataSource) {
    case 1:
      return 'ATSPM'
    case 2:
      return 'PeMS'
    case 3:
      return 'ClearGuide'
  }
}
