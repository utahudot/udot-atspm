import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'
import { RawToolResponse, ToolOptions, ToolType } from '../common/types'
import { TransformedToolResponse } from '../types'
import { transformToolData } from './transformData'

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
  toolType: ToolType
  toolOptions: ToolOptions
}

type StringBooleanMap = Record<string, boolean | string>

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
  type: ToolType,
  options: ToolOptions
): Promise<TransformedToolResponse> => {
  const endpoint = toolTypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)
  const response = await reportsAxios.post(endpoint, transformedOptions)

  return transformToolData({
    type: type,
    data: response,
  } as unknown as RawToolResponse)
}

export const useTools = ({
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
