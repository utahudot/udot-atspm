// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - impact.ts
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

export interface Impact {
    id?: string | null;
    description?: string | null;
    start?: Date | null;
    end?: Date | null;
    startMile: number;
    endMile: number;
    impactTypeIds?: string[] | null;
    impactTypes?: ImpactType[] | null;
    createdOn?: Date | null;
    createdBy?: string | null;
    updatedOn?: Date | null;
    updatedBy?: string | null;
    deletedOn?: Date | null;
    deletedBy?: string | null;
    segmentIds?: string[] | null;
    impactTypeNames?:string
}

export interface ImpactType {
    id?: string | null;
    name: string;
    description?: string | null;
}
