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
