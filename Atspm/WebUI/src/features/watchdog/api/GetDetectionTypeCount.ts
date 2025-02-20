// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - GetDetectionTypeCount.ts
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
import { DetectionTypeCount } from '@/features/watchdog/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType } from '@/lib/react-query'
import { useQuery } from 'react-query'

const getDetectionTypeCount = async (
  date: string
): Promise<DetectionTypeCount[]> => {
  const response = await configAxios.get(
    `/Location/GetDetectionTypeCount?date=${date}`
  )
  return response.value
}

type QueryFnType = typeof getDetectionTypeCount

export const useGetDetectionTypeCount = (date: string) => {
  //date must beformmated yyyy-MM-dd or MM-dd-yyyy
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['detectionTypeCount', date],
    queryFn: () => getDetectionTypeCount(date),
  })
}
