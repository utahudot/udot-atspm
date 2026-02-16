import { RawLinkPivotForTsdData } from '@/features/tools/link-pivot/types'
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { dateToTimestamp } from '@/utils/dateTime'
import { useQuery } from 'react-query'
import { mapStringBooleansToBoolean, toolTypeApiMap } from '../../api/getTools'
import { ToolOptions, ToolType } from '../../common/types'

type QueryFnType = typeof getLinkPivotForTsd

type BaseOptions = {
  config?: QueryConfig<QueryFnType>
}

type UseLinkPivotForTsdOptions = BaseOptions & {
  toolType: ToolType
  toolOptions: ToolOptions
}

export const getLinkPivotForTsd = async (
  type: ToolType,
  options: ToolOptions
): Promise<RawLinkPivotForTsdData[]> => {
  const endpoint = toolTypeApiMap[type]

  const transformedOptions = mapStringBooleansToBoolean(options)

  transformedOptions.start = dateToTimestamp(transformedOptions.start as Date)
  transformedOptions.end = dateToTimestamp(transformedOptions.end as Date)

  const response = (await reportsAxios.post(
    endpoint,
    transformedOptions
  )) as RawLinkPivotForTsdData[]

  return response
}

export const useLinkPivotForTsd = ({
  toolType,
  toolOptions,
  config,
}: UseLinkPivotForTsdOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: false,
    queryKey: ['tools', toolType, toolOptions],
    queryFn: () => getLinkPivotForTsd(toolType, toolOptions),
  })
}
