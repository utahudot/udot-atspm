// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - editUsers.ts
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
import { identityRequest } from '@/lib/axios'
import { useMutation } from 'react-query'

interface EditUsersData {
  firstName: string
  lastName: string
  agency: string
  email: string
  userName: string
  userId: string
  fullName: string
  roles: string[]
  areaIds: number[]
  regionIds: number[]
  jurisdictionIds: number[]
}

export function useEditUsers() {
  return useMutation({
    mutationFn: async (data: EditUsersData) =>
      identityRequest({
        url: '/Users/update',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        data,
      }),
  })
}
