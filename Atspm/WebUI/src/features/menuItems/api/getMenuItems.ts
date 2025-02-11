// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getMenuItems.ts
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
import { MenuItems } from '@/features/links/types/linkDto'
import { useGetRequest } from '@/hooks/useGetRequest'
import { configAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const route = '/MenuItems'
const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetMenuItems() {
  return useGetRequest<MenuItems>({
    route,
    headers,
    axiosInstance: configAxios,
  })
}
