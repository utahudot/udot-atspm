// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getLinkPivotAdjustments.ts
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
import {
  mapStringBooleansToBoolean,
  toolTypeApiMap,
} from '@/features/charts/api/getTools'
import { ToolOptions, ToolType } from '@/features/charts/common/types'
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'
import { RawLinkPivotData } from '../types'

type QueryFnType = typeof getLinkPivotAdjustment

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

type UseToolsOptions = BaseOptions & {
  toolType: ToolType
  toolOptions: ToolOptions
}

export const getLinkPivotAdjustment = async (
  type: ToolType,
  options: ToolOptions
): Promise<RawLinkPivotData> => {
  const endpoint = toolTypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)
  const response: RawLinkPivotData = await reportsAxios.post(
    endpoint,
    transformedOptions
  )

  return response
}

export const useLinkPivotAdjustment = ({
  toolType,
  toolOptions,
  config,
}: UseToolsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: [ToolType.LinkPivot, toolOptions],
    queryFn: () => getLinkPivotAdjustment(toolType, toolOptions),
  })
}
