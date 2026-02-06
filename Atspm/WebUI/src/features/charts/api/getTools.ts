// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getTools.ts
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
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { dateToTimestamp } from '@/utils/dateTime'
import { useQuery } from 'react-query'
import { ToolOptions, ToolType } from '../common/types'
import {
  RawTimeSpaceDiagramResponse,
  TimeSpaceResponseData,
} from '../timeSpaceDiagram/shared/types'

export const toolTypeApiMap: Record<ToolType, string> = {
  [ToolType.TimeSpaceHistoric]: '/TimeSpaceDiagram/GetReportData',
  [ToolType.TimeSpaceAverage]: '/TimeSpaceDiagramAverage/GetReportData',
  [ToolType.LinkPivot]: '/LinkPivot/GetReportData',
  [ToolType.LpPcd]: '/LinkPivot/getPcdData',
}

type QueryFnType = typeof getTools

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

type UseToolsOptions = BaseOptions & {
  toolType: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
  toolOptions: ToolOptions
}

type StringBooleanMap = Record<string, boolean | string | Date>

export const mapStringBooleansToBoolean = (obj: ToolOptions) => {
  return Object.entries(obj).reduce<StringBooleanMap>((acc, [key, value]) => {
    // Check if the value is exactly "true" or "false" (case-insensitive)
    if (typeof value === 'string') {
      if (value.toLowerCase() === 'true') {
        acc[key] = true
      } else if (value.toLowerCase() === 'false') {
        acc[key] = false
      } else {
        // If it's a string but not "true" or "false", keep it unchanged
        acc[key] = value
      }
    } else {
      // If the value is not a string, keep it unchanged
      acc[key] = value
    }
    return acc
  }, {})
}

export const getTools = async (
  type: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage,
  options: ToolOptions
): Promise<RawTimeSpaceDiagramResponse> => {
  const endpoint = toolTypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)

  if (type === ToolType.TimeSpaceHistoric) {
    transformedOptions.start = dateToTimestamp(transformedOptions.start as Date)
    transformedOptions.end = dateToTimestamp(transformedOptions.end as Date)
  }

  const response = (await reportsAxios.post(
    endpoint,
    transformedOptions
  )) as TimeSpaceResponseData

  return {
    type,
    data: response,
  }
}

export const useTimeSpaceCall = ({
  toolType,
  toolOptions,
  config,
}: UseToolsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: ['tools', toolType, toolOptions],
    queryFn: () => getTools(toolType, toolOptions),
  })
}
