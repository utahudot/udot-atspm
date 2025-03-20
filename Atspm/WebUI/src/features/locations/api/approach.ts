// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - approach.ts
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
import { Approach } from '@/features/locations/types'
import { useDeleteRequest } from '@/hooks/useDeleteRequest'
import { useGetRequest } from '@/hooks/useGetRequest'
import { usePostRequest } from '@/hooks/usePostRequest'
import { configAxios } from '@/lib/axios'
import { ApiResponse } from '@/types'
import { AxiosHeaders } from 'axios'
import Cookies from 'js-cookie'

const token = Cookies.get('token')
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${token}`,
})

export function useGetApproach(id: number) {
  return useGetRequest<ApiResponse<Approach>>({
    route: `/Approach/${id}/?expand=detectors($expand=DetectionTypes, detectorComments)`,
    headers,
    axiosInstance: configAxios,
  })
}

export function useCreateApproach() {
  const mutation = usePostRequest({
    url: '/Approach',
    axiosInstance: configAxios,
    headers,
  })
  return mutation
}

export function useEditApproach() {
  const mutation = usePostRequest({
    url: '/UpsertApproach',
    axiosInstance: configAxios,
    headers,
  })
  return mutation
}

export function useDeleteApproach() {
  const mutation = useDeleteRequest({
    url: '/Approach',
    axiosInstance: configAxios,
    headers,
  })
  return mutation
}
