// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getChartDefaults.ts
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
