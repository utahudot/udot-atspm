import {
  mapStringBooleansToBoolean,
  toolTypeApiMap,
} from '@/features/charts/api/getTools'
import { ToolOptions, ToolType } from '@/features/charts/common/types'
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'
import { RawLinkPivotPcdData, RawLinkPivotPcdResponse } from '../types'

type QueryFnType = typeof getLinkPivotPcdCharts

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

type UseToolsOptions = BaseOptions & {
  toolType: ToolType.LpPcd
  toolOptions: ToolOptions
}

export const getLinkPivotPcdCharts = async (
  type: ToolType.LpPcd,
  options: ToolOptions
): Promise<RawLinkPivotPcdResponse> => {
  const endpoint = toolTypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)
  const response: RawLinkPivotPcdData = await reportsAxios.post(
    endpoint,
    transformedOptions
  )

  return { type, data: response }
}

export const useLinkPivotPcdCharts = ({
  toolType,
  toolOptions,
  config,
}: UseToolsOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: [toolType, toolOptions],
    queryFn: () => getLinkPivotPcdCharts(toolType, toolOptions),
  })
}
