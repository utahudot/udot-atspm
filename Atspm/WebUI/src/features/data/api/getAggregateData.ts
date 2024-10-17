// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getAggregateData.ts
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
import { usePostRequest } from '@/hooks/usePostRequest'
import { reportsAxios } from '@/lib/axios'
import { AxiosHeaders } from 'axios'

const route = '/Aggregation/getReportData'
const headers: AxiosHeaders = new AxiosHeaders({
  'Content-Type': 'application/json',
})

export function usePostAggregateData() {
  const mutation = usePostRequest({
    url: route,
    axiosInstance: reportsAxios,
    headers,
  })
  return mutation
}
