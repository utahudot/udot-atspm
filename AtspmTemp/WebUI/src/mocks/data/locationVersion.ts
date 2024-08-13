// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - locationVersion.ts
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
export const locationVersions = {
  '@odata.context':
    'https://localhost:44315/api/v1/$metadata#Locations(jurisdiction(),region())',
  value: [
    {
      latitude: 40.62398502,
      longitude: -111.9387819,
      note: 'Initial',
      primaryName: 'Redwood Road',
      locationIdentifier: '7115',
      secondaryName: '7000 South',
      jurisdictionId: 35,
      chartEnabled: true,
      versionAction: 'Initial',
      start: '2011-01-01T00:00:00-07:00',
      pedsAre1to1: true,
      locationTypeId: 1,
      regionId: 2,
      id: 1680,
      jurisdiction: {
        countyParish: null,
        name: 'UDOT_Reg_2',
        mpo: 'WFRC',
        otherPartners: null,
        id: 35,
        userJurisdictions: [],
      },
      region: {
        description: 'Region 2',
        id: 2,
        userRegions: [],
      },
    },
  ],
}
