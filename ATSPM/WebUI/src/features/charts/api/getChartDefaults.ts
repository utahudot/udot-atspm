import { ChartType } from '@/features/charts/common/types'
import { ChartDefaults, Default } from '@/features/charts/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

const normalizeString = (str: string) => str.replace(/\s+/g, '').toLowerCase()

const determineChartType = (chartName: string): ChartType | 'Unknown' => {
  const normalizedChartName = normalizeString(chartName)

  const chartTypeKey = Object.keys(ChartType).find(
    (key) =>
      normalizeString(ChartType[key as keyof typeof ChartType]) ===
      normalizedChartName
  )

  if (chartTypeKey) {
    return ChartType[chartTypeKey as keyof typeof ChartType] as ChartType
  }

  return 'Unknown'
}

export const getChartDefaults = async (): Promise<
  ApiResponse<ChartDefaults>
> => {
  const response = await configAxios.get<ApiResponse<ChartDefaults[]>>(
    '/MeasureType?expand=measureOptions'
  )

  const enhancedData = response.value.map((chart: ChartDefaults) => ({
    ...chart,
    chartType: determineChartType(chart.name),
    measureOptions: chart.measureOptions.reduce(
      (acc, current) => {
        acc[current.option] = current
        return acc
      },
      {} as Record<string, Default>
    ),
  }))

  return {
    ...response,
    value: enhancedData,
  }
}

type QueryFnType = typeof getChartDefaults

type UseChartDefaultsOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useChartDefaults = ({ config }: UseChartDefaultsOptions = {}) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['chartdefaults'],
    queryFn: getChartDefaults,
  })
}
