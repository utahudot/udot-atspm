// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - removeAuditFields.ts
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
type AuditKeys = 'created' | 'createdBy' | 'modified' | 'modifiedBy'

export const removeAuditFields = <
  T extends {
    created?: unknown
    createdBy?: unknown
    modified?: unknown
    modifiedBy?: unknown
  },
>(
  data: T
): Omit<T, AuditKeys> => {
  const { created, createdBy, modified, modifiedBy, ...rest } = data
  return rest
}
