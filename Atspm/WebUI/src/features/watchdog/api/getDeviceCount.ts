// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getDeviceCount.ts
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
import { DeviceCount } from '@/features/watchdog/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType } from '@/lib/react-query'
import { useQuery } from 'react-query'

const getDeviceCount = async (): Promise<DeviceCount[]> => {
  const response = await configAxios.get('/Device/GetActiveDevicesCount')
  return response
}

type QueryFnType = typeof getDeviceCount

export const useGetDeviceCount = () => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['deviceCount'],
    queryFn: getDeviceCount,
  })
}
